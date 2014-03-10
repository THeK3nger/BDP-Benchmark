// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PriorityQueue.cs" company="Davide Aversa">
//   MIT License
// </copyright>
// <summary>
//   Implement a generic priority queue that order elements of class V according the priority P.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RoomOfRequirement
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Implement a generic priority queue that order elements of class V according the priority P.
    /// </summary>
    /// <typeparam name="TP">
    /// The type of the priority value.
    /// </typeparam>
    /// <typeparam name="TV">
    /// The type of the contents of the queue.
    /// </typeparam>
    internal class PriorityQueue<TP, TV>
    {
        #region InternalState

        /// <summary>
        /// The internal representation of the queue.
        /// </summary>
        private readonly SortedDictionary<TP, Queue<TV>> list = new SortedDictionary<TP, Queue<TV>>();

        #endregion

        /// <summary>
        /// Gets the number of item in the queue.
        /// </summary>
        /// <value>The count.</value>
        public int Count
        {
            get
            {
                return list.Count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        /// <value><c>true</c> if this instance is empty; otherwise, <c>false</c>.</value>
        public bool IsEmpty 
        {
            get
            {
                return !list.Any();
            }
        }

        /// <summary>
        /// Enqueue the specified value with the given priority.
        /// </summary>
        /// <param name="priority">
        /// Priority.
        /// </param>
        /// <param name="value">
        /// Value.
        /// </param>
        public void Enqueue(TP priority, TV value)
        {
            Queue<TV> q;
            if (!list.TryGetValue(priority, out q))
            {
                q = new Queue<TV>();
                list.Add(priority, q);
            }

            q.Enqueue(value);
        }

        /// <summary>
        /// Dequeue the first element in the queue.
        /// </summary>
        /// <returns>
        /// The <see cref="TV"/>.
        /// </returns>
        public TV Dequeue()
        {
            // will throw if there isn’t any first element!
            var pair = list.First();
            var v = pair.Value.Dequeue();
            if (pair.Value.Count == 0)
            {
                // nothing left of the top priority.
                list.Remove(pair.Key);
            }

            return v;
        }
    }
}
