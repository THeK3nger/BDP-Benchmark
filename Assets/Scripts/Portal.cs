// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Portal.cs" company="">
//   
// </copyright>
// <summary>
//   Implement the concept of Portal Edge in a grid-like map.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Runtime.Serialization;

using UnityEngine;

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
    /// Initializes a new instance of the <see cref="Portal"/> class.
    /// </summary>
    /// <param name="square1">
    /// The first square.
    /// </param>
    /// <param name="square2">
    /// The second square.
    /// </param>
    /// <param name="area1">
    /// The area of the first square.
    /// </param>
    /// <param name="area2">
    /// The area of the second square.
    /// </param>
    public Portal(MapSquare square1, MapSquare square2, int area1, int area2)
    {
        // TODO: There can be inconsistencies. It is possible to develop a more robust constructor.
        this.LinkedSquares = new Tuple<MapSquare, MapSquare>(square1, square2);
        this.LinkedAreas = new Tuple<int, int>(area1, area2);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Portal"/> class.
    /// </summary>
    /// <param name="info">
    /// The info.
    /// </param>
    /// <param name="context">
    /// The context.
    /// </param>
    public Portal(SerializationInfo info, StreamingContext context) 
    {
        var firstSquare = (Tuple<int, int>)info.GetValue("LinkedSquaresFirst", typeof(Tuple<int, int>));
        var secondSquare = (Tuple<int, int>)info.GetValue("LinkedSquaresSecond", typeof(Tuple<int, int>));
        this.LinkedSquares = new Tuple<MapSquare, MapSquare>(
            new MapSquare(firstSquare.First, firstSquare.Second), 
            new MapSquare(secondSquare.First, secondSquare.Second));
        this.LinkedAreas = (Tuple<int, int>)info.GetValue("LinkedAreas", typeof(Tuple<int, int>));
    }

    /// <summary>
    /// Gets the linked areas.
    /// </summary>
    /// <value>The linked areas.</value>
    public Tuple<int, int> LinkedAreas { get; private set; }

    /// <summary>
    /// Gets the linked squares.
    /// </summary>
    /// <value>The linked squares.</value>
    public Tuple<MapSquare, MapSquare> LinkedSquares { get; private set; }

    /// <summary>
    /// Gets the middle point.
    /// </summary>
    /// <value>The middle point.</value>
    public Vector2 MidPoint
    {
        get
        {
            return new Vector2(
                (this.LinkedSquares.First.x + this.LinkedSquares.Second.x) * 0.5f, 
                (this.LinkedSquares.First.y + this.LinkedSquares.Second.y) * 0.5f);
        }
    }

    /// <summary>
    /// Distance between portals p1 and p2.
    /// </summary>
    /// <param name="p1">
    /// First Portal.
    /// </param>
    /// <param name="p2">
    /// Second Portal.
    /// </param>
    /// <returns>
    /// The <see cref="float"/>.
    /// </returns>
    public static float Distance(Portal p1, Portal p2)
    {
        if (p1 == null)
        {
            throw new ArgumentNullException("p1");
        }

        if (p2 == null)
        {
            throw new ArgumentNullException("p2");
        }        

        return Vector2.Distance(p1.MidPoint, p2.MidPoint);
    }

    /// <summary>
    /// Distance between a portal p1 and a 2D point.
    /// </summary>
    /// <param name="p1">
    /// A portal.
    /// </param>
    /// <param name="ms">
    /// A map square.
    /// </param>
    /// <returns>
    /// The <see cref="float"/> distance between portal and square.
    /// </returns>
    public static float Distance(Portal p1, MapSquare ms)
    {
        if (p1 == null)
        {
            throw new ArgumentNullException("p1");
        }

        if (ms == null)
        {
            throw new ArgumentNullException("ms");
        }

        return Vector2.Distance(p1.MidPoint, new Vector2(ms.x, ms.y));
    }

    #region OperatorsOverload

    /// <summary>
    /// The this.
    /// </summary>
    /// <param name="area">
    /// The area.
    /// </param>
    /// <returns>
    /// The <see cref="MapSquare"/>.
    /// </returns>
    public MapSquare this[int area]
    {
        get
        {
            if (LinkedAreas.First == area)
            {
                return LinkedSquares.First;
            }

            if (LinkedAreas.Second == area)
            {
                return this.LinkedSquares.Second;
            }

            return null;
        }
    }

    /// <summary>
    /// The this.
    /// </summary>
    /// <param name="ms">
    /// The ms.
    /// </param>
    /// <returns>
    /// The <see cref="int"/>.
    /// </returns>
    public int this[MapSquare ms] 
    {
        get
        {
            if (LinkedSquares.First == ms)
            {
                return this.LinkedAreas.First;
            }

            if (LinkedSquares.Second == ms)
            {
                return this.LinkedAreas.Second;
            }

            return -1;
        }
    }

    #endregion

    /// <summary>
    /// Determines whether this instance is linked with the specified other.
    /// </summary>
    /// <returns>
    /// <c>true</c> if this instance is linked with the specified other; otherwise, <c>false</c>.
    /// </returns>
    /// <param name="other">
    /// Another portal.
    /// </param>
    public bool IsLinkedWith(Portal other)
    {
        if (other == null)
        {
            return false;
        }

        return this.LinkedAreas.First == other.LinkedAreas.First || this.LinkedAreas.First == other.LinkedAreas.Second
               || this.LinkedAreas.Second == other.LinkedAreas.First
               || this.LinkedAreas.Second == other.LinkedAreas.Second;
    }

    #region StandardOverrides

    /// <summary>
    /// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="Portal"/>.
    /// </summary>
    /// <param name="obj">
    /// The <see cref="System.Object"/> to compare with the current <see cref="Portal"/>.
    /// </param>
    /// <returns>
    /// <c>true</c> if the specified <see cref="System.Object"/> is equal to the current <see cref="Portal"/>; otherwise, <c>false</c>.
    /// </returns>
    public override bool Equals(object obj)
    {
        if (obj == null)
        {
            return false;
        }

        var other = (Portal)obj;
        return this.LinkedSquares == other.LinkedSquares;
    }

    /// <summary>
    /// Serves as a hash function for a <see cref="Portal"/> object.
    /// </summary>
    /// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a hash table.</returns>
    public override int GetHashCode()
    {
        return this.LinkedSquares.GetHashCode();
    }

    /// <summary>
    /// Returns a <see cref="System.String"/> that represents the current <see cref="Portal"/>.
    /// </summary>
    /// <returns>A <see cref="System.String"/> that represents the current <see cref="Portal"/>.</returns>
    public override string ToString()
    {
        return string.Format("[Portal: LinkedSquares={0} LinkedAreas={1}]", LinkedSquares, LinkedAreas);
    }

    #endregion

    #region SertializationMethods

    /// <summary>
    /// Implement ISerializable interface.
    /// </summary>
    /// <param name="info">
    /// Serialization Info.
    /// </param>
    /// <param name="context">
    /// Streaming Context.
    /// </param>
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        if (info == null)
        {
            throw new ArgumentNullException("info");
        }

        var linkedSquaresFirst = new Tuple<int, int>(
            this.LinkedSquares.First.x, 
            this.LinkedSquares.First.y);
        var linkedSquaresSecond = new Tuple<int, int>(
            this.LinkedSquares.Second.x, 
            this.LinkedSquares.Second.y);
        info.AddValue("LinkedSquaresFirst", linkedSquaresFirst);
        info.AddValue("LinkedSquaresSecond", linkedSquaresSecond);
        info.AddValue("LinkedAreas", this.LinkedAreas);
    }

    #endregion
}
