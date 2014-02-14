using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using RoomOfRequirement.Generic;

public class Map2D {

    /// <summary>
    /// Gets the map width.
    /// </summary>
    /// <value>The width.</value>
    public int Width {
        get {
            return rawMap.Width;
        }
    }

    /// <summary>
    /// Gets the map height.
    /// </summary>
    /// <value>The height.</value>
    public int Height {
        get {
            return rawMap.Height;
        }
    }

    /// <summary>
    /// Gets the map.
    /// </summary>
    /// <value>The map.</value>
    public Grid<char> Map {
        get { return rawMap; }
    }

    /// <summary>
    /// The raw map data.
    /// </summary>
    Grid<char> rawMap;

    /// <summary>
    /// The map square catalog.
    /// </summary>
    Dictionary<string, string> itemsCatalogue = new Dictionary<string, string>
    {
        {"non-free","@T"},
        {"free","."}
    };

    /// <summary>
    /// Constructor for the Map2D type.
    /// </summary>
    /// <param name="rawMap"></param>
    public Map2D(Grid<char> rawMap) {
        this.rawMap = rawMap;
    }

    /// <summary>
    /// Determines whether the <x,y> square is free.
    /// </summary>
    /// <returns><c>true</c> if the <x,y> square is free; otherwise, <c>false</c>.</returns>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    public bool IsFree(int x, int y) {
        return IsFree(new MapSquare(x, y));
    }

    public bool IsFree(MapSquare ms) {
        if (ms.x >= Width || ms.x < 0 || ms.y >= Height || ms.y < 0)
            return false;
        return ElementIs("free", rawMap[ms.x, ms.y]);

    }

    /// <summary>
    /// Checks if the given element belong to type.
    /// </summary>
    /// <returns><c>true</c>, if element is of type, <c>false</c> otherwise.</returns>
    /// <param name="type">The elements type.</param>
    /// <param name="element">The tested element.</param>
    public bool ElementIs(string type, char element) {
        string elementsList = itemsCatalogue[type];
        return elementsList.IndexOf(element) != -1;
    }


}
