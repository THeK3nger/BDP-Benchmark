// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MapRenderer.cs" company="Davide Aversa">
//   MIT License.
// </copyright>
// <summary>
//   Implement a rendering component for a BDPMap map.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;

using RoomOfRequirement.Architectural;

using UnityEngine;

/// <summary>
/// Implement a rendering component for a BDPMap map.
/// </summary>
[RequireComponent(typeof(BDPMap))]
public class MapRenderer : MonoSingleton<MapRenderer>
{
    /// <summary>
    /// The size of a grid square;
    /// </summary>
    public float CellSize;

    /// <summary>
    /// The origin of the grid;
    /// </summary>
    public Vector2 Origin;

    /// <summary>
    /// The canvas tilemap.
    /// </summary>
    public tk2dTileMap Tilemap;

    /// <summary>
    /// Gets or sets a value indicating whether keep drawing.
    /// </summary>
    public bool KeepDrawing { get; set; }

    /// <summary>
    /// The start.
    /// </summary>
    public void Start() 
    {
        if (Tilemap == null) 
        {
            Debug.LogError("TileMap is not set in the inspector!");
        }

        KeepDrawing = true;
        StartCoroutine(DrawCallback());
    }

    /// <summary>
    /// Draws the areas partitioning on the map.
    /// </summary>
    public void DrawAreaMap()
    {
        var currentColor = 1;
        var areaColor = new Dictionary<int, int>();
        for (var x = 0; x < BDPMap.Instance.Width; x++)
        {
            for (var y = 0; y < BDPMap.Instance.Height; y++)
            {
                var currentArea = BDPMap.Instance.GetArea(x, y);
                if (currentArea == 0)
                {
                    continue;
                }

                if (areaColor.ContainsKey(currentArea))
                {
                    Tilemap.SetTile(x, BDPMap.Instance.Height - y, 0, areaColor[currentArea]);
                }
                else
                {
                    areaColor.Add(currentArea, currentColor + 1);
                    currentColor = (currentColor + 1) % 5;
                    Tilemap.SetTile(x, BDPMap.Instance.Height - y, 0, areaColor[currentArea]);
                }
            }
        }

        Tilemap.Build();
    }

    /// <summary>
    /// Draws the map updating the tilemap.
    /// </summary>
    public void DrawMap()
    {
        for (var x = 0; x < BDPMap.Instance.Width; x++)
        {
            for (var y = 0; y < BDPMap.Instance.Height; y++)
            {
                if (!BDPMap.Instance.IsFree(x, y))
                {
                    Tilemap.SetTile(x, BDPMap.Instance.Height - y, 0, 0);
                }
            }
        }

        Tilemap.Build();
    }

    /// <summary>
    /// The grid 2 cartesian.
    /// </summary>
    /// <param name="ms">
    /// The ms.
    /// </param>
    /// <returns>
    /// The <see cref="Vector2"/>.
    /// </returns>
    public Vector2 Grid2Cartesian(MapSquare ms)
    {
        var fixedy = BDPMap.Instance.Height - ms.y;
        var x = Origin.x + ((1.0f + ms.x) * 0.5f * CellSize);
        var y = Origin.y + ((1.0f + fixedy) * 0.5f * CellSize);
        return new Vector2(x, y);
    }

    /// <summary>
    /// Drawing Co-routine.
    /// </summary>
    /// <returns>
    /// The <see cref="IEnumerator"/>.
    /// </returns>
    private IEnumerator DrawCallback()
    {
        while (KeepDrawing)
        {
            yield return new WaitForSeconds(0.1f);
            if (!BDPMap.Instance.MapIsLoaded)
            {
                continue;
            }

            this.DrawAreaMap();
            this.DrawMap();
        }
    }
}
