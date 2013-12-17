using System.Collections.Generic;
using System.Runtime.Serialization;
using System;

namespace RoomOfRequirement
{
	[Serializable]
	public class UndirectedLabeledGraph<TNode,TLVertex,TLEdges> : UndirectedGraph<TNode>, ISerializable
	{

		Dictionary<TNode,TLVertex> vertexLabel;
		Dictionary<Tuple<TNode,TNode>,TLEdges> edgesLabel;

		public UndirectedLabeledGraph () : base()
		{
			vertexLabel = new Dictionary<TNode,TLVertex> ();
			edgesLabel = new Dictionary<Tuple<TNode, TNode>, TLEdges> ();
		}

		public UndirectedLabeledGraph (SerializationInfo info, StreamingContext context) : base(info, context)
		{
			vertexLabel = (Dictionary<TNode,TLVertex>)
			info.GetValue ("VertexLabel", typeof(Dictionary<TNode,TLVertex>));
			edgesLabel = (Dictionary<Tuple<TNode,TNode>,TLEdges>)
			info.GetValue ("EdgesLabel", typeof(Dictionary<Tuple<TNode,TNode>,TLEdges>));
		}

		public void SetVertexLabel (TNode t, TLVertex label)
		{
			if (ContainVertex (t)) {
				if (!vertexLabel.ContainsKey (t)) {
					vertexLabel.Add (t, label);
				} else {
					vertexLabel [t] = label;
				}
			}
		}

		public void SetEdgeLabel (TNode t1, TNode t2, TLEdges label)
		{
			if (AreAdjacent (t1, t2)) {
				var edge1 = new Tuple<TNode,TNode> (t1, t2);
				var edge2 = new Tuple<TNode,TNode> (t2, t1);
				if (edgesLabel.ContainsKey (edge1)) {
					edgesLabel [edge1] = label;
				} else if (edgesLabel.ContainsKey (edge2)) {
					edgesLabel [edge2] = label;
				} else {
					edgesLabel.Add (edge1, label);
				}
			}
		}

		public TLVertex GetVertexLabel (TNode t)
		{
			if (vertexLabel.ContainsKey (t)) {
				return vertexLabel [t];
			}
			return default(TLVertex);
		}

		public TLEdges GetEdgeLabel (TNode t1, TNode t2)
		{
			var edge1 = new Tuple<TNode,TNode> (t1, t2);
			var edge2 = new Tuple<TNode,TNode> (t2, t1);
			if (edgesLabel.ContainsKey (edge1)) {
				return edgesLabel [edge1];
			} 
			if (edgesLabel.ContainsKey (edge2)) {
				return edgesLabel [edge2];
			}
			return default(TLEdges);
		}

	#region SertializationMethods
		/// <summary>
		/// Implement ISerializable interface.
		/// </summary>
		/// <param name="info">Serialization Info.</param>
		/// <param name="context">Streaming Context.</param>
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
			if (info == null)
				throw new ArgumentNullException ("info");
			info.AddValue ("VertexLabel", vertexLabel);
			info.AddValue ("EdgesLabel", edgesLabel);
		}
	#endregion

	}
}