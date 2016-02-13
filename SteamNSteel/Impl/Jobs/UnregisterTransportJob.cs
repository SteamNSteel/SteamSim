using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SteamNSteel.Jobs;

namespace SteamNSteel.Impl.Jobs
{
	//FIXME: This is probably not required, and is only here to proxy creation requests to the tick thread.
	internal class UnregisterTransportJob : IJob
	{
		private readonly SteamTransportStateMachine _steamTransportStateMachine;
		private readonly SteamTransport _transport;

		public UnregisterTransportJob(SteamTransportStateMachine steamTransportStateMachine, SteamTransport transport)
		{
			_steamTransportStateMachine = steamTransportStateMachine;
			_transport = transport;
		}

		public void execute()
		{
			_steamTransportStateMachine.RemoveTransportInternal(_transport);
		}
	}
}
