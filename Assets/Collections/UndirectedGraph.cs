using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using System;

namespace RoomOfRequirement
{
	[Serializable]
	public class UndirectedGraph<TNode> : Graph<TNode>, ISerializable
	{

		public UndirectedGraph () : base()
		{
		}

		public UndirectedGraph (SerializationInfo info, StreamingContext context) : base(info, context)
		{

		}

		public override bool AreAdjacent (TNode t1, TNode t2)
		{
			return base.AreAdjacent (t1, t2) || base.AreAdjacent (t2, t1);
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
		}
	#endregion
	}
}