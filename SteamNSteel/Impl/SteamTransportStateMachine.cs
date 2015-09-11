using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using SteamNSteel.Impl.Jobs;
using SteamNSteel.Jobs;

namespace SteamNSteel.Impl
{
	public class SteamTransportStateMachine : INotifyTopologyCalculated, INotifyIdealCondenstationComplete
	{
		private readonly List<SteamTransportTopology> _activeTopologies = new List<SteamTransportTopology>();
		private readonly List<SteamTransportLocation> _pendingTopologyChanges = new List<SteamTransportLocation>();
		private readonly List<SteamTransportTopology> _newTopologies = new List<SteamTransportTopology>();
		private readonly object _pendingTopologyListLock = new object();
        private long _topologyGeneration = 0;
		private Barrier barrier = new Barrier(2);
		private int _pendingTopologiesToProcess = 0;

		public void OnTick()
		{
			if (_pendingTopologyChanges.Count > 0)
			{
				ProcessTopologyChanges();
			}
			else
			{
				ProcessIdealCondensation();
			}
		}

		public void PostTick()
		{
			barrier.SignalAndWait();
		}

		private void ProcessTopologyChanges()
		{
			_topologyGeneration++;
			Console.WriteLine("Processing Topology Changes: Generation #{0}", _topologyGeneration);
			lock (_pendingTopologyListLock)
			{
				foreach (var steamTransportLocation in _pendingTopologyChanges)
				{
					_pendingTopologiesToProcess++;
                    TheMod.JobManager.AddBackgroundJob(new RecalculateTopologyJob(steamTransportLocation, _topologyGeneration, this));
				}
				_pendingTopologyChanges.Clear();
			}
		}

		public void AddPendingTopologyChange(SteamTransportLocation steamTransportLocation)
		{
			lock (_pendingTopologyListLock)
			{
				_pendingTopologyChanges.Add(steamTransportLocation);
				Console.WriteLine($"PendingTopologyChanges: {_pendingTopologyChanges.Count}");
			}
		}

		internal void DeactivateTopology(SteamTransportTopology topology)
		{
			_activeTopologies.Remove(topology);
		}

		public void TopologyCalculated(RecalculateTopologyJob.Result result)
		{
			Console.WriteLine($"{Thread.CurrentThread.Name} - Topology calculated {result.NewTopology}");

			lock (_pendingTopologyListLock)
			{
				_newTopologies.Add(result.NewTopology);
			}

			bool moveToNextState = Interlocked.Decrement(ref _pendingTopologiesToProcess) == 0;

			

			if (moveToNextState)
			{
				Console.WriteLine($"{Thread.CurrentThread.Name} - Cleaning Topologies");
				foreach (var topology in _newTopologies)
				{
					if (topology.GetTransports().Any())
					{
						_activeTopologies.Add(topology);
					}
				}
				_newTopologies.Clear();

				Console.WriteLine($"{Thread.CurrentThread.Name} - Processing Ideal Condensation");
				foreach (var topology in _activeTopologies)
				{
					Console.WriteLine("Active Topology: {0}", topology);
				}
				ProcessIdealCondensation();
			}
		}

		private void ProcessIdealCondensation()
		{
			TheMod.JobManager.AddBackgroundJob(new CalculateIdealCondensationJob(this));
			
		}

		private void Finished()
		{
			barrier.SignalAndWait();
		}

		public void IdealCondensationCalculated(CalculateIdealCondensationJob.Result result)
		{
			Finished();
		}
	}

	public interface INotifyTopologyCalculated
	{
		void TopologyCalculated(RecalculateTopologyJob.Result result);
	}

	public interface INotifyIdealCondenstationComplete
	{
		void IdealCondensationCalculated(CalculateIdealCondensationJob.Result result);
	}
}
