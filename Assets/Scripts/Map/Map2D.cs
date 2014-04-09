// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Map2D.cs" company="Davide Aversa">
//   MIT License
// </copyright>
// <summary>
//   Store raw information for a 2D map.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

using RoomOfRequirement.Generic;

/// <summary>
/// Store raw information for a 2D map.
/// </summary>
public class Map2D
{

    /// <summary>
    /// The raw map data.
    /// </summary>
    private readonly Grid<char> rawMap;

    /// <summary>
    /// The map square catalog.
    /// </summary>
    private readonly Dictionary<string, string> itemsCatalogue = new Dictionary<string, string>
                                                            {
                                                                { "non-free", "@T" }, 
                                                                { "free", "." }
                                                            };

    /// <summary>
    /// Initializes a new instance of the <see cref="Map2D"/> class. 
    /// Constructor for the Map2D type.
    /// </summary>
    /// <param name="rawMap">
    /// The encapsulated RawMap
    /// </param>
    public Map2D(Grid<char> rawMap)
    {
        if (rawMap == null)
        {
            throw new ArgumentNullException("rawMap");
        }

        this.rawMap = rawMap;
    }

    /// <summary>
    /// Gets the map width.
    /// </summary>
    /// <value>The width.</value>
    public int Width
    {
        get
        {
            return this.rawMap.Width;
        }
    }

    /// <summary>
    /// Gets the map height.
    /// </summary>
    /// <value>The height.</value>
    public int Height
    {
        get
        {
            return this.rawMap.Height;
        }
    }

    /// <summary>
    /// Gets the map.
    /// </summary>
    /// <value>The map.</value>
    public Grid<char> Map
    {
        get
        {
            return this.rawMap;
        }
    }

    /// <summary>
    /// The is free.
    /// </summary>
    /// <param name="x">
    /// The x.
    /// </param>
    /// <param name="y">
    /// The y.
    /// </param>
    /// <returns>
    /// The <see cref="bool"/>.
    /// </returns>
    public bool IsFree(int x, int y)
    {
        return IsFree(new MapSquare(x, y));
    }

    /// <summary>
    /// The is free.
    /// </summary>
    /// <param name="ms">
    /// The ms.
    /// </param>
    /// <returns>
    /// The <see cref="bool"/>.
    /// </returns>
    public bool IsFree(MapSquare ms)
    {
        if (ms == null)
        {
            throw new ArgumentNullException("ms");
        }

        if (ms.x >= this.Width || ms.x < 0 || ms.y >= this.Height || ms.y < 0)
        {
            return false;
        }

        return this.ElementIs("free", this.rawMap[ms.x, ms.y]);
    }

    /// <summary>
    /// Checks if the given element belong to type.
    /// </summary>
    /// <returns>
    /// <c>true</c>, if element is of type, <c>false</c> otherwise.
    /// </returns>
    /// <param name="type">
    /// The elements type.
    /// </param>
    /// <param name="element">
    /// The tested element.
    /// </param>
    public bool ElementIs(string type, char element)
    {
        var elementsList = this.itemsCatalogue[type];
        return elementsList.IndexOf(element) != -1;
    }

    public IEnumerable<MapSquare> MapSquares()
    {
        for (var x = 0; x < this.Width; x++)
        {
            for (var y = 0; y < this.Height; y++)
            {
                yield return new MapSquare(x, y);
            }
        }
    } 
}
