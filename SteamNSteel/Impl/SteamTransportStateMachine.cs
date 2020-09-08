using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Steam.API;
using SteamNSteel.Impl.Jobs;

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

		public void onTick()
		{
			processTransports();
		}

		private void processTransports()
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

			expectedJobs = jobs.Count;
			foreach (ProcessTransportJob job in jobs)
			{
				TheMod.JobManager.addBackgroundJob(job);
			}

			expectingJobs = true;
		}

		public void postTick()
		{
			if (expectingJobs)
			{
//				Console.WriteLine($"{TheMod.CurrentTick} Waiting postTick");
				barrier.SignalAndWait();
				//Console.WriteLine($"{TheMod.CurrentTick} finished postTick");
			}
		}

		private void finished()
		{
			//Console.WriteLine($"{TheMod.CurrentTick} Waiting PostJobs");
			barrier.SignalAndWait();
			//Console.WriteLine($"{TheMod.CurrentTick} Released PostJobs");
		}

		internal void addTransport(SteamTransport transport)
		{
			TheMod.JobManager.addPreTickJob(new RegisterTransportJob(this, transport));
		}

		internal void removeTransport(SteamTransport transport)
		{
			TheMod.JobManager.addPreTickJob(new UnregisterTransportJob(this, transport));
		}

		internal void addTransportInternal(SteamTransport transport)
		{
			SteamTransportLocation steamTransportLocation = transport.getTransportLocation();
			Console.WriteLine($"{TheMod.CurrentTick} Adding Transport {steamTransportLocation}");
			TransientData.Add(transport, new SteamTransportTransientData(transport));

			foreach (Direction direction in Direction.VALID_DIRECTIONS)
			{
				if (!transport.canConnect(direction)) continue;
				SteamTransportLocation altSteamTransportLocation = steamTransportLocation.offset(direction);
				
				ProcessTransportJob foundTransportJob;
                if (!IndividualTransportJobs.TryGetValue(altSteamTransportLocation, out foundTransportJob)) continue;
				SteamTransport foundTransport = foundTransportJob._transport;
				Direction oppositeDirection = direction.getOpposite();
				if (!foundTransport.canConnect(oppositeDirection)) continue;

				transport.setAdjacentTransport(direction, foundTransport);
				foundTransport.setAdjacentTransport(oppositeDirection, transport);
			}

			IndividualTransportJobs.Add(steamTransportLocation, new ProcessTransportJob(transport, this, _steamNSteelConfiguration));
		}

		internal void removeTransportInternal(SteamTransport transport)
		{
			IndividualTransportJobs.Remove(transport.getTransportLocation());
			TransientData.Remove(transport);

			foreach (Direction direction in Direction.VALID_DIRECTIONS)
			{
				SteamTransport adjacentTransport = (SteamTransport)transport.getAdjacentTransport(direction);
				if (adjacentTransport == null) continue;

				adjacentTransport.setAdjacentTransport(direction.getOpposite(), null);
			}
		}

		internal SteamTransportTransientData getJobDataForTransport(ISteamTransport processTransportJob)
		{
			return TransientData[processTransportJob];
		}

		public void jobComplete()
		{
			if (Interlocked.Decrement(ref expectedJobs) == 0)
			{
				finished();
			}
		}
	}

	internal interface INotifyTransportJobComplete
	{
		void jobComplete();
	}
}
