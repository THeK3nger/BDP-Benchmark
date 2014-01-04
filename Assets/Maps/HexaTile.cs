using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Representation for a tile in a hexagonal grid.
/// </summary>
public class HexaTile {

	#region PublicProperties
	/// <summary>
	/// Gets the x.
	/// </summary>
	/// <value>The x.</value>
	public int x { get; private set; }
	/// <summary>
	/// Gets the y.
	/// </summary>
	/// <value>The y.</value>
	public int y { get; private set; }
	#endregion

	#region Constructors
	/// <summary>
	/// Initializes a new instance of the <see cref="MapSquare"/> class.
	/// </summary>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	public HexaTile(int x, int y) {
		this.x = x;
		this.y = y;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="HexaTile"/> class.
	/// Use the dummyZ system.
	/// </summary>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	/// <param name="z">The z coordinate.</param>
	public HexaTile(int x, int y, int z) {
		this.x = x + z;
		this.y = y - z;
	}
	#endregion

	/// <summary>
	/// Transform the hexagon tile back to the euclidean coordinate.
	/// </summary>
	/// <returns>The euclidean coordinate for the tile centre.</returns>
	/// <param name="size">The size of the hexagon apothem.</param>
	public Vector2 ToEuclidean(float apothem) {
		float ex = apothem * (2 * x + y);
		float ey = apothem * Mathf.Sqrt(3) * y;
		return new Vector2(ex,ey);
	}

	/// <summary>
	/// Return a Vector2 representation of the tile.
	/// </summary>
	/// <returns>The vector2.</returns>
	public Vector2 ToVector2() {
		return new Vector2(x,y);
	}

	#region StandardOverride

	/// <summary>
	/// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="MapSquare"/>.
	/// </summary>
	/// <param name="obj">The <see cref="System.Object"/> to compare with the current <see cref="MapSquare"/>.</param>
	/// <returns><c>true</c> if the specified <see cref="System.Object"/> is equal to the current <see cref="MapSquare"/>;
	/// otherwise, <c>false</c>.</returns>
	public override bool Equals(object obj)
	{
		if (obj == null) return false;
		var other = (MapSquare) obj;
		return (x == other.x) && (y == other.y);
	}
	
	public static bool operator == (HexaTile a, HexaTile b)
	{
		// If both are null, or both are same instance, return true.
		if (System.Object.ReferenceEquals (a, b)) {
			return true;
		}
		
		// If one is null, but not both, return false.
		if (((object)a == null) || ((object)b == null)) {
			return false;
		}
		
		// Return true if the fields match:
		return a.Equals (b);
	}
	
	public static bool operator != (HexaTile a, HexaTile b)
	{
		return !(a == b);
	}
	
	/// <summary>
	/// Serves as a hash function for a <see cref="MapSquare"/> object.
	/// </summary>
	/// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data 
	/// structures such as a hash table.</returns>
	public override int GetHashCode()
	{
		return x*11+y;
	}
	
	/// <summary>
	/// Returns a <see cref="System.String"/> that represents the current <see cref="MapSquare"/>.
	/// </summary>
	/// <returns>A <see cref="System.String"/> that represents the current <see cref="MapSquare"/>.</returns>
	public override string ToString ()
	{
		return string.Format ("[{0}, {1}]", x, y );
	}

	#endregion

	#region StaticFunctions

	/// <summary>
	/// Return the dummyZ triple of the given tile.
	/// </summary>
	/// <remarks>
	/// The DummyZ representation is the coordinate system in R^2 with 3 axis that
	/// are aligned to the principal exagon movement direction.
	/// There are an infinite number of DummyZ representation for each hexagonal tile.
	/// In this function we found the one which minimize the magnitude of the tile position
	/// vector:
	/// 
	///     |dzX| + |dzY| + |dzZ|
	/// 
	/// or, in other word
	/// 
	///     |x+z| + |y-z| + |z|
	/// 
	/// </remarks>
	/// <returns>The z.</returns>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	public static Vector3 DummyZ(int x, int y) {
		if (x*y >= 0) return new Vector3(x,y,0);
		if (Mathf.Abs(x) < Mathf.Abs(y)) return new Vector3(0,y+x,x);
		return new Vector3(x-y,0,-y);
	}

	/// <summary>
	/// Return the dummyZ triple of the given tile.
	/// </summary>
	/// <returns>The z.</returns>
	/// <param name="a">The given tile.</param>
	public static Vector3 DummyZ(HexaTile a) {
		return HexaTile.DummyZ(a.x,a.y);
	}

	/// <summary>
	/// Distance between a and b centres in the euclidean space.
	/// </summary>
	/// <param name="a">The first tile.</param>
	/// <param name="b">The second tile.</param>
	/// <param name="apothem">The hexagon apothem.</param>
	public static float Distance(HexaTile a, HexaTile b, float apothem) {
		return (a.ToEuclidean(apothem) - b.ToEuclidean(apothem)).magnitude;
	}
	
	/// <summary>
	/// The step distance between the hexatiles a and b.
	/// </summary>
	/// <returns>The distance.</returns>
	/// <param name="a">The first tile.</param>
	/// <param name="b">The second tile.</param>
	public static double StepDistance(HexaTile a, HexaTile b) {
		int diffX = a.x - b.x;
		int diffY = a.y - b.y;
		Vector3 dummyZDif = HexaTile.DummyZ(diffX,diffY);
		return Mathf.Abs(dummyZDif.x)+Mathf.Abs(dummyZDif.y)+Mathf.Abs(dummyZDif.z);
	}

	#endregion
	
}