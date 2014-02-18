using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Implement a rendering component for a BDPMap map.
/// </summary>
[RequireComponent(typeof(BDPMap))]
public class MapRenderer : MonoBehaviour {
	
	/// <summary>
	/// The canvas tilemap.
	/// </summary>
	public tk2dTileMap Tilemap;

    /// <summary>
    /// The original BDPMap map.
    /// </summary>
    BDPMap baseMap;

	// Use this for initialization
	void Start () {
		if (Tilemap == null) {
			Debug.LogError("TileMap is not set in the inspector!");
		}
        baseMap = GetComponent<BDPMap>();
        StartCoroutine(DrawCallback());
	}
	
    /// <summary>
    /// Drawing Coroutine.
    /// </summary>
    /// <returns></returns>
	IEnumerator DrawCallback() {
        while (true) {
            yield return new WaitForSeconds(1.0f);
            if (baseMap.MapIsLoaded) {
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
		for (int x = 0; x < baseMap.Width; x++) {
			for (int y = 0; y < baseMap.Height; y++) {
				if (!baseMap.IsFree(x,y)) {
					Tilemap.SetTile (x, baseMap.Height - y, 0, 0);
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
		for (int x = 0; x < baseMap.Width; x++) {
			for (int y = 0; y < baseMap.Height; y++) {
				currentArea = baseMap.Areas [x,y];
				if (currentArea == 0)
					continue;
				if (areaColor.ContainsKey (currentArea)) {
					Tilemap.SetTile (x, baseMap.Height - y, 0, areaColor [currentArea]);
				} else {
					areaColor.Add (currentArea, currentColor + 1);
					currentColor = (currentColor + 1) % 5;
					Tilemap.SetTile (x, baseMap.Height - y, 0, areaColor [currentArea]);
				}
			}
		}
		Tilemap.Build ();
	}
}
