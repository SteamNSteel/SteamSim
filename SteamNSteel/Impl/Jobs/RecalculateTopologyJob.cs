using System;
using System.Collections.Generic;
using Steam.API;
using SteamNSteel.Jobs;

namespace SteamNSteel.Impl.Jobs
{
	public class RecalculateTopologyJob : IJob
	{
		private readonly SteamTransportLocation _location;
		private readonly int _topologyGeneration;

		public RecalculateTopologyJob(SteamTransportLocation location, int topologyGeneration)
		{
			_location = location;
			_topologyGeneration = topologyGeneration;
		}

		public void Execute()
		{
			var steamTransportRegistry = TheMod.SteamTransportRegistry;
			HashSet<SteamTransportTopology> topologiesToMerge = new HashSet<SteamTransportTopology>();
			SteamTransportTopology newTopology = new SteamTransportTopology(_topologyGeneration);
			lock (newTopology.GetLockObject())
			{
				var todoList = new Stack<SteamTransportLocation>();
				todoList.Push(_location);
				while (todoList.Count > 0)
				{
					var location = todoList.Pop();

					var transportAtLocation = steamTransportRegistry.GetSteamTransportAtLocation(location);
					transportAtLocation.SetTopology(newTopology);

					foreach (var forgeDirection in ForgeDirection.VALID_DIRECTIONS)
					{
						var steamTransportLocation = _location.Offset(forgeDirection);
						var steamTransportAtLocation = steamTransportRegistry.GetSteamTransportAtLocation(steamTransportLocation);
						if (steamTransportAtLocation == null) continue;

						var currentTopology = steamTransportAtLocation.GetTopology();
						if (currentTopology == null || currentTopology.IsSupercededBy(newTopology))
						{
							todoList.Push(steamTransportLocation);
						}
						else if (!currentTopology.Equals(newTopology) && currentTopology.IsSameGenerationAs(newTopology))
						{
							//FIXME: Merge topologies.
							//This check might not be nessessary in Java
							if (currentTopology.HasPriorityOver(newTopology) && !topologiesToMerge.Contains(currentTopology))
							{
								topologiesToMerge.Add(currentTopology);
							}
						}
					}
				}
			}

			foreach (var topology in topologiesToMerge)
			{
				lock (topology.GetLockObject())
				{
					foreach (var transport in topology.GetTransports())
					{
						newTopology.AddTransport(transport);
					}
					topology.MakeObsolete();
				}
			}
		}
	}
}