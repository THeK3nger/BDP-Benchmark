using UnityEngine;
using System.Collections.Generic;

namespace RoomOfRequirement.AI.Data
{
	/// <summary>
	/// Hierarchical blackboard for storing non-homogeneous pairs of codes.
	/// </summary>
	public class HierarchicalBlackboard
	{
		#region PrivateAttributes
		/// <summary>
		/// The parent blackboard.
		/// </summary>
		HierarchicalBlackboard parent;

		/// <summary>
		/// Internal data representation.
		/// </summary>
		Dictionary<string,object> data;
		#endregion

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="RoomOfRequirement.AI.Data.HierarchicalBlackboard"/> class.
		/// </summary>
		public HierarchicalBlackboard ()
		{
			data = new Dictionary<string, object> ();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RoomOfRequirement.AI.Data.HierarchicalBlackboard"/> class.
		/// </summary>
		/// <param name="parent">Parent.</param>
		public HierarchicalBlackboard (HierarchicalBlackboard parent)
		{
			data = new Dictionary<string, object>();
			this.parent = parent;
		}
		#endregion

		/// <summary>
		/// Get the specified key.
		/// </summary>
		/// <param name="key">Key.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T Get<T> (string key)
		{
			if (data.ContainsKey(key)) {
				return (T) data [key];
			}
			if (parent != null) {
				return parent.Get<T>(key);
			}
			return null;
		}

		/// <summary>
		/// Set the specified key and value.
		/// </summary>
		/// <param name="key">Key.</param>
		/// <param name="value">Value.</param>
		public void Set (string key, object value) {
			if (!data.ContainsKey(key)) {
				data.Add(key,value);
			}
		}

	}
}