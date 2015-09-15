using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Steam.API;
using SteamNSteel.Impl.Jobs;
using SteamNSteel.Jobs;

namespace SteamNSteel.Impl
{
	public class SteamTransportStateMachine : INotifyTransportJobComplete
	{
		public SteamTransportStateMachine()
		{
			_steamNSteelConfiguration = new SteamNSteelConfiguration();
		}

		private IDictionary<SteamTransportLocation, ProcessTransportJob> IndividualTransportJobs = new Dictionary<SteamTransportLocation, ProcessTransportJob>();
		private IDictionary<ISteamTransport, SteamTransportTransientData> TransientData = new Dictionary<ISteamTransport, SteamTransportTransientData>();
		private Barrier barrier = new Barrier(2);
		private SteamNSteelConfiguration _steamNSteelConfiguration;
		private int expectedJobs;
		private bool expectingJobs;

		public void OnTick()
		{
			ProcessTransports();
		}

		private void ProcessTransports()
		{
			if (expectedJobs > 0)
			{
				throw new InvalidOperationException("Attempt to run a second tick with already outstanding jobs?");
			}
			var jobs = IndividualTransportJobs.Values;
			if (!jobs.Any())
			{
				expectingJobs = false;
				return;
			}
			
			foreach (var job in jobs)
			{
				Interlocked.Increment(ref expectedJobs);
				TheMod.JobManager.AddBackgroundJob(job);
			}

			expectingJobs = true;
		}

		public void PostTick()
		{
			if (expectingJobs)
			{
				barrier.SignalAndWait();
			}
		}

		private void Finished()
		{
			barrier.SignalAndWait();
		}

		internal void AddTransport(SteamTransport transport)
		{
			
			IndividualTransportJobs.Add(transport.GetTransportLocation(), new ProcessTransportJob(transport, this, _steamNSteelConfiguration));
			TransientData.Add(transport, new SteamTransportTransientData(transport));
		}

		internal SteamTransportTransientData GetJobDataForTransport(ISteamTransport processTransportJob)
		{
			return TransientData[processTransportJob];
		}

		public void JobComplete()
		{
			if (Interlocked.Decrement(ref expectedJobs) == 0)
			{
				Finished();
			}
		}
	}

	internal interface INotifyTransportJobComplete
	{
		void JobComplete();
	}
}
