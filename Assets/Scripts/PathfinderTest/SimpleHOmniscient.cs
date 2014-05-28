using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

using RoomOfRequirement.Search;

public class SimpleHOmniscient {

	public static int FindPath(MapSquare start, MapSquare target) {
		if (start == null)
		{
			throw new ArgumentNullException("start");
		}
		
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}

		int counter = 0;
		var path = AStar.FindPath(
			start, 
			target, 
			(m1) => { return SimpleHOmniscient.GetNeighbours(m1,target); },
			(m1, m2) =>
			{
			var p1 = BDPMap.Instance.GetPortalGroupBySquare(m1);
			var p2 = BDPMap.Instance.GetPortalGroupBySquare(m2);
			if (p1 != null && p2 != null)
			{
				return SimpleHOmniscient.PGBeliefDistance(p1[0], p2[0]);
			}
			return MapSquare.Distance(m1, m2);
		}, 
		(ms) => MapSquare.Distance(ms, target));
		counter += AStar.ExpandedNodes;
		if (path == null) return counter;

		// LOW LEVEL EXPANSION.
		var pathList = PathfindTester.Path2MapSquareList(path);

		for (var i=1; i<pathList.Count(); i++) {
			var area = BDPMap.Instance.GetArea(pathList[i-1]);
			var lowlevel = AStar.FindPath(
				pathList[i-1], 
				pathList[i], 
				(ms) => GetLowLevelNeighbours(ms).Where(m => BDPMap.Instance.GetArea(m) == area || m == target).ToList(),
				MapSquare.Distance, 
				(ms) => MapSquare.Distance(ms, target));
			counter += AStar.ExpandedNodes;
			if (lowlevel == null) return counter;
		}

		return counter;
	}

	/// <summary>
	/// The get neighbours.
	/// </summary>
	/// <param name="ms">
	/// The ms.
	/// </param>
	/// <returns>
	/// The <see cref="List"/>.
	/// </returns>
	public static List<MapSquare> GetNeighbours(MapSquare ms, MapSquare target)
	{
		var originalMap = BDPMap.Instance;
		var result = new List<MapSquare>();
		var area = originalMap.GetArea(ms);
		
		var pgs = originalMap.GetPortalGroupByAreas(area);
		if (pgs == null)
		{
			Debug.LogError("BOOM");
		}

		var candidates = (from pg in pgs where SimpleHOmniscient.IsPassable(pg) select SimpleHOmniscient.MidPortal(pg)).Select(p => p[area, reverse: true]);
		
		result.AddRange(candidates);
		
		if (area == originalMap.GetArea(target) && !result.Contains(target))
		{
			result.Add(target);
		}
		
		return result;
	}

	private static bool IsPassable(PortalGroup pg)
	{
		//var ps = pg.Portals;
		//foreach (var p in ps) {
		//	if (!IsPassable(p))return false;
		//}
		//return true;
		return IsPassable(SimpleHOmniscient.MidPortal(pg));
	}

	private static bool IsPassable(Portal p)
	{
		return BDPMap.Instance.IsFree(p.LinkedSquares.First) && BDPMap.Instance.IsFree(p.LinkedSquares.Second);
	}

	public static Portal MidPortal(PortalGroup pg) {
		return pg.Portals.ToList()[Mathf.FloorToInt(pg.Portals.Count() / 2.0f)];
	}

	public static double PGBeliefDistance(PortalGroup pg1, PortalGroup pg2)
	{
		if (pg1 == pg2)
		{
			return 0;
		}
		
		var d = BDPMap.Instance.PortalConnectivity.GetEdgeLabel(pg1, pg2);
		if (Math.Abs(d) > 0.0001 && BDPMap.Instance.PortalConnectivity.GetVertexLabel(pg1)
		    && BDPMap.Instance.PortalConnectivity.GetVertexLabel(pg2))
		{
			return d;
		}
		
		return Mathf.Infinity;
	}

	public static List<MapSquare> GetLowLevelNeighbours(MapSquare ms)
	{
		if (ms == null)
		{
			throw new ArgumentNullException("ms");
		}
		
		var result = new List<MapSquare>();
		
		// Add up.
		if (BDPMap.Instance.IsFree(ms.Up))
		{
			result.Add(ms.Up);
		}
		
		// Add down.
		if (BDPMap.Instance.IsFree(ms.Down))
		{
			result.Add(ms.Down);
		}
		
		// Add left.
		if (BDPMap.Instance.IsFree(ms.Left))
		{
			result.Add(ms.Left);
		}
		
		// Add right.
		if (BDPMap.Instance.IsFree(ms.Right))
		{
			result.Add(ms.Right);
		}
		
		return result;
	}

}
