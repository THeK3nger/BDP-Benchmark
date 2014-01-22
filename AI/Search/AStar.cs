using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RoomOfRequirement.Search
{

/// <summary>
/// Generic Implementation of the A* algorithm.
/// </summary>
	public class AStar
	{

	#region ProfilingCollection
		// Profiling Info
		static public bool CollectProfiling = false;
		static public int ExpandedNodes = 0;
		static public int MaxMemoryQueue = 0;
		static public long ElapsedTime = 0;
		//---------------
	#endregion

		/// <summary>
		/// Finds the optimal path between start and destionation TNode.
		/// </summary>
		/// <returns>The path.</returns>
		/// <param name="start">Starting Node.</param>
		/// <param name="destination">Destination Node.</param>
		/// <param name="distance">Function to compute distance beween nodes.</param>
		/// <param name="estimate">Function to estimate the remaining cost for the goal.</param>
		/// <typeparam name="TNode">Any class implement IHasNeighbours.</typeparam>
		static public Path<TNode> FindPath<TNode> (
		IHasNeighbours<TNode> dataStructure,
		TNode start,
		TNode destination,
		Func<TNode,TNode,double> distance,
		Func<TNode, double> estimate)
		{
			//---------------------- Profiling Information
			// The value are decopled from the static variables in order to provide 
			// a more thread safe access to this values. In fact, the static 
			// variable are updated only at the end of the search.
			float expandedNodes = 0;
			float maxMemoryQueue = 0;
			Stopwatch st = new Stopwatch ();
			//----------------------
			var closed = new HashSet<TNode> ();
			var queue = new PriorityQueue<double, Path<TNode>> ();
			queue.Enqueue (0, new Path<TNode> (start));
			if (CollectProfiling)
				st.Start ();
			while (!queue.IsEmpty) {
				if (CollectProfiling) {
					if (queue.Count > maxMemoryQueue)
						maxMemoryQueue = queue.Count;
				}
				var path = queue.Dequeue ();
				if (closed.Contains (path.LastStep))
					continue;
				if (path.LastStep.Equals (destination)) {
					if (CollectProfiling) {
						st.Stop ();
						AStar.ExpandedNodes = (int)expandedNodes;
						AStar.ElapsedTime = st.ElapsedTicks;
						AStar.MaxMemoryQueue = (int)maxMemoryQueue;
					}
					return path;
				}
				closed.Add (path.LastStep);
				expandedNodes++;
				foreach (TNode n in dataStructure.Neighbours(path.LastStep)) {
					double d = distance (path.LastStep, n);
					if (n.Equals (destination))
						d = 0;
					var newPath = path.AddStep (n, d);
					queue.Enqueue (newPath.TotalCost + estimate (n), newPath);
				}
			}
			AStar.ExpandedNodes = (int)expandedNodes;
			AStar.ElapsedTime = st.ElapsedTicks;
			AStar.MaxMemoryQueue = (int)maxMemoryQueue;
			return null;
		}

	}
}
