using UnityEngine;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System;

/// <summary>
/// Represent a portal group (a set of consecutive portals).
/// </summary>
[Serializable]
public class PortalGroup : ISerializable {

	#region InternalState
	/// <summary>
	/// The portals.
	/// </summary>
	List<Portal> portals;
	#endregion

	public Vector2 first = new Vector2(-1,-1);
	public Vector2 last = new Vector2(-1,-1);

	public bool Horizontal {
		get {
			if (Math.Abs (first.y - last.y) < 0.01)
				return true;
			return false;
		}
	}

	#region Constructors
	/// <summary>
	/// Initializes a new instance of the <see cref="PortalGroup"/> class.
	/// </summary>
	public PortalGroup() {
		portals = new List<Portal>();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="PortalGroup"/> class.
	/// </summary>
	/// <param name="info">Info.</param>
	/// <param name="context">Context.</param>
	public PortalGroup(SerializationInfo info, StreamingContext context) {
		portals = (List<Portal>) info.GetValue("Portals",typeof(List<Portal>));
	}
	#endregion

	/// <summary>
	/// Gets the areas linked by this instance.
	/// </summary>
	/// <value>The linked areas.</value>
	public Tuple<int,int> LinkedAreas {
		get { 
			if (portals.Count>0) {
				return portals[0].LinkedAreas;
			}
			return null;
		}
	}

	/// <summary>
	/// Get the portal group middle point.
	/// </summary>
	/// <value>The middle point.</value>
	public Vector2 MidPoint {
		get {
			Vector2 accumulator = new Vector2(0,0);
			foreach (Portal p in portals) {
				accumulator += p.MidPoint;
			}
			return accumulator / portals.Count;
		}
	}

	/// <summary>
	/// Gets the portals in the portal group.
	/// </summary>
	/// <value>The portals.</value>
	public IEnumerable<Portal> Portals {
		get { return portals; }
	}

	/// <summary>
	/// Add a portal to the portal group.
	/// </summary>
	/// <param name="p">P.</param>
	public void Add(Portal p) {
		if (!portals.Contains(p)) {
			portals.Add(p);
			Vector2 midP = p.MidPoint;
			//Debug.Log("ADD: " + midP);
			if (first.x < 0 && last.x < 0) {
				first = midP;
				last = midP;
			} else {
				if (Math.Abs (midP.y - last.y) < 0.01) {
					if (midP.x < first.x) {
						first=midP;
						return;
					}
					if (midP.x > last.x) {
						last = midP;
						return;
					}
				} else {
					if (midP.y < first.y) {
						first = midP;
						return;
					}
					if (midP.y > last.y) {
						last = midP;
						return;
					}
				}
			}
		}
	}

	/// <summary>
	/// Return the nearest portal to a given 2D point in the current instance.
	/// </summary>
	/// <returns>The portal.</returns>
	/// <param name="point">Point.</param>
	/// <param name="minDist">Minimum dist.</param>
	public Portal NearestPortal(MapSquare ms, out double minDist) {
		Portal min = null;
		minDist = Mathf.Infinity;
		foreach (Portal p in portals) {
			float newDist = Portal.Distance(p,ms);
			if (newDist < minDist) {
				minDist = newDist;
				min = p;
			}
		}
		return min;
	}

	/// <summary>
	/// Return the nearest portal to a given 2D point in the current instance.
	/// </summary>
	/// <returns>The portal.</returns>
	/// <param name="point">Point.</param>
	public Portal NearestPortal(MapSquare ms) {
		double dummy = 0;
		return NearestPortal(ms,out dummy);
	}

	/// <summary>
	/// Determines whether this instance is linked with the specified other.
	/// </summary>
	/// <returns><c>true</c> if this instance is linked with the specified other; otherwise, <c>false</c>.</returns>
	/// <param name="other">Another portal.</param>
	public bool IsLinkedWith (PortalGroup other)
	{
		if (other == null) return false;
		return this.LinkedAreas.First == other.LinkedAreas.First ||
			this.LinkedAreas.First == other.LinkedAreas.Second || 
				this.LinkedAreas.Second == other.LinkedAreas.First ||
				this.LinkedAreas.Second == other.LinkedAreas.Second;
	}

	/// <summary>
	/// Check if the current portal group links the specified area1 and area2.
	/// </summary>
	/// <param name="area1">Area1.</param>
	/// <param name="area2">Area2.</param>
	public bool Connect(int area1, int area2) {
		Tuple<int,int> linked = LinkedAreas;
		return (linked.First == area1 && linked.Second == area2) ||
			(linked.First == area2 && linked.Second == area1);
	}

	/// <summary>
	/// Check if the current portal group belongs to a given area.
	/// </summary>
	/// <returns><c>true</c>, if to was belonged, <c>false</c> otherwise.</returns>
	/// <param name="area">Area.</param>
	public bool BelongTo(int area) {
		Tuple<int,int> linked = LinkedAreas;
		return linked.First == area || linked.Second == area;
	}

	/// <summary>
	/// Check if the portal group contains the given MapSquare.
	/// </summary>
	/// <param name="ms">Ms.</param>
	public bool Contains(MapSquare ms) {
		// A square is contained in a portal group if it fall in the 2-square band
		// that join the first portal of the group with the last portal of the group.
		float edgeMin;
		float edgeMax;
		float mid;
		if (!Horizontal) {
			edgeMin = first.y;
			edgeMax = last.y;
			mid = first.x;
			//Debug.Log(edgeMin + " " + edgeMax + " " + ms);
			if (ms.y >= edgeMin && ms.y <= edgeMax) {
				if (ms.x <= mid+0.51 && ms.x >= mid-0.51) {
					return true;
				}
			}
		} else {
			edgeMin = first.x;
			edgeMax = last.x;
			mid = first.y;
//			Debug.Log(edgeMin + " " + edgeMax + " " + mid + " " + ms);
			if (ms.x >= edgeMin && ms.x <= edgeMax) {
				if (ms.y <= mid+0.5 && ms.y >= mid-0.5) {
					return true;
				}
			}
		}
		return false;


//		foreach (Portal p in Portals) {
//			if (p.LinkedSquares.First.Equals(ms) || p.LinkedSquares.Second.Equals(ms)) 
//				return true;
//		}
//		return false;
	}

	public bool ContainsOld(MapSquare ms) {
		foreach (Portal p in Portals) {
			if (p.LinkedSquares.First.Equals(ms) || p.LinkedSquares.Second.Equals(ms)) 
				return true;
		}
		return false;
	}

    /// <summary>
    /// Check if the portal group is a dummy portal group.
    /// </summary>
    /// <returns></returns>
    public bool IsDummy() {
        return (this.LinkedAreas.First == this.LinkedAreas.Second);
    }

    public static int CommonArea(PortalGroup pgA,PortalGroup pgB) {
        if (pgA.LinkedAreas.First == pgB.LinkedAreas.First) return pgA.LinkedAreas.First;
        if (pgA.LinkedAreas.First == pgB.LinkedAreas.Second) return pgA.LinkedAreas.First;
        if (pgA.LinkedAreas.Second == pgB.LinkedAreas.First) return pgA.LinkedAreas.Second;
        if (pgA.LinkedAreas.Second == pgB.LinkedAreas.Second) return pgA.LinkedAreas.Second;
        return -1;
    }

	/// <summary>
	/// Compute the distance between the specified pg1 and pg2.
	/// </summary>
	/// <param name="pg1">Pg1.</param>
	/// <param name="pg2">Pg2.</param>
	public static double Distance(PortalGroup pg1, PortalGroup pg2) {
        // Check if it is a dummy pg
        if (pg1.IsDummy() && pg2.IsDummy()) {
            return MapSquare.Distance(pg1.portals[0].LinkedSquares.First, pg2.portals[0].LinkedSquares.First);
        }
        if (pg1.IsDummy()) {
            return PortalGroup.Distance(pg2, pg1.portals[0].LinkedSquares.First);
        }
        if (pg2.IsDummy()) {
            return PortalGroup.Distance(pg1, pg2.portals[0].LinkedSquares.First);
        }
		return (pg1.MidPoint-pg2.MidPoint).magnitude;
	}

	/// <summary>
	/// Compute the distance between the specified pg1 and a point.
	/// </summary>
	/// <param name="pg1">Pg1.</param>
	/// <param name="point">Point.</param>
	public static double Distance(PortalGroup pg1, MapSquare ms) {
		double minDist = 0;
		pg1.NearestPortal(ms,out minDist);
		return minDist;
	}

	#region StandardOverrides
	/// <summary>
	/// Returns a <see cref="System.String"/> that represents the current <see cref="PortalGroup"/>.
	/// </summary>
	/// <returns>A <see cref="System.String"/> that represents the current <see cref="PortalGroup"/>.</returns>
	public override string ToString ()
	{
		return string.Format ("[PG: {0}]", LinkedAreas);
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
		info.AddValue("Portals",portals);
	}
	#endregion

}
