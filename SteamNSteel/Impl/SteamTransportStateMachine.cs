using System.Collections.Generic;
using System.Threading;
using SteamNSteel.Impl.Jobs;

namespace SteamNSteel.Impl
{
	public class SteamTransportStateMachine
	{
		private IDictionary<SteamTransportLocation, ProcessTransportJob> IndividualTransportJobs = new Dictionary<SteamTransportLocation, ProcessTransportJob>();

		private Barrier barrier = new Barrier(2);

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
			IndividualTransportJobs.Add(result.GetTransportLocation(), new ProcessTransportJob(result));
		}
	}
}
