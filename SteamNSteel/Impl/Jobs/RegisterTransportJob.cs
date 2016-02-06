using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SteamNSteel.Jobs;

namespace SteamNSteel.Impl.Jobs
{
	//FIXME: This is probably not required, and is only here to proxy creation requests to the tick thread.
	internal class RegisterTransportJob : IJob
	{
		private readonly SteamTransportStateMachine _steamTransportStateMachine;
		private readonly SteamTransport _transport;

		public RegisterTransportJob(SteamTransportStateMachine steamTransportStateMachine, SteamTransport transport)
		{
			_steamTransportStateMachine = steamTransportStateMachine;
			_transport = transport;
		}

		public void Execute()
		{
			_steamTransportStateMachine.AddTransportInternal(_transport);
		}
	}
}
