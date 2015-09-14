using System.Collections.Generic;
using System.Threading;
using SteamNSteel.Impl.Jobs;

namespace SteamNSteel.Impl
{
	public class SteamTransportStateMachine
	{
		public SteamTransportStateMachine()
		{
			_steamNSteelConfiguration = new SteamNSteelConfiguration();
		}

		private IDictionary<SteamTransportLocation, ProcessTransportJob> IndividualTransportJobs = new Dictionary<SteamTransportLocation, ProcessTransportJob>();

		private Barrier barrier = new Barrier(2);
		private SteamNSteelConfiguration _steamNSteelConfiguration;

		public void OnTick()
		{
			Finished();
		}

		private void ProcessTransports()
		{
			throw new System.NotImplementedException();
		}

		public void PostTick()
		{
			barrier.SignalAndWait();
		}

		private void Finished()
		{
			barrier.SignalAndWait();
		}

		internal void AddTransport(SteamTransport result)
		{
			
			IndividualTransportJobs.Add(result.GetTransportLocation(), new ProcessTransportJob(result, _steamNSteelConfiguration));
		}
	}
}
