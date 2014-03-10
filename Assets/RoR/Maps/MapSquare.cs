// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MapSquare.cs" company="Davide Aversa">
//   MIT License
// </copyright>
// <summary>
//   Representation for a grid square. It store various information about a 2D grid square.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;

using UnityEngine;

/// <summary>
/// Representation for a grid square. It store various information about a 2D grid square.
/// </summary>
public class MapSquare
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MapSquare"/> class.
    /// </summary>
    /// <param name="x">
    /// The x coordinate.
    /// </param>
    /// <param name="y">
    /// The y coordinate.
    /// </param>
    public MapSquare(int x, int y) 
    {
        this.x = x;
        this.y = y;
    }

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

    /// <summary>
    /// Gets the MapSquare above the current one.
    /// </summary>
    /// <value>The upper MapSquare.</value>
    public MapSquare Up
    {
        get
        {
            return new MapSquare(x, y - 1);
        }
    }

    /// <summary>
    /// Gets the MapSquare below the current one.
    /// </summary>
    /// <value>The bottom MapSquare.</value>
    public MapSquare Down
    {
        get
        {
            return new MapSquare(x, y + 1);
        }
    }

    /// <summary>
    /// Gets the MapSquare on the left of the current one.
    /// </summary>
    /// <value>The left.</value>
    public MapSquare Left
    {
        get
        {
            return new MapSquare(x - 1, y);
        }
    }

    /// <summary>
    /// Gets the MapSquare on the left of the current one.
    /// </summary>
    /// <value>The right.</value>
    public MapSquare Right
    {
        get
        {
            return new MapSquare(x + 1, y);
        }
    }

    /// <summary>
    /// Distance between specified ms1 and ms2.
    /// </summary>
    /// <param name="ms1">
    /// The first MapSquare.
    /// </param>
    /// <param name="ms2">
    /// The second MapSquare.
    /// </param>
    /// <returns>
    /// The <see cref="double"/>.
    /// </returns>
    public static double Distance(MapSquare ms1, MapSquare ms2)
    {
        if (ms1 == null)
        {
            throw new ArgumentNullException("ms1");
        }

        if (ms2 == null)
        {
            throw new ArgumentNullException("ms2");
        }

        return Mathf.Sqrt(Mathf.Pow(ms1.x - ms2.x, 2) + Mathf.Pow(ms1.x - ms2.x, 2));
    }

    /// <summary>
    /// Manhattans distance between specified ms1 and ms2.
    /// </summary>
    /// <returns>
    /// The manhattan distance between ms1 and ms2.
    /// </returns>
    /// <param name="ms1">
    /// The first MapSquare.
    /// </param>
    /// <param name="ms2">
    /// The second MapSquare.
    /// </param>
    public static double ManhattanDistance(MapSquare ms1, MapSquare ms2)
    {
        if (ms1 == null)
        {
            throw new ArgumentNullException("ms1");
        }

        if (ms2 == null)
        {
            throw new ArgumentNullException("ms2");
        }

        return Mathf.Abs(ms1.x - ms2.x) + Mathf.Abs(ms1.x - ms2.x);
    }

    /// <summary>
    /// The ==.
    /// </summary>
    /// <param name="a">
    /// The a.
    /// </param>
    /// <param name="b">
    /// The b.
    /// </param>
    /// <returns>
    /// True if and only if a is equal to b.
    /// </returns>
    public static bool operator ==(MapSquare a, MapSquare b)
    {
        // If both are null, or both are same instance, return true.
        if (object.ReferenceEquals(a, b))
        {
            return true;
        }

        // If one is null, but not both, return false.
        if (((object)a == null) || ((object)b == null))
        {
            return false;
        }

        // Return true if the fields match:
        return a.Equals(b);
    }

    /// <summary>
    /// The !=.
    /// </summary>
    /// <param name="a">
    /// The a.
    /// </param>
    /// <param name="b">
    /// The b.
    /// </param>
    /// <returns>
    /// True if and only if a is different from b
    /// </returns>
    public static bool operator !=(MapSquare a, MapSquare b)
    {
        return !(a == b);
    }

    /// <summary>
    /// The to vector 2.
    /// </summary>
    /// <returns>
    /// The <see cref="Vector2"/>.
    /// </returns>
    public Vector2 ToVector2() 
    {
        return new Vector2(x, y);
    }

    /// <summary>
    /// Transform the square back to the euclidean coordinate.
    /// </summary>
    /// <returns>
    /// The euclidean coordinate for the tile centre.
    /// </returns>
    /// <param name="size">
    /// The size of the square side.
    /// </param>
    public Vector2 ToEuclidean(float size) 
    {
        var ex = size * (0.5f + x);
        var ey = size * (0.5f + y);
        return new Vector2(ex, ey);
    }

    /// <summary>
    /// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="MapSquare"/>.
    /// </summary>
    /// <param name="obj">
    /// The <see cref="System.Object"/> to compare with the current <see cref="MapSquare"/>.
    /// </param>
    /// <returns>
    /// <c>true</c> if the specified <see cref="System.Object"/> is equal to the current <see cref="MapSquare"/>;
    /// otherwise, <c>false</c>.
    /// </returns>
    public override bool Equals(object obj) 
    {
        if (obj == null) 
        {
            return false;
        }

        var other = (MapSquare)obj;
        return (x == other.x) && (y == other.y);
    }

    /// <summary>
    /// Serves as a hash function for a <see cref="MapSquare"/> object.
    /// </summary>
    /// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data 
    /// structures such as a hash table.</returns>
    public override int GetHashCode()
    {
        return (x * 11) + y;
    }

    /// <summary>
    /// Returns a <see cref="System.String"/> that represents the current <see cref="MapSquare"/>.
    /// </summary>
    /// <returns>A <see cref="System.String"/> that represents the current <see cref="MapSquare"/>.</returns>
    public override string ToString()
    {
        return string.Format("[{0}, {1}]", x, y);
    }
}
