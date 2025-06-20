﻿using SfaChatGraph.Server.Services.EventService;

namespace SfaChatGraph.Server.Services.ChatService.Events
{
	public class ChatEvent : IEvent
	{
		private static readonly IComparer<DateTime> comparer = Comparer<DateTime>.Default;
		public DateTime TimeStamp { get; init; } = DateTime.UtcNow;
		public Guid ChatId { get; init; }
		public string Activity { get; init; }
		public string? Detail { get; init; }
		public string? Trace { get; init; }
		public bool Done { get; init; }


		public static ChatEvent CActivity(Guid chatId, string activity, string? detail = null, string? trace = null) => new ChatEvent
		{
			ChatId = chatId,
			Detail = detail,
			Activity = activity,
			Done = false,
			Trace = trace
		};

		public static ChatEvent CDone(Guid chatId) => new ChatEvent
		{
			ChatId = chatId,
			Activity = null,
			Done = true
		};



		public int CompareTo(IEvent other) => other switch
		{
			ChatEvent otherEvent => comparer.Compare(TimeStamp, otherEvent.TimeStamp),
			_ => throw new ArgumentException($"Cannot compare {GetType()} with {other.GetType()}")
		};
	}
}
