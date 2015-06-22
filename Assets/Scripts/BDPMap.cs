// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BDPMap.cs" company="">
//   
// </copyright>
// <summary>
//   Handle a map for a BDP approach.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography.X509Certificates;

using RoomOfRequirement.Architectural;
using RoomOfRequirement.Generic;

using UnityEngine;

/// <summary>
/// Handle a map for a BDP approach.
/// </summary>
/// <remarks>
/// <para>
/// It handles a 2D char representation, a 2D partitioning of the map and
/// the creation of an high-level representation of the map in terms of
/// areas, portals and area and portal connectivity.
/// </para>
/// <para>
/// All this information is computed <b>before</b> the actual pathfinding
/// begins and are stored in a set of graph-like data-structure.
/// </para>
/// <para>
/// The map is loaded by a text assets.
/// </para>
/// </remarks>
public class BDPMap : MonoSingleton<BDPMap>
{
    #region PublicInterface

    /// <summary>
    /// The map file.
    /// </summary>
    public TextAsset MapFile;

    #endregion

    #region InternalState

    /// <summary>
    /// The area connectivity.
    /// </summary>
    private UndirectedGraph<int> areaConnectivity;

    /// <summary>
    /// The decomposed area map labels matrix.
    /// </summary>
    private Grid<int> areaMap;

    /// <summary>
    /// The portal squares.
    /// </summary>
    private Dictionary<MapSquare, bool> portalSquares;

    /// <summary>
    /// The raw map data.
    /// </summary>
    private Map2D rawMap;

    /// <summary>
    /// The reverse portal dictionary.
    /// </summary>
    private Dictionary<MapSquare, List<PortalGroup>> reversePortalDict;

    #endregion

    #region PublicProperties

    /// <summary>
    /// Gets the map height.
    /// </summary>
    /// <value>The height.</value>
    public int Height
    {
        get
        {
            return rawMap.Height;
        }
    }

    /// <summary>
    /// Gets a value indicating whether map is loaded.
    /// </summary>
    public bool MapIsLoaded { get; private set; }

    /// <summary>
    /// Gets the portal connectivity.
    /// </summary>
    public UndirectedLabeledGraph<PortalGroup, bool, double> PortalConnectivity { get; private set; }

    /// <summary>
    /// Gets the portal squares.
    /// </summary>
    public Dictionary<MapSquare, bool>.KeyCollection PortalSquares
    {
        get
        {
            return portalSquares.Keys;
        }
    }

    /// <summary>
    /// Gets the portals number.
    /// </summary>
    public int PortalsNumber
    {
        get
        {
            return portalSquares.Count;
        }
    }

    /// <summary>
    /// Gets the map width.
    /// </summary>
    /// <value>The width.</value>
    public int Width
    {
        get
        {
            return rawMap.Width;
        }
    }

    #endregion

    #region StaticMethods

    /// <summary>
    /// Perform a map partitioning algorithm on the map.
    /// </summary>
    /// <param name="bdpMap">
    /// The desired BDPMap to which compute partition.
    /// </param>
    /// <remarks>
    /// The algorithm is described in the paper
    /// "Improved Heuristics for Optimal Pathfinding on Game Maps"
    /// </remarks>
    /// <returns>
    /// The <see cref="Grid"/> containing the area partitioning of the map.
    /// </returns>
    public void MapPartitioning()
    {
        this.areaMap = new Grid<int>(Height, Width);
        var currentAreaLabel = 1;
        var topMostFreeIdx = GetFirstFreeUnlabeled();

        // While there are still unlabeled squares.
        while (topMostFreeIdx != null)
        {
            var xleft = topMostFreeIdx.x;
            var y = topMostFreeIdx.y;
            var shrunkR = false; // The current area shrunk on the right side?
            var shrunkL = false; // The current area shrunk on the left side?
            while (true)
            {
                var x = xleft;

                // Fill the current horizontal line.
                areaMap[x, y] = currentAreaLabel;
                while (IsLabelFree(x + 1, y) && !IsLabelFree(x + 1, y - 1))
                {
                    x++;
                    areaMap[x, y] = currentAreaLabel;
                }

                // If the line is smaller then the upper one, area is shrunk right.
                if (areaMap[x + 1, y - 1] == currentAreaLabel)
                {
                    shrunkR = true;

                    // Else, if it is already shrunk and try to get bigger, end the area.
                }
                else if (areaMap[x, y - 1] != currentAreaLabel && shrunkR)
                {
                    while (areaMap[x, y] == currentAreaLabel)
                    {
                        // This while undo the last line.
                        areaMap[x, y] = 0;
                        x--;
                    }

                    break;
                }

                // Come back to the first square and go to the next line.
                x = xleft;
                y = y + 1;
                if (y >= Height)
                {
                    break;
                }

                // If the square is not free, move to the right.
                while (!IsLabelFree(x, y) && areaMap[x, y - 1] == currentAreaLabel)
                {
                    x++;
                }

                // If you can move to the left, move to the left.
                while (IsLabelFree(x - 1, y) && !IsLabelFree(x - 1, y - 1))
                {
                    x--;
                }

                // If the line is smaller then the upper one, area is shrunk left.
                if (areaMap[x - 1, y - 1] == currentAreaLabel)
                {
                    shrunkL = true;
                }
                else if (areaMap[x, y - 1] != currentAreaLabel && shrunkL)
                {
                    // Else, if it is already shrunk and try to get bigger, end the area.
                    break;
                }

                // If you cannot find a valid free spot on this line, end the area.
                if (!IsLabelFree(x, y))
                {
                    break;
                }

                xleft = x;
            }

            // Go to the next area.
            currentAreaLabel++;
            topMostFreeIdx = GetFirstFreeUnlabeled();
        }
    }

    #endregion

    /// <summary>
    /// Compute the BDP map.
    /// </summary>
    public void ComputeMap()
    {
        MapIsLoaded = false;
        LoadMapFromFile();
        MapPartitioning();
        ConnectivityGraph();
		Debug.LogWarning ("GATES: " + PortalConnectivity.Vertices.Count ());
        GetComponent<MapRenderer>().StartRenderer();
        MapIsLoaded = true;
         
    }

    /// <summary>
    /// The get adjacent portal gropus.
    /// </summary>
    /// <param name="pg">
    /// The pg.
    /// </param>
    /// <returns>
    /// The <see cref="List"/>.
    /// </returns>
    public List<PortalGroup> GetAdjacentPortalGropus(PortalGroup pg)
    {
        return this.PortalConnectivity.GetNeighbours(pg);
    }

    /// <summary>
    /// The get area.
    /// </summary>
    /// <param name="x">
    /// The x.
    /// </param>
    /// <param name="y">
    /// The y.
    /// </param>
    /// <returns>
    /// The <see cref="int"/>.
    /// </returns>
    public int GetArea(int x, int y)
    {
        if (x >= Width || x < 0 || y >= Height || y < 0)
        {
            return 0;
        }

        return areaMap[x, y];
    }

    /// <summary>
    /// The get area.
    /// </summary>
    /// <param name="ms">
    /// The ms.
    /// </param>
    /// <returns>
    /// The <see cref="int"/>.
    /// </returns>
    public int GetArea(MapSquare ms)
    {
        return GetArea(ms.x, ms.y);
    }

    /// <summary>
    /// Gets the portal group in PortalConnectivity in a given area.
    /// </summary>
    /// <returns>
    /// The portal group by areas.
    /// </returns>
    /// <param name="area">
    /// The desired Area.
    /// </param>
    public List<PortalGroup> GetPortalGroupByAreas(int area)
    {
        return this.PortalConnectivity.Vertices.Where(pg => pg.BelongTo(area)).ToList();
    }

    /// <summary>
    /// Gets the portal groups in PortalConnectivity that link the given areas.
    /// </summary>
    /// <returns>
    /// The portal group by areas.
    /// </returns>
    /// <param name="area1">
    /// THe first Area1.
    /// </param>
    /// <param name="area2">
    /// The second Area2.
    /// </param>
    public List<PortalGroup> GetPortalGroupByAreas(int area1, int area2)
    {
        return this.PortalConnectivity.Vertices.Where(pg => pg.Connect(area1, area2)).ToList();
    }

    /// <summary>
    /// Gets the portal group by square.
    /// </summary>
    /// <returns>
    /// The portal group by square.
    /// </returns>
    /// <param name="ms">
    /// the given map square.
    /// </param>
    public List<PortalGroup> GetPortalGroupBySquare(MapSquare ms)
    {
        return this.reversePortalDict.ContainsKey(ms) ? this.reversePortalDict[ms] : null;
    }

    /// <summary>
    /// Return the state of the desired PortalGroup side defined by the area.
    /// </summary>
    /// <param name="pg">
    /// The queried portal group.
    /// </param>
    /// <param name="area">
    /// The given portal area.
    /// </param>
    /// <returns>
    /// <c>true</c> if the PortalGroup is open; otherwise, <c>false</c>.
    /// </returns>
    public bool GetPortalGroupState(PortalGroup pg, int area)
    {
        if (pg == null)
        {
            throw new ArgumentNullException("pg");
        }

        return pg.Portals.Select(p => this.portalSquares[p[area]]).FirstOrDefault();
    }

    /// <summary>
    /// Check if two tiles lie in the same area.
    /// </summary>
    /// <param name="msA">
    /// The first tile.
    /// </param>
    /// <param name="msB">
    /// The second tile.
    /// </param>
    /// <returns>
    /// Return <c>true</c> is msA and msB lie in the same area. <c>false</c> otherwise. 
    /// </returns>
    public bool HaveSameArea(MapSquare msA, MapSquare msB)
    {
        return areaMap[msA.x, msA.y] == areaMap[msB.x, msB.y];
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

        if (portalSquares.ContainsKey(ms))
        {
            return rawMap.IsFree(ms) && portalSquares[ms];
        }

        return this.rawMap.IsFree(ms);
    }

    /// <summary>
    /// The is portal square.
    /// </summary>
    /// <param name="ms">
    /// The ms.
    /// </param>
    /// <returns>
    /// The <see cref="bool"/>.
    /// </returns>
    public bool IsPortalSquare(MapSquare ms)
    {
        return portalSquares.ContainsKey(ms);
    }

    /// <summary>
    /// Loads the map data from class TextAsset
    /// </summary>
    public void LoadMapFromFile()
    {
        // Load text file from TextAsset.
        string mapString = MapFile.text;
        rawMap = BDP.Map.MapParser.ParseMapFromString(mapString);
    }

    /// <summary>
    /// Debug function who prints an area of the map around a specific map square.
    /// </summary>
    /// <param name="ms">
    /// The center of the printed area.
    /// </param>
    /// <param name="range">
    /// The range of the area.
    /// </param>
    public void PrintStateAround(MapSquare ms, int range)
    {
        string result = "  ";

        // Build Header
        for (var x = ms.x - range; x < ms.x + range; x++)
        {
            result += " " + x;
        }

        result += "\n";
        for (var y = ms.y - range; y < ms.y + range; y++)
        {
            result += y + " ";
            for (int x = ms.x - range; x < ms.x + range; x++)
            {
                if (IsFree(x, y))
                {
                    result += " " + GetArea(x, y);
                }
                else
                {
                    result += " @";
                }
            }

            result += "\n";
        }

        Debug.Log(result);
    }

    /// <summary>
    /// Set the state of the given PortalGroup side lying in the given area.
    /// </summary>
    /// <param name="pg">
    /// The portal group to change.
    /// </param>
    /// <param name="state">
    /// The desired state.
    /// </param>
    /// <param name="area">
    /// The side of the portal group that will be changed.
    /// </param>
    public void SetPortalGroup(PortalGroup pg, bool state, int area)
    {
        if (pg == null)
        {
            throw new ArgumentNullException("pg");
        }

        foreach (var p in pg.Portals)
        {
            this.portalSquares[p[area]] = state;
            //this.portalSquares[p[area, true]] = state;
        }
    }

    public IEnumerable<MapSquare> MapSquares()
    {
        return this.rawMap.MapSquares();
    } 

    #region SertializationMethods

    /// <summary>
    /// The load data.
    /// </summary>
    /// <returns>
    /// The <see cref="bool"/>.
    /// </returns>
    public bool LoadData()
    {
        Stream stream = File.Open("MapCache.cache", FileMode.Open);

        var bformatter = new BinaryFormatter();
        bformatter.Binder = new VersionDeserializationBinder();
        Debug.Log("Reading Data from MapCache.cache");
        var data = (Map2DSerialization)bformatter.Deserialize(stream);
        rawMap = new Map2D(data.RealMap);
        areaMap = data.AreaMap;
        this.PortalConnectivity = data.PortalConnectivity;
        areaConnectivity = data.AreaConnectivity;
        stream.Close();
        Debug.Log("Reading Complete");

        // DebugMessages();
        return true;
    }

    /// <summary>
    /// The serialize data.
    /// </summary>
    public void SerializeData()
    {
        var data = new Map2DSerialization
                       {
                           RealMap = this.rawMap.Map, 
                           AreaMap = this.areaMap, 
                           PortalConnectivity = this.PortalConnectivity, 
                           AreaConnectivity = this.areaConnectivity
                       };

        Stream stream = File.Open("MapCache.cache", FileMode.Create);
        var bformatter = new BinaryFormatter();
        bformatter.Binder = new VersionDeserializationBinder();
        Debug.Log("Writing Information on MapCache.cache");
        bformatter.Serialize(stream, data);
        stream.Close();
    }

    #endregion

    #region ConnectivityComputation

    /// <summary>
    /// Compute the connectivity graph of the map.
    /// </summary>
    public void ConnectivityGraph()
    {
        reversePortalDict = new Dictionary<MapSquare, List<PortalGroup>>();
        areaConnectivity = new UndirectedGraph<int>();
        this.PortalConnectivity = new UndirectedLabeledGraph<PortalGroup, bool, double>();
        portalSquares = new Dictionary<MapSquare, bool>();

        // Column Scan
        // int currentArea;
        // MapSquare currentSquare;
        ColumnConnectivityScan();

        // Row Scan
        RowConnectivityScan();

        FillPortalConnectivity();
    }

    /// <summary>
    /// Returns the first unlabeled free top-leftmost square.
    /// </summary>
    /// <returns>The coordinates of the first free unlabeled top-leftmost square.</returns>
    private MapSquare GetFirstFreeUnlabeled()
    {
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                if (rawMap.IsFree(x, y) && areaMap[x, y] == 0)
                {
                    return new MapSquare(x, y);
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Determines whether the [x,y] label is free and unlabeled.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the [x,y] label is free and unlabeled; otherwise, <c>false</c>.
    /// </returns>
    /// <param name="x">
    /// The x coordinate.
    /// </param>
    /// <param name="y">
    /// The y coordinate.
    /// </param>
    private bool IsLabelFree(int x, int y) 
    {
        return rawMap.IsFree(x, y) && areaMap[x, y] == 0;
    }

    /// <summary>
    /// Add an entry in the areaConnectivity list.
    /// </summary>
    /// <param name="currentArea">
    /// the first area
    /// </param>
    /// <param name="otherArea">
    /// the second, linked, area
    /// </param>
    private void AddToAreaConnectivity(int currentArea, int otherArea)
    {
        if (areaConnectivity == null)
        {
            Debug.LogError("Area Connectivity cannot be null!");
            return;
        }

        areaConnectivity.AddVertex(currentArea);
        areaConnectivity.AddVertex(otherArea);
        areaConnectivity.AddEdge(currentArea, otherArea);
    }

    /// <summary>
    /// Add a new portal square to the portalSquares list. It requires the 
    /// two squares which form a portal.
    /// </summary>
    /// <param name="currentSquare">
    /// the current square
    /// </param>
    /// <param name="otherSquare">
    /// the other square
    /// </param>
    private void AddToPortalSquares(MapSquare currentSquare, MapSquare otherSquare = null)
    {
        if (portalSquares == null)
        {
            Debug.LogError("PortalSquares Cannot Be Null!");
            return;
        }

        if (!(currentSquare == null || this.portalSquares.ContainsKey(currentSquare)))
        {
            portalSquares.Add(currentSquare, true);
        }

        if (!(otherSquare == null || this.portalSquares.ContainsKey(otherSquare)))
        {
            portalSquares.Add(otherSquare, true);
        }
    }

    /// <summary>
    /// Add an entry to the reversePortalDictionary.
    /// </summary>
    /// <param name="currentSquare">
    /// the current square
    /// </param>
    /// <param name="otherSquare">
    /// the other square
    /// </param>
    /// <param name="pg">
    /// the portal group where these two squares belong. 
    /// </param>
    private void AddToReversePortalDictionary(MapSquare currentSquare, MapSquare otherSquare, PortalGroup pg)
    {
        if (currentSquare == null)
        {
            throw new ArgumentNullException("currentSquare");
        }

        if (otherSquare == null)
        {
            throw new ArgumentNullException("otherSquare");
        }

        if (pg == null)
        {
            throw new ArgumentNullException("pg");
        }

        if (reversePortalDict == null)
        {
            Debug.LogError("ReversePortalDict cannot be null!");
            return;
        }

        if (!reversePortalDict.ContainsKey(currentSquare))
        {
            reversePortalDict[currentSquare] = new List<PortalGroup>();
        }

        reversePortalDict[currentSquare].Add(pg);
        if (!reversePortalDict.ContainsKey(otherSquare))
        {
            reversePortalDict[otherSquare] = new List<PortalGroup>();
        }

        reversePortalDict[otherSquare].Add(pg);
    }

    /// <summary>
    /// Scan the map searching for vertical portal groups.
    /// </summary>
    /// TODO: Code duplication with RowConnectivityScan.
    private void ColumnConnectivityScan()
    {
        var portalStrike = false;
        var pg = new PortalGroup();
        for (var x = 0; x < Width; x++)
        {
            for (var y = 0; y < Height; y++)
            {
                // Check left for other areas.
                // UNDONE: If I move right and I don't change area I can avoid left checking.
                var currentArea = GetArea(x, y);
                var leftArea = this.GetArea(x - 1, y);
                var currentSquare = new MapSquare(x, y);
                var leftSquare = new MapSquare(x - 1, y);
                var condOne = rawMap.IsFree(x, y) && leftArea != currentArea && leftArea != 0;
                var condTwo = !portalStrike || (currentArea == GetArea(x, y - 1) && leftArea == GetArea(x - 1, y - 1)) && pg.Portals.Count() < 5;
                if (condOne && condTwo)
                {
                    pg.Add(new Portal(currentSquare, leftSquare, currentArea, leftArea));
                    AddToReversePortalDictionary(currentSquare, leftSquare, pg);
                    AddToAreaConnectivity(currentArea, leftArea);
                    AddToPortalSquares(currentSquare, leftSquare);
                    portalStrike = true;
                }
                else
                {
                    if (!portalStrike)
                    {
                        continue;
                    }

                    portalStrike = false;
                    this.PortalConnectivity.AddVertex(pg);
                    pg = new PortalGroup();

                    if (condOne)
                    {
                        pg.Add(new Portal(currentSquare, leftSquare, currentArea, leftArea));
                        AddToReversePortalDictionary(currentSquare, leftSquare, pg);
                        AddToAreaConnectivity(currentArea, leftArea);
                        AddToPortalSquares(currentSquare, leftSquare);
                        portalStrike = true;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Fill the portal connectivity edges and weights on the basis of the
    /// portal groups found in the previous steps.
    /// </summary>
    private void FillPortalConnectivity()
    {
        foreach (var pg1 in this.PortalConnectivity.Vertices)
        {
            foreach (var pg2 in this.PortalConnectivity.Vertices)
            {
                if (pg1.IsLinkedWith(pg2))
                {
                    this.PortalConnectivity.AddEdge(pg1, pg2);
                    this.PortalConnectivity.SetEdgeLabel(pg1, pg2, PortalGroup.Distance(pg1, pg2));
                }
            }
        }
    }

    /// <summary>
    /// Scan the map searching for horizontal portal groups.
    /// </summary>
    /// TODO: Code duplication with ColumnConnectivityScan.
    private void RowConnectivityScan()
    {
        var portalStrike = false;
        var pg = new PortalGroup();
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                var currentArea = GetArea(x, y);
                var topArea = this.GetArea(x, y - 1);
                var currentSquare = new MapSquare(x, y);
                var topSquare = new MapSquare(x, y - 1);
                var condOne = rawMap.IsFree(x, y) && topArea != currentArea && topArea != 0;
                var condTwo = !portalStrike || (currentArea == GetArea(x - 1, y) && topArea == GetArea(x - 1, y - 1)) && pg.Portals.Count() < 5;
                if (condOne && condTwo)
                {
                    pg.Add(new Portal(currentSquare, topSquare, currentArea, topArea));
                    AddToReversePortalDictionary(currentSquare, topSquare, pg);
                    AddToAreaConnectivity(currentArea, topArea);
                    AddToPortalSquares(currentSquare, topSquare);
                    portalStrike = true;
                }
                else
                {
                    if (!portalStrike)
                    {
                        continue;
                    }

                    portalStrike = false;
                    this.PortalConnectivity.AddVertex(pg);
                    pg = new PortalGroup();

                    if (condOne) {
                        pg.Add(new Portal(currentSquare, topSquare, currentArea, topArea));
                        AddToReversePortalDictionary(currentSquare, topSquare, pg);
                        AddToAreaConnectivity(currentArea, topArea);
                        AddToPortalSquares(currentSquare, topSquare);
                        portalStrike = true;
                    }
                }
            }
        }
    }

    #endregion


    void Update() {
        if (Input.GetKeyDown(KeyCode.G)) {
            DrawConnectivityGraph();
        }
    }

    private void DrawConnectivityGraph()
    {
        foreach (var portalGroup in PortalConnectivity.Vertices)
        {
            Vector3 p1 = MapRenderer.Instance.Grid2Cartesian(portalGroup.MidPortal.LinkedSquares.First);
            foreach (var altro in PortalConnectivity.GetNeighbours(portalGroup))
            {
                Vector3 p2 = MapRenderer.Instance.Grid2Cartesian(altro.MidPortal.LinkedSquares.First);
                Debug.DrawLine(p1, p2,Color.blue,2.0f);
            }
        }
    }

    #region Nested type: Map2DSerialization

    /// <summary>
    /// The map 2 d serialization.
    /// </summary>
    [Serializable]
    private class Map2DSerialization : ISerializable
    {
        /// <summary>
        /// The area connectivity.
        /// </summary>
        public UndirectedGraph<int> AreaConnectivity;

        /// <summary>
        /// The area map.
        /// </summary>
        public Grid<int> AreaMap;

        /// <summary>
        /// The portal connectivity.
        /// </summary>
        public UndirectedLabeledGraph<PortalGroup, bool, double> PortalConnectivity;

        /// <summary>
        /// The real map.
        /// </summary>
        public Grid<char> RealMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="Map2DSerialization"/> class.
        /// </summary>
        public Map2DSerialization()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Map2DSerialization"/> class.
        /// </summary>
        /// <param name="info">
        /// The info.
        /// </param>
        /// <param name="context">
        /// The context.
        /// </param>
        public Map2DSerialization(SerializationInfo info, StreamingContext context)
        {
            this.RealMap = (Grid<char>)info.GetValue("RealMap", typeof(Grid<char>));
            this.AreaMap = (Grid<int>)info.GetValue("AreaMap", typeof(Grid<int>));
            this.AreaConnectivity = (UndirectedGraph<int>)info.GetValue("AreaGraph", typeof(UndirectedGraph<int>));
            this.PortalConnectivity =
                (UndirectedLabeledGraph<PortalGroup, bool, double>)
                info.GetValue("PortalGraph", typeof(UndirectedLabeledGraph<PortalGroup, bool, double>));
        }

        #region ISerializable Members

        /// <summary>
        /// The get object data.
        /// </summary>
        /// <param name="info">
        /// The info.
        /// </param>
        /// <param name="context">
        /// The context.
        /// </param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("RealMap", this.RealMap);
            info.AddValue("AreaMap", this.AreaMap);
            info.AddValue("AreaGraph", this.AreaConnectivity);
            info.AddValue("PortalGraph", this.PortalConnectivity);
        }

        #endregion
    }

    #endregion

}

// This is a SerializationBinder. It is needed by Unity to match the serialized assembly 
// with the current assembly. It is a standard class, no modification should be ever needed.

/// <summary>
/// The version deserialization binder.
/// </summary>
public sealed class VersionDeserializationBinder : SerializationBinder
{
    /// <summary>
    /// The bind to type.
    /// </summary>
    /// <param name="assemblyName">
    /// The assembly name.
    /// </param>
    /// <param name="typeName">
    /// The type name.
    /// </param>
    /// <returns>
    /// The <see cref="Type"/>.
    /// </returns>
    public override Type BindToType(string assemblyName, string typeName)
    {
        if (!string.IsNullOrEmpty(assemblyName) && !string.IsNullOrEmpty(typeName))
        {
            assemblyName = Assembly.GetExecutingAssembly().FullName;

            // The following line of code returns the type.
            Type typeToDeserialize = Type.GetType(string.Format("{0}, {1}", typeName, assemblyName));
            return typeToDeserialize;
        }

        return null;
    }


}
