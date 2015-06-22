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
    /// Gets or sets a value indicating whether keep drawing.
    /// </summary>
    public bool KeepDrawing { get; set; }

    public List<Color> AreaColors = new List<Color>();

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Texture2D texture;

    /// <summary>
    /// The start.
    /// </summary>
    public void Start() 
    {

    }

    public void StartRenderer() {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        KeepDrawing = true;
        meshFilter.mesh = CreateMapCanvas();
        texture = new Texture2D(BDPMap.Instance.Width, BDPMap.Instance.Height, TextureFormat.ARGB32, false);
        texture.filterMode = FilterMode.Point;
        meshRenderer.material.mainTexture = this.texture;
        StartCoroutine(DrawCallback());
    }

    /// <summary>
    /// Draws the areas partitioning on the map.
    /// </summary>
    public void DrawAreaMap()
    {
        var currentColor = 0;
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

                Color choosenColor;
                if (areaColor.ContainsKey(currentArea))
                {
                    choosenColor = AreaColors[areaColor[currentArea]];
                }
                else
                {
                    areaColor.Add(currentArea, currentColor);
                    currentColor = (currentColor + 1) % AreaColors.Count;
                    choosenColor = AreaColors[currentColor];
                }

                texture.SetPixel(x, BDPMap.Instance.Height - y, choosenColor);
            }
        }
        texture.Apply();

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
                    this.texture.SetPixel(x,BDPMap.Instance.Height - y,Color.black);
                }
            }
        }
        this.texture.Apply();

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

    private Mesh CreateMapCanvas() {
        Mesh mesh = new Mesh();
        // Setup vertices
		Vector3[] newVertices = new Vector3[4];
        float halfHeight = BDPMap.Instance.Height * 0.5f;
        float halfWidth = BDPMap.Instance.Width * 0.5f;
		newVertices [0] = new Vector3 (-halfWidth, -halfHeight, 0);
		newVertices [1] = new Vector3 (-halfWidth, halfHeight, 0);
		newVertices [2] = new Vector3 (halfWidth, -halfHeight, 0);
		newVertices [3] = new Vector3 (halfWidth, halfHeight, 0);

        // Setup UVs
        Vector2[] newUVs = new Vector2[newVertices.Length];
        newUVs[0] = new Vector2(0, 0);
        newUVs[1] = new Vector2(0, 1);
        newUVs[2] = new Vector2(1, 0);
        newUVs[3] = new Vector2(1, 1);

        // Setup triangles
        int[] newTriangles = new int[] { 0, 1, 2, 3, 2, 1 };

        // Setup normals
        Vector3[] newNormals = new Vector3[newVertices.Length];
        for (int i = 0; i < newNormals.Length; i++) {
            newNormals[i] = Vector3.forward;
        }

        // Create quad
        mesh.vertices = newVertices;
        mesh.uv = newUVs;
        mesh.triangles = newTriangles;
        mesh.normals = newNormals;

        return mesh;
    }
}
