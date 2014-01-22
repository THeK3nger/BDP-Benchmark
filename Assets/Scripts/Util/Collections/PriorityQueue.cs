using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Implement a generic priority queue that order elements of class V according the priority P.
/// </summary>
class PriorityQueue<P, V>
{
#region InternalState
	/// <summary>
	/// The internal representation of the queue.
	/// </summary>
	SortedDictionary<P, Queue<V>> list = new SortedDictionary<P, Queue<V>> ();
#endregion

	public int Count {
		get {
			return list.Count;
		}
	}

	/// <summary>
	/// Enqueue the specified value with the given priority.
	/// </summary>
	/// <param name="priority">Priority.</param>
	/// <param name="value">Value.</param>
	public void Enqueue (P priority, V value)
	{
		Queue<V> q;
		if (!list.TryGetValue (priority, out q)) {
			q = new Queue<V> ();
			list.Add (priority, q);
		}
		q.Enqueue (value);
	}

	/// <summary>
	/// Dequeue the first element in the queue.
	/// </summary>
	public V Dequeue ()
	{
		// will throw if there isn’t any first element!
		var pair = list.First ();
		var v = pair.Value.Dequeue ();
		if (pair.Value.Count == 0) // nothing left of the top priority.
			list.Remove (pair.Key);
		return v;
	}

	/// <summary>
	/// Gets a value indicating whether this instance is empty.
	/// </summary>
	/// <value><c>true</c> if this instance is empty; otherwise, <c>false</c>.</value>
	public bool IsEmpty {
		get { return !list.Any (); }
	}
}