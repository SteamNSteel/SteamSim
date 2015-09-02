using System;
using System.Collections.Generic;
using Steam.API;
using SteamNSteel.Jobs;

namespace SteamNSteel.Impl.Jobs
{
	public class RecalculateTopologyJob : IJob
	{
		private readonly SteamTransportLocation _location;
		private readonly long _topologyGeneration;
		private readonly INotifyTopologyCalculated _notifyTopologyCalculated;

		public RecalculateTopologyJob(SteamTransportLocation location, long topologyGeneration, INotifyTopologyCalculated notifyTopologyCalculated)
		{
			_location = location;
			_topologyGeneration = topologyGeneration;
			_notifyTopologyCalculated = notifyTopologyCalculated;
		}

		public void Execute()
		{
			SteamTransportRegistry steamTransportRegistry = TheMod.SteamTransportRegistry;
			HashSet<SteamTransportTopology> topologiesToMerge = new HashSet<SteamTransportTopology>();
			SteamTransportTopology newTopology = new SteamTransportTopology(_topologyGeneration);
			lock (newTopology.GetLockObject())
			{
				Stack<SteamTransportLocation> todoList = new Stack<SteamTransportLocation>();
				todoList.Push(_location);
				while (todoList.Count > 0)
				{
					SteamTransportLocation location = todoList.Pop();

					SteamTransport transportAtLocation = (SteamTransport)steamTransportRegistry.GetSteamTransportAtLocation(location);
					transportAtLocation.SetTopology(newTopology);

					foreach (var forgeDirection in ForgeDirection.VALID_DIRECTIONS)
					{
						SteamTransportLocation steamTransportLocation = _location.Offset(forgeDirection);
						SteamTransport steamTransportAtLocation = (SteamTransport)steamTransportRegistry.GetSteamTransportAtLocation(steamTransportLocation);
						if (steamTransportAtLocation == null) continue;

						SteamTransportTopology currentTopology = steamTransportAtLocation.GetTopology();
						if (currentTopology == null || currentTopology.IsSupercededBy(newTopology))
						{
							todoList.Push(steamTransportLocation);
						}
						else if (!currentTopology.Equals(newTopology) && currentTopology.IsSameGenerationAs(newTopology))
						{
							//This check might not be nessessary in Java
							if (currentTopology.HasPriorityOver(newTopology) && !topologiesToMerge.Contains(currentTopology))
							{
								topologiesToMerge.Add(currentTopology);
							}
						}
					}
				}
			}

			foreach (SteamTransportTopology topology in topologiesToMerge)
			{
				Console.WriteLine("Merging Topology {0} into {1}", topology, newTopology);
				lock (topology.GetLockObject())
				{
					foreach (SteamTransport transport in topology.GetTransports())
					{
						transport.SetTopology(newTopology);
					}
					topology.MakeObsolete();
				}
			}

			_notifyTopologyCalculated.TopologyCalculated(new Result(newTopology));
		}

		public class Result
		{
			internal readonly SteamTransportTopology NewTopology;

			internal Result(SteamTransportTopology newTopology)
			{
				NewTopology = newTopology;
			}
		}
	}
}