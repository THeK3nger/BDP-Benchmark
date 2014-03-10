// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AStar.cs" company="Davide Aversa">
//   MIT License
// </copyright>
// <summary>
//   Generic Implementation of the A* algorithm.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RoomOfRequirement.Search
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    using  RoomOfRequirement.AI.Data;

    /// <summary>
    /// Generic Implementation of the A* algorithm.
    /// </summary>
    public class AStar
    {
        #region ProfilingCollection

        // Profiling Info

        /// <summary>
        /// Gets or sets a value indicating whether collect profiling.
        /// </summary>
        public static bool CollectProfiling { get; set; }

        /// <summary>
        /// Gets the expanded nodes.
        /// </summary>
        public static int ExpandedNodes { get; private set; }

        /// <summary>
        /// Gets the max memory queue.
        /// </summary>
        public static int MaxMemoryQueue { get; private set; }

        /// <summary>
        /// Gets the elapsed time.
        /// </summary>
        public static long ElapsedTime { get; private set; }

        // ---------------
        #endregion

        /// <summary>
        /// Finds the optimal path between start and destination TNode.
        /// </summary>
        /// <returns>
        /// The path.
        /// </returns>
        /// <param name="start">
        /// Starting Node.
        /// </param>
        /// <param name="destination">
        /// Destination Node.
        /// </param>
        /// <param name="neighbours">
        /// The neighbours.
        /// </param>
        /// <param name="distance">
        /// Function to compute distance between nodes.
        /// </param>
        /// <param name="estimate">
        /// Function to estimate the remaining cost for the goal.
        /// </param>
        /// <typeparam name="TNode">
        /// Any class implement IHasNeighbours.
        /// </typeparam>
        public static Path<TNode> FindPath<TNode>(
            TNode start, 
            TNode destination, 
            Func<TNode, IEnumerable<TNode>> neighbours, 
            Func<TNode, TNode, double> distance, 
            Func<TNode, double> estimate)
        {
            // ---------------------- Profiling Information
            // The value are decoupled from the static variables in order to provide 
            // a more thread safe access to this values. In fact, the static 
            // variable are updated only at the end of the search.
            float expandedNodes = 0;
            float maxMemoryQueue = 0;
            var st = new Stopwatch();

            // ----------------------
            var closed = new HashSet<TNode>();
            var queue = new PriorityQueue<double, Path<TNode>>();
            queue.Enqueue(0, new Path<TNode>(start));
            if (CollectProfiling)
            {
                st.Start();
            }

            while (!queue.IsEmpty)
            {
                if (CollectProfiling)
                {
                    if (queue.Count > maxMemoryQueue)
                    {
                        maxMemoryQueue = queue.Count;
                    }
                }

                var path = queue.Dequeue();
                if (closed.Contains(path.LastStep))
                {
                    continue;
                }

                if (path.LastStep.Equals(destination))
                {
                    if (!CollectProfiling)
                    {
                        return path;
                    }

                    st.Stop();
                    ExpandedNodes = (int)expandedNodes;
                    ElapsedTime = st.ElapsedTicks;
                    MaxMemoryQueue = (int)maxMemoryQueue;

                    return path;
                }

                closed.Add(path.LastStep);
                expandedNodes++;
                foreach (var n in neighbours(path.LastStep))
                {
                    var d = distance(path.LastStep, n);
                    if (n.Equals(destination))
                    {
                        d = 0;
                    }

                    var newPath = path.AddStep(n, d);
                    queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
                }
            }

            ExpandedNodes = (int)expandedNodes;
            ElapsedTime = st.ElapsedTicks;
            MaxMemoryQueue = (int)maxMemoryQueue;
            return null;
        }
    }
}
