﻿using AwosFramework.ApiClients.Jupyter.WebSocket.Jupyter;
using AwosFramework.ApiClients.Jupyter.WebSocket.Jupyter.Models.Messages;
using AwosFramework.ApiClients.Jupyter.WebSocket.Jupyter.Protocol;
using AwosFramework.ApiClients.Jupyter.WebSocket.Terminal;
using AwosFramework.ApiClients.Jupyter.WebSocket.Terminal.Protocol;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AwosFramework.ApiClients.Jupyter.WebSocket.Base
{
	public delegate Task SendDeletegate(ReadOnlyMemory<byte> data, bool lastMessage);
	public interface IProtocol<TRes, TError> : IDisposable
	{

		public static FrozenDictionary<string, Type> Implementations { get; } = GetImplementations();
		private static FrozenDictionary<string, Type> GetImplementations()
		{
			var dict = new Dictionary<string, Type>();
			var types = typeof(IProtocol<TRes, TError>).Assembly.GetTypes();
			foreach (var type in types)
			{
				if (type.IsClass && type.IsAbstract == false && type.IsAssignableTo(typeof(IProtocol<TRes, TError>)))
				{
					var attributes = type.GetCustomAttributes<ProtocolImplementationAttribute>();
					foreach (var attribute in attributes)
					{
						dict.Add(attribute.ProtocolName, type);
						if (attribute.IsDefault)
							dict.Add(string.Empty, type);
					}
				}
			}

			return dict.ToFrozenDictionary();
		}

		public static IProtocol<TRes, TError> CreateInstance(string? protocolName, params object[] ctorParams)
		{
			protocolName ??= string.Empty; // default protocol name
			if (Implementations.TryGetValue(protocolName, out var type) == false)
				throw new ArgumentException($"Protocol {protocolName} not found", nameof(protocolName));

			var instance = Activator.CreateInstance(type, ctorParams);
			if (instance is not IProtocol<TRes, TError> protocol)
				throw new ArgumentException($"Protocol {protocolName} is not a valid protocol", nameof(protocolName));

			return protocol;
		}

		public Task<ProtocolResult<TRes, TError>> ReadAsync(Memory<byte> memory, bool endOfMessage);
		public Task<long> SendAsync(TRes toSend, SendDeletegate sender);

	}

	public interface ITerminalProtocol : IProtocol<TerminalMessage, TerminalError>
	{
		public static ITerminalProtocol CreateInstance(string? protocolName, TerminalWebsocketClientOptions options) => (ITerminalProtocol)IProtocol<TerminalMessage, TerminalError>.CreateInstance(protocolName, options);
	}

	public interface IJupyterProtocol : IProtocol<JupyterMessage, JupyterError>
	{
		public static IJupyterProtocol CreateInstance(string? protocolName, JupyterWebsocketOptions options) => (IJupyterProtocol)IProtocol<JupyterMessage, JupyterError>.CreateInstance(protocolName, options);
	}
}