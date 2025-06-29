﻿using AwosFramework.Generators.FunctionCalling;
using Lucene.Net.Util;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OpenAI.Chat;
using SfaChatGraph.Server.Config;
using SfaChatGraph.Server.Models;
using SfaChatGraph.Server.RDF;
using SfaChatGraph.Server.Services.ChatHistoryService;
using SfaChatGraph.Server.Services.ChatService;
using SfaChatGraph.Server.Services.ChatService.Events;
using SfaChatGraph.Server.Services.EventService;
using SfaChatGraph.Server.Utils;
using SfaChatGraph.Server.Utils.ServiceCollection;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Web;
using VDS.RDF;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Writing.Formatting;

namespace SfaChatGraph.Server.Controllers
{
	[ApiController]
	[Route("/api/v1/chat")]
	public class ChatController : ControllerBase
	{
		private readonly IChatHistoryService _chatHistoryService;
		private readonly IServiceProvider _serviceProvider;
		private readonly IChatService _chatService;
		private readonly IGraphRag _graphDb;
		private readonly ChatServiceEventService _eventService;
		private ILogger _logger;

		public ChatController(IGraphRag graphDb, IChatService chatService, ILoggerFactory loggerFactory, IChatHistoryService chatHistoryService, ChatServiceEventService eventService, IServiceProvider serviceProvider = null)
		{
			_graphDb=graphDb;
			_logger=loggerFactory.CreateLogger<ChatController>();
			_chatService = chatService;
			_chatHistoryService=chatHistoryService;
			_eventService=eventService;
			_serviceProvider=serviceProvider;
		}

		[HttpGet("describe")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public async Task<IActionResult> DescribeAsync([FromQuery] string subject)
		{
			var graph = await _graphDb.DescribeAsync(subject);
			return Ok(graph);
		}

		[HttpGet("history/{id}")]
		[ProducesResponseType<ApiMessage[]>(StatusCodes.Status200OK)]
		public async Task<IActionResult> GetHistoryAsync(Guid id, [FromQuery] bool loadBlobs = false)
		{
			var history = await _chatHistoryService.GetChatHistoryAsync(id, loadBlobs);
			return Ok(history.Messages);
		}

		[HttpGet("tool-data/{id}")]
		[ProducesResponseType<FileResult>(StatusCodes.Status200OK)]
		public async Task<IActionResult> GetToolDataAsync(Guid id, [FromQuery] string download = null)
		{
			if (_chatHistoryService.SupportsToolData == false)
				return NotFound();

			var data = await _chatHistoryService.GetToolDataAsync(id);
			if (data == null)
				return NotFound();

			if (string.IsNullOrEmpty(download) == false)
				data.FileDownloadName = HttpUtility.UrlDecode(download);

			Response.Headers.CacheControl = "public, max-age=31536000, immutable"; // 1 year
			Response.Headers.Expires = DateTime.UtcNow.AddYears(1).ToString("R");
			return data;
		}

		[HttpPost("complete/{id}")]
		[ProducesResponseType<ApiMessage[]>(StatusCodes.Status200OK)]
		public async Task<IActionResult> ChatAsync([FromBody] ApiChatRequest chat, Guid id, [FromQuery] Guid? eventChannel)
		{
			var service = _chatService;
			if (chat.AiConfig != null)
				service = _serviceProvider.GetFromConfig<IChatService, AiConfig>(chat.AiConfig);

			if(service == null)
				return NotFound($"No service found for {chat.AiConfig?.Implementation}");

			IEventSink<ChatEvent> sink = null;
			if (eventChannel.HasValue)
				sink = _eventService.GetChannel(eventChannel.Value);

			chat.Message.TimeStamp = DateTime.UtcNow;
			await sink?.PushAsync(ChatEvent.CActivity(id, "Loading chat history"));
			var history = await _chatHistoryService.GetChatHistoryAsync(id);
			var ctx = service.CreateContext(id, sink, history.Messages);
			ctx.AddUserMessage(chat.Message);
			var response = await service.CompleteAsync(ctx, chat.Temperature, chat.MaxErrors);
			if (response.Success == false)
				return StatusCode(500, response.Error);

			// append created to history since created contains user message as well
			await _chatHistoryService.AppendAsync(id, ctx.Created);

			// return only newly generated messages, since client already has user message
			return Ok(ctx.Created.Skip(1));
		}
	}
}
