using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System;

namespace RoomOfRequirement.Generic
{

/// <summary>
/// A generic implementation of a graph.
/// </summary>
	[Serializable]
	public class Graph<TNode>  : IHasNeighbours<TNode>, ISerializable
	{

		/// <summary>
		/// The adjacency list.
		/// </summary>
		Dictionary<TNode,List<TNode>> adjacencyList;

		/// <summary>
		/// Initializes a new instance of the <see cref="Graph`1"/> class.
		/// </summary>
		public Graph ()
		{
			adjacencyList = new Dictionary<TNode, List<TNode>> ();
		}

		public Graph (SerializationInfo info, StreamingContext context)
		{
			adjacencyList = (Dictionary<TNode,List<TNode>>)
			info.GetValue ("AdjacencyList", typeof(Dictionary<TNode,List<TNode>>));
		}

		/// <summary>
		/// Contains the vertex.
		/// </summary>
		/// <returns><c>true</c>, if vertex was contained, <c>false</c> otherwise.</returns>
		/// <param name="t">T.</param>
		public bool ContainVertex (TNode t)
		{
			return adjacencyList.ContainsKey (t);
		}

		/// <summary>
		/// Adds the vertex.
		/// </summary>
		/// <param name="t">T.</param>
		public void AddVertex (TNode t)
		{
			if (!ContainVertex (t)) {
				adjacencyList.Add (t, new List<TNode> ());
			}
		}

		/// <summary>
		/// Adds the edge.
		/// </summary>
		/// <param name="t1">T1.</param>
		/// <param name="t2">T2.</param>
		public void AddEdge (TNode t1, TNode t2)
		{
			if (!ContainVertex (t1))
				AddVertex (t1);
			if (!ContainVertex (t2))
				AddVertex (t2);
			if (!AreAdjacent (t1, t2)) {
				adjacencyList [t1].Add (t2);
			}
		}

		/// <summary>
		/// Removes the vertex.
		/// </summary>
		/// <param name="t">T.</param>
		public void RemoveVertex (TNode t)
		{
			if (ContainVertex (t)) {
				adjacencyList.Remove (t);
			}
			foreach (TNode other in adjacencyList.Keys) {
				adjacencyList [other].RemoveAll (p => p.Equals (t));
			}
		}

		/// <summary>
		/// Removes the edge.
		/// </summary>
		/// <param name="t1">T1.</param>
		/// <param name="t2">T2.</param>
		public void RemoveEdge (TNode t1, TNode t2)
		{
			if (AreAdjacent (t1, t2)) {
				adjacencyList [t1].RemoveAll (p => p.Equals (t2));
				// This is a security check to simplify UndirectedGraph implementations.
				if (AreAdjacent (t1, t2)) {
					adjacencyList [t2].RemoveAll (p => p.Equals (t1));
				}
			}
		}

		/// <summary>
		/// Check if two nodes are adjacent.
		/// </summary>
		/// <returns><c>true</c>, if they are adjacent, <c>false</c> otherwise.</returns>
		/// <param name="t1">T1.</param>
		/// <param name="t2">T2.</param>
		public virtual bool AreAdjacent (TNode t1, TNode t2)
		{
			if (!ContainVertex (t1))
				return false;
			if (!ContainVertex (t2))
				return false;
			return adjacencyList [t1].Contains (t2);
		}

		/// <summary>
		/// Gets the vertices.
		/// </summary>
		/// <value>The vertices.</value>
		public IEnumerable<TNode> Vertices {
			get {
				return adjacencyList.Keys;
			}
		}

		/// <summary>
		/// Gets the neighbours.
		/// </summary>
		/// <returns>The neighbours.</returns>
		/// <param name="t">T.</param>
		public List<TNode> GetNeighbours (TNode t)
		{
			if (!ContainVertex (t))
				return null;
			var res = new List<TNode> ();
			foreach (TNode v in Vertices) {
				if (AreAdjacent (t, v)) {
					res.Add (v);
				}
			}
			return res;
		}

		/// <summary>
		/// IHasNeighbours implementation 
		/// </summary>
		/// <value>The neighbours.</value>
		IEnumerable<TNode> IHasNeighbours<TNode>.Neighbours (TNode t)
		{
			return GetNeighbours (t);
		}

	#region SertializationMethods
		/// <summary>
		/// Implement ISerializable interface.
		/// </summary>
		/// <param name="info">Serialization Info.</param>
		/// <param name="context">Streaming Context.</param>
		public virtual void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");
			info.AddValue ("AdjacencyList", adjacencyList);
		}
	#endregion
	}
}