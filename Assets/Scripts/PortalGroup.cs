// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PortalGroup.cs" company="Davide Aversa">
//   MIT License
// </copyright>
// <summary>
//   Represent a portal group (a set of consecutive portals).
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using UnityEngine;

/// <summary>
/// Represent a portal group (a set of consecutive portals).
/// </summary>
[Serializable]
public class PortalGroup : ISerializable
{
    /// <summary>
    /// The first.
    /// </summary>
    public Vector2 First = new Vector2(-1, -1);

    /// <summary>
    /// The last.
    /// </summary>
    public Vector2 Last = new Vector2(-1, -1);

    #region InternalState

    /// <summary>
    /// The portals.
    /// </summary>
    private readonly List<Portal> portals;
    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="PortalGroup"/> class.
    /// </summary>
    public PortalGroup()
    {
        this.portals = new List<Portal>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PortalGroup"/> class.
    /// </summary>
    /// <param name="info">
    /// Info serializing.
    /// </param>
    /// <param name="context">
    /// Context serializing.
    /// </param>
    public PortalGroup(SerializationInfo info, StreamingContext context) 
    {
        this.portals = (List<Portal>)info.GetValue("Portals", typeof(List<Portal>));
    }


    /// <summary>
    /// Gets a value indicating whether horizontal.
    /// </summary>
    public bool Horizontal
    {
        get
        {
            return Math.Abs(this.First.y - this.Last.y) < 0.01;
        }
    }

    /// <summary>
    /// Gets the areas linked by this instance.
    /// </summary>
    /// <value>The linked areas.</value>
    public Tuple<int, int> LinkedAreas
    {
        get
        {
            return this.portals.Count > 0 ? this.portals[0].LinkedAreas : null;
        }
    }

    /// <summary>
    /// Gets the portal group middle point.
    /// </summary>
    /// <value>The middle point.</value>
    public Vector2 MidPoint
    {
        get
        {
            var accumulator = new Vector2(0, 0);
            accumulator = this.portals.Aggregate(accumulator, (current, p) => current + p.MidPoint);
            return accumulator / portals.Count;
        }
    }

    /// <summary>
    /// Gets the portals in the portal group.
    /// </summary>
    /// <value>The portals.</value>
    public IEnumerable<Portal> Portals
    {
        get
        {
            return portals;
        }
    }

    public Portal MidPortal
    {
        get
        {
            return this.Portals.ToList()[Mathf.FloorToInt(this.Portals.Count() / 2.0f)];
        }
    }

    /// <summary>
    /// Add a portal to the portal group.
    /// </summary>
    /// <param name="p">
    /// The portals to add.
    /// </param>
    public void Add(Portal p)
    {
        if (p == null)
        {
            throw new ArgumentNullException("p");
        }

        if (portals.Contains(p))
        {
            return;
        }

        this.portals.Add(p);
        var midP = p.MidPoint;

        // Debug.Log("ADD: " + midP);
        if (this.First.x < 0 && this.Last.x < 0)
        {
            this.First = midP;
            this.Last = midP;
        }
        else
        {
            if (Math.Abs(midP.y - this.Last.y) < 0.01)
            {
                if (midP.x < this.First.x)
                {
                    this.First = midP;
                    return;
                }

                if (midP.x > this.Last.x)
                {
                    this.Last = midP;
                }
            }
            else
            {
                if (midP.y < this.First.y)
                {
                    this.First = midP;
                    return;
                }

                if (midP.y > this.Last.y)
                {
                    this.Last = midP;
                }
            }
        }
    }

    /// <summary>
    /// Return the nearest portal to a given 2D point in the current instance.
    /// </summary>
    /// <returns>
    /// The portal.
    /// </returns>
    /// <param name="ms">
    /// The ms.
    /// </param>
    /// <param name="minDist">
    /// Minimum dist.
    /// </param>
    public Portal NearestPortal(MapSquare ms, out double minDist)
    {
        Portal min = null;
        minDist = Mathf.Infinity;
        foreach (var p in portals)
        {
            var newDist = Portal.Distance(p, ms);
            if (!(newDist < minDist))
            {
                continue;
            }

            minDist = newDist;
            min = p;
        }

        return min;
    }

    /// <summary>
    /// Return the nearest portal to a given 2D point in the current instance.
    /// </summary>
    /// <returns>
    /// The portal.
    /// </returns>
    /// <param name="ms">
    /// The ms.
    /// </param>
    public Portal NearestPortal(MapSquare ms)
    {
        double dummy = 0;
        return NearestPortal(ms, out dummy);
    }

    /// <summary>
    /// Determines whether this instance is linked with the specified other.
    /// </summary>
    /// <returns>
    /// <c>true</c> if this instance is linked with the specified other; otherwise, <c>false</c>.
    /// </returns>
    /// <param name="other">
    /// Another portal.
    /// </param>
    public bool IsLinkedWith(PortalGroup other)
    {
        if (other == null)
        {
            return false;
        }

        return this.LinkedAreas.First == other.LinkedAreas.First || this.LinkedAreas.First == other.LinkedAreas.Second
               || this.LinkedAreas.Second == other.LinkedAreas.First
               || this.LinkedAreas.Second == other.LinkedAreas.Second;
    }

    /// <summary>
    /// Check if the current portal group links the specified area1 and area2.
    /// </summary>
    /// <param name="area1">
    /// Area1.
    /// </param>
    /// <param name="area2">
    /// Area2.
    /// </param>
    /// <returns>
    /// The <see cref="bool"/>.
    /// </returns>
    public bool Connect(int area1, int area2)
    {
        return (LinkedAreas.First == area1 && LinkedAreas.Second == area2) || (LinkedAreas.First == area2 && LinkedAreas.Second == area1);
    }

    /// <summary>
    /// Check if the current portal group belongs to a given area.
    /// </summary>
    /// <returns>
    /// <c>true</c>, if to was belonged, <c>false</c> otherwise.
    /// </returns>
    /// <param name="area">
    /// Area.
    /// </param>
    public bool BelongTo(int area)
    {
        return LinkedAreas.First == area || LinkedAreas.Second == area;
    }

    /// <summary>
    /// Check if the portal group contains the given MapSquare.
    /// </summary>
    /// <param name="ms">
    /// Ms.
    /// </param>
    /// <returns>
    /// The <see cref="bool"/>.
    /// </returns>
    public bool Contains(MapSquare ms)
    {
        // A square is contained in a portal group if it fall in the 2-square band
        // that join the first portal of the group with the last portal of the group.
        float edgeMin;
        float edgeMax;
        float mid;
        if (!Horizontal)
        {
            edgeMin = this.First.y;
            edgeMax = this.Last.y;
            mid = this.First.x;

            // Debug.Log(edgeMin + " " + edgeMax + " " + ms);
            if (ms.y >= edgeMin && ms.y <= edgeMax)
            {
                if (ms.x <= mid + 0.51 && ms.x >= mid - 0.51)
                {
                    return true;
                }
            }
        }
        else
        {
            edgeMin = this.First.x;
            edgeMax = this.Last.x;
            mid = this.First.y;

            if (ms.x >= edgeMin && ms.x <= edgeMax)
            {
                if (ms.y <= mid + 0.5 && ms.y >= mid - 0.5)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// The contains old.
    /// </summary>
    /// <param name="ms">
    /// The ms.
    /// </param>
    /// <returns>
    /// The <see cref="bool"/>.
    /// </returns>
    public bool ContainsOld(MapSquare ms)
    {
        return this.Portals.Any(p => p.LinkedSquares.First.Equals(ms) || p.LinkedSquares.Second.Equals(ms));
    }

    /// <summary>
    /// Check if the portal group is a dummy portal group.
    /// </summary>
    /// <returns>
    /// The <see cref="bool"/>.
    /// </returns>
    public bool IsDummy()
    {
        return this.LinkedAreas.First == this.LinkedAreas.Second;
    }

    /// <summary>
    /// The common area.
    /// </summary>
    /// <param name="pgA">
    /// The pg a.
    /// </param>
    /// <param name="pgB">
    /// The pg b.
    /// </param>
    /// <returns>
    /// The <see cref="int"/>.
    /// </returns>
    public static int CommonArea(PortalGroup pgA, PortalGroup pgB)
    {
        if (pgA.LinkedAreas.First == pgB.LinkedAreas.First)
        {
            return pgA.LinkedAreas.First;
        }

        if (pgA.LinkedAreas.First == pgB.LinkedAreas.Second)
        {
            return pgA.LinkedAreas.First;
        }

        if (pgA.LinkedAreas.Second == pgB.LinkedAreas.First)
        {
            return pgA.LinkedAreas.Second;
        }

        if (pgA.LinkedAreas.Second == pgB.LinkedAreas.Second)
        {
            return pgA.LinkedAreas.Second;
        }

        return -1;
    }

    /// <summary>
    /// Compute the distance between the specified pg1 and pg2.
    /// </summary>
    /// <param name="pg1">
    /// Pg1.
    /// </param>
    /// <param name="pg2">
    /// Pg2.
    /// </param>
    /// <returns>
    /// The <see cref="double"/>.
    /// </returns>
    public static double Distance(PortalGroup pg1, PortalGroup pg2)
    {
        // Check if it is a dummy pg
        if (pg1.IsDummy() && pg2.IsDummy())
        {
            return MapSquare.Distance(pg1.portals[0].LinkedSquares.First, pg2.portals[0].LinkedSquares.First);
        }

        if (pg1.IsDummy())
        {
            return PortalGroup.Distance(pg2, pg1.portals[0].LinkedSquares.First);
        }

        if (pg2.IsDummy())
        {
            return PortalGroup.Distance(pg1, pg2.portals[0].LinkedSquares.First);
        }

        return (pg1.MidPoint - pg2.MidPoint).magnitude;
    }

    /// <summary>
    /// Compute the distance between the specified pg1 and a point.
    /// </summary>
    /// <param name="pg1">
    /// Pg1.
    /// </param>
    /// <param name="ms">
    /// The ms.
    /// </param>
    /// <returns>
    /// The <see cref="double"/>.
    /// </returns>
    public static double Distance(PortalGroup pg1, MapSquare ms)
    {
        double minDist = 0;
        pg1.NearestPortal(ms, out minDist);
        return minDist;
    }

    public static bool AreContiguous(PortalGroup pg1, PortalGroup pg2)
    {
        if (pg1.Horizontal != pg2.Horizontal)
        {
            return false;
        }

        if (pg1.Horizontal)
        {
            if (Math.Abs(pg1.MidPoint.y - pg2.MidPoint.y) > 0.1)
            {
                return false;
            }

            if (Math.Abs(pg1.Last.x - (pg2.First.x + 1)) < 0.1 || Math.Abs(pg1.First.x - (pg2.Last.x - 1)) < 0.1)
            {
                return true;
            }
        }
        else
        {
            if (Math.Abs(pg1.MidPoint.x - pg2.MidPoint.x) > 0.1)
            {
                return false;
            }

            if (Math.Abs(pg1.Last.y - (pg2.First.y + 1)) < 0.1 || Math.Abs(pg1.First.y - (pg2.Last.y - 1)) < 0.1) 
            {
                return true;
            }
        }

        return false;
    }

    public static bool LinkSameArea(PortalGroup pg1, PortalGroup pg2)
    {
        return Tuple<int,int>.CommutativeEquals(pg1.LinkedAreas, pg2.LinkedAreas);
    }

    #region StandardOverrides

    /// <summary>
    /// Returns a <see cref="System.String"/> that represents the current <see cref="PortalGroup"/>.
    /// </summary>
    /// <returns>A <see cref="System.String"/> that represents the current <see cref="PortalGroup"/>.</returns>
    public override string ToString()
    {
        return string.Format("[PG: {0}]", LinkedAreas);
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

        info.AddValue("Portals", portals);
    }

    #endregion
}
