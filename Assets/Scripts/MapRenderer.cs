using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RoomOfRequirement.Architectural;

/// <summary>
/// Implement a rendering component for a BDPMap map.
/// </summary>
[RequireComponent(typeof(BDPMap))]
public class MapRenderer : MonoSingleton<MapRenderer> {
	
	/// <summary>
	/// The canvas tilemap.
	/// </summary>
	public tk2dTileMap Tilemap;

    /// <summary>
    /// The origin of the grid;
    /// </summary>
    public Vector2 Origin;

    /// <summary>
    /// The size of a grid square;
    /// </summary>
    public float CellSize;


	// Use this for initialization
	void Start () {
		if (Tilemap == null) {
			Debug.LogError("TileMap is not set in the inspector!");
		}
        StartCoroutine(DrawCallback());
	}
	
    /// <summary>
    /// Drawing Coroutine.
    /// </summary>
    /// <returns></returns>
	IEnumerator DrawCallback() {
        while (true) {
            yield return new WaitForSeconds(0.1f);
            if (BDPMap.Instance.MapIsLoaded) {
                DrawAreaMap();
                DrawMap();
            }
        }
	}

	/// <summary>
	/// Draws the map updating the tilemap.
	/// </summary>
	public void DrawMap ()
	{
        for (int x = 0; x < BDPMap.Instance.Width; x++) {
            for (int y = 0; y < BDPMap.Instance.Height; y++) {
                if (!BDPMap.Instance.IsFree(x, y)) {
                    Tilemap.SetTile(x, BDPMap.Instance.Height - y, 0, 0);
				} else {
					//Tilemap.SetTile (x, Height - y, 0, -1);
				}
			}
		}
		Tilemap.Build ();
	}

	/// <summary>
	/// Draws the areas partitioning on the map.
	/// </summary>
	public void DrawAreaMap ()
	{
		int currentColor = 1;
		var areaColor = new Dictionary<int, int> ();
		int currentArea;
        for (int x = 0; x < BDPMap.Instance.Width; x++) {
            for (int y = 0; y < BDPMap.Instance.Height; y++) {
                currentArea = BDPMap.Instance.GetArea(x, y);
				if (currentArea == 0)
					continue;
				if (areaColor.ContainsKey (currentArea)) {
                    Tilemap.SetTile(x, BDPMap.Instance.Height - y, 0, areaColor[currentArea]);
				} else {
					areaColor.Add (currentArea, currentColor + 1);
					currentColor = (currentColor + 1) % 5;
                    Tilemap.SetTile(x, BDPMap.Instance.Height - y, 0, areaColor[currentArea]);
				}
			}
		}
		Tilemap.Build ();
	}

    public Vector2 Grid2Cartesian(MapSquare ms) {
        float x = Origin.x + (1.0f + ms.x) * 0.5f * CellSize;
        float y = Origin.y + (1.0f + ms.y) * 0.5f * CellSize;
        return new Vector2(x, y);
    } 
}
