using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using System;

/// <summary>
/// Implement the concept of Portal Edge in a grid-like map.
/// </summary>
/// <remarks>
/// A portal edge is an edge that connects two different area in a map.
/// </remarks>
[Serializable]
public class Portal : ISerializable
{

	/// <summary>
	/// The linked squares.
	/// </summary>
	Tuple<MapSquare,MapSquare> linkedSquares;
	Tuple<int,int> linkedAreas;

	/// <summary>
	/// Gets the linked squares.
	/// </summary>
	/// <value>The linked squares.</value>
	public Tuple<MapSquare, MapSquare> LinkedSquares {
		get {
			return linkedSquares;
		}
	}

	/// <summary>
	/// Gets the linked areas.
	/// </summary>
	/// <value>The linked areas.</value>
	public Tuple<int, int> LinkedAreas {
		get {
			return linkedAreas;
		}
	}

	/// <summary>
	/// Gets the middle point.
	/// </summary>
	/// <value>The middle point.</value>
	public Vector2 MidPoint {
		get { return new Vector2(
				(linkedSquares.First.x + linkedSquares.Second.x)*0.5f,
				(linkedSquares.First.y + linkedSquares.Second.y)*0.5f
				);}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Portal"/> class.
	/// </summary>
	/// <param name="square1">The first square.</param>
	/// <param name="square2">The second square.</param>
	/// <param name="area1">The area of the first square.</param>
	/// <param name="area2">The area of the second square.</param>
	public Portal (MapSquare square1, MapSquare square2, int area1, int area2)
	{
		// TODO: There can be inconsistencies. It is possible to develop a more robust constructor.
		linkedSquares = new Tuple<MapSquare, MapSquare> (square1, square2);
		linkedAreas = new Tuple<int,int> (area1, area2);
	}

	public Portal (SerializationInfo info, StreamingContext context) {
		Tuple<int,int> firstSquare = (Tuple<int,int>) info.GetValue("LinkedSquaresFirst",typeof(Tuple<int,int>));
		Tuple<int,int> secondSquare = (Tuple<int,int>) info.GetValue("LinkedSquaresSecond",typeof(Tuple<int,int>));
		linkedSquares = new Tuple<MapSquare,MapSquare>(new MapSquare(firstSquare.First,firstSquare.Second),
		                                               new MapSquare(secondSquare.First,secondSquare.Second));
		linkedAreas = (Tuple<int,int>) info.GetValue("LinkedAreas",typeof(Tuple<int,int>));
	}

	/// <summary>
	/// Determines whether this instance is linked with the specified other.
	/// </summary>
	/// <returns><c>true</c> if this instance is linked with the specified other; otherwise, <c>false</c>.</returns>
	/// <param name="other">Another portal.</param>
	public bool IsLinkedWith (Portal other)
	{
		if (other == null) return false;
		return this.linkedAreas.First == other.LinkedAreas.First ||
			this.linkedAreas.First == other.LinkedAreas.Second || 
			this.linkedAreas.Second == other.LinkedAreas.First ||
			this.linkedAreas.Second == other.LinkedAreas.Second;
	}

	/// <summary>
	/// Distance between portals p1 and p2.
	/// </summary>
	/// <param name="p1">P1.</param>
	/// <param name="p2">P2.</param>
	public static float Distance(Portal p1, Portal p2) {
		return Vector2.Distance(p1.MidPoint,p2.MidPoint);
	}

	/// <summary>
	/// Distance between a portal p1 and a 2D point.
	/// </summary>
	/// <param name="p1">P1.</param>
	/// <param name="point">Point.</param>
	public static float Distance(Portal p1, MapSquare ms) {
		return Vector2.Distance(p1.MidPoint,new Vector2(ms.x,ms.y));
	}

	#region OperatorsOverload
	public MapSquare this[int area] {
		get {
			if (LinkedAreas.First == area) return LinkedSquares.First;
			if (LinkedAreas.Second == area) return linkedSquares.Second;
			return null;
		}
	}

	public int this[MapSquare ms] {
		get {
			if (LinkedSquares.First == ms) return linkedAreas.First;
			if (LinkedSquares.Second == ms) return linkedAreas.Second;
			return -1;
		}
	}
	#endregion

	#region StandardOverrides
	/// <summary>
	/// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="Portal"/>.
	/// </summary>
	/// <param name="obj">The <see cref="System.Object"/> to compare with the current <see cref="Portal"/>.</param>
	/// <returns><c>true</c> if the specified <see cref="System.Object"/> is equal to the current <see cref="Portal"/>; otherwise, <c>false</c>.</returns>
	public override bool Equals(object obj)
	{
		if (obj == null) return false;
		var other = (Portal) obj;
		return linkedSquares == other.linkedSquares;
	}
	
	/// <summary>
	/// Serves as a hash function for a <see cref="Portal"/> object.
	/// </summary>
	/// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a hash table.</returns>
	public override int GetHashCode()
	{
		return linkedSquares.GetHashCode();
	}

	/// <summary>
	/// Returns a <see cref="System.String"/> that represents the current <see cref="Portal"/>.
	/// </summary>
	/// <returns>A <see cref="System.String"/> that represents the current <see cref="Portal"/>.</returns>
	public override string ToString ()
	{
		return string.Format ("[Portal: LinkedSquares={0} LinkedAreas={1}]", LinkedSquares, LinkedAreas);
	}
	#endregion

	#region SertializationMethods
	/// <summary>
	/// Implement ISerializable interface.
	/// </summary>
	/// <param name="info">Serialization Info.</param>
	/// <param name="context">Streaming Context.</param>
	public void GetObjectData (SerializationInfo info, StreamingContext context) {
		if (info == null)
			throw new ArgumentNullException("info");
		Tuple<int,int> linkedSquaresFirst = new Tuple<int,int>((int)(linkedSquares.First.x),(int) (linkedSquares.First.y));
		Tuple<int,int> linkedSquaresSecond = new Tuple<int,int>((int) linkedSquares.Second.x,(int) linkedSquares.Second.y);
		info.AddValue("LinkedSquaresFirst",linkedSquaresFirst);
		info.AddValue("LinkedSquaresSecond",linkedSquaresSecond);
		info.AddValue("LinkedAreas",linkedAreas);
	}
	#endregion
}
