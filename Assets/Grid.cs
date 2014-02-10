using System.Collections;
using System;
using System.Runtime.Serialization;
using UnityEngine;

namespace RoomOfRequirement.Generic
{

/// <summary>
/// This class represents a set of data shaped as a 2D grid. 
/// </summary>
	[Serializable]
	public class Grid<T> : IEnumerable, ISerializable
	{

#region InternalState
		/// <summary>
		/// The internal data.
		/// </summary>
		T[] internalData;
#endregion

#region PublicProperties
		/// <summary>
		/// Gets the width.
		/// </summary>
		/// <value>The width.</value>
		public int Width { get; private set; }

		/// <summary>
		/// Gets the height.
		/// </summary>
		/// <value>The height.</value>
		public int Height { get; private set; }
#endregion

#region OperatorsOverload
		/// <summary>
		/// Gets or sets the <see cref="Grid`1"/> with the specified x y.
		/// </summary>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		public T this [int x, int y] {
			get {
				return internalData [GetArrayIndex (x, y)];
			}

			set {
				internalData [GetArrayIndex (x, y)] = value;
			}
		}
#endregion

#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="Grid`1"/> class.
		/// </summary>
		/// <param name="height">Height.</param>
		/// <param name="width">Width.</param>
		public Grid (int height, int width)
		{
			Width = width;
			Height = height;
			internalData = new T[Width * Height];
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Grid`1"/> class.
		/// </summary>
		/// <param name="info">Serialization Info.</param>
		/// <param name="context">Context.</param>
		public Grid (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");
			Width = (int)info.GetValue ("Width", typeof(int));
			Height = (int)info.GetValue ("Height", typeof(int));
			internalData = (T[])info.GetValue ("InternalData", typeof(T[]));
		}
#endregion

#region PrivateUtilityMethods
		/// <summary>
		/// Converts a pair of matrix indexes <i,j> in the corresponding
		/// index of the linearized array associated to the map matrix.
		/// </summary>
		/// <returns>The array index.</returns>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		int GetArrayIndex (int x, int y)
		{
			return y * Width + x;
		}
#endregion

		public bool IsOutOfBound(int x, int y)
		{
			if (x<0 || y<0 || x>=Width || y>=Height) return true;
			return false;
		}

#region EnumerableImplementation
		/// <summary>
		/// Gets the enumerator.
		/// </summary>
		/// <returns>The enumerator.</returns>
		public IEnumerator GetEnumerator ()
		{
			for (int i=0; i<internalData.Length; i++) {
				yield return internalData [i];
			}
		}
#endregion

#region SertializationMethods
		/// <summary>
		/// Implement ISerializable interface.
		/// </summary>
		/// <param name="info">Serialization Info.</param>
		/// <param name="context">Streaming Context.</param>
		public void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");
			info.AddValue ("Width", Width);
			info.AddValue ("Height", Height);
			info.AddValue ("InternalData", internalData);
		}
#endregion

	}
}