﻿using AwosFramework.ApiClients.Jupyter.WebSocket.Jupyter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AwosFramework.ApiClients.Jupyter.WebSocket.Jupyter.Models.Messages.Control
{
	[MessageType("interrupt_request", ChannelKind.Control)]
	public class KernelInterruptRequest
	{
	}
}
