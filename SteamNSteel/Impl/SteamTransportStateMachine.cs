using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using SteamNSteel.Impl.Jobs;
using SteamNSteel.Jobs;

namespace SteamNSteel.Impl
{
	public class SteamTransportStateMachine : INotifyTopologyCalculated
	{
		private readonly List<SteamTransportTopology> ActiveTopologies = new List<SteamTransportTopology>();
		private readonly List<SteamTransportLocation> PendingTopologyChanges = new List<SteamTransportLocation>();
		private readonly List<SteamTransportTopology> NewTopologies = new List<SteamTransportTopology>();
		private readonly object PendingTopologyListLock = new object();
        private long topologyGeneration = 0;

		public void OnTick()
		{
			if (PendingTopologyChanges.Count > 0)
			{
				ProcessTopologyChanges();
			}
			else
			{
				ProcessIdealCondensation();
			}
		}

		private void ProcessTopologyChanges()
		{
			topologyGeneration++;
			Console.WriteLine("Processing Topology Changes: Generation #{0}", topologyGeneration);
			lock (PendingTopologyListLock)
			{
				foreach (var steamTransportLocation in PendingTopologyChanges)
				{
					TheMod.JobManager.AddBackgroundJob(new RecalculateTopologyJob(steamTransportLocation, topologyGeneration, this));
				}
			}
		}

		public void AddPendingTopologyChange(SteamTransportLocation steamTransportLocation)
		{
			lock (PendingTopologyListLock)
			{
				PendingTopologyChanges.Add(steamTransportLocation);
			}
		}

		internal void DeactivateTopology(SteamTransportTopology topology)
		{
			ActiveTopologies.Remove(topology);
		}

		public void TopologyCalculated(RecalculateTopologyJob.Result result)
		{
			Console.WriteLine("Topology calculated {0}", result.NewTopology);
			bool moveToNextState = false;
			
			lock (PendingTopologyListLock)
			{
				NewTopologies.Add(result.NewTopology);
				PendingTopologyChanges.RemoveAt(PendingTopologyChanges.Count - 1);
				if (!PendingTopologyChanges.Any())
				{
					moveToNextState = true;
				}
			}

			if (moveToNextState)
			{
				Console.WriteLine("Cleaning Topologies");
				foreach (var topology in NewTopologies)
				{
					if (topology.GetTransports().Any())
					{
						ActiveTopologies.Add(topology);
					}
				}
				NewTopologies.Clear();

				Console.WriteLine("Processing Ideal Condensation");
				foreach (var topology in ActiveTopologies)
				{
					Console.WriteLine("Active Topology: {0}", topology);
				}
				ProcessIdealCondensation();
			}
		}

		private void ProcessIdealCondensation()
		{
			
		}
	}

	public interface INotifyTopologyCalculated
	{
		void TopologyCalculated(RecalculateTopologyJob.Result result);
	}
}
