using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Reflection;
using System;
using UnityEngine;
using RoomOfRequirement.Generic;

/// <summary>
/// Contains and manages a 2D grid map.
/// </summary>
public class Map2D : MonoBehaviour
{

#region PublicInterface
	/// <summary>
	/// The map file.
	/// </summary>
	public TextAsset MapFile;

	/// <summary>
	/// The compute graph on sta
	/// </summary>
	public bool ComputeGraphOnStart;
#endregion

#region InternalState
	/// <summary>
	/// The raw map data.
	/// </summary>
	Grid<char> rawMap;

	/// <summary>
	/// The decomposed area map labels matrix.
	/// </summary>
	Grid<int> areaMap;

	/// <summary>
	/// The area connectivity.
	/// </summary>
	UndirectedGraph<int> areaConnectivity;

	/// <summary>
	/// The portal connectivity.
	/// </summary>
	UndirectedLabeledGraph<PortalGroup,bool,double> portalConnectivity;

	/// <summary>
	/// The portal squares.
	/// </summary>
	Dictionary<MapSquare,bool> portalSquares;

	Dictionary<MapSquare,List<PortalGroup>> reversePortalDict;
#endregion

#region PublicProperties
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
	/// Gets the areas map.
	/// </summary>
	/// <value>The areas map.</value>
	public Grid<int> Areas {
		get { return areaMap; }
	}

	public UndirectedGraph<int> AreaConnectivity {
		get {
			return areaConnectivity;
		}
	}

	public UndirectedLabeledGraph<PortalGroup, bool, double> PortalConnectivity {
		get {
			return portalConnectivity;
		}
	}

	public Dictionary<MapSquare, bool> PortalSquares {
		get {
			return portalSquares;
		}
	}

#endregion

	/// <summary>
	/// The map square catalogue.
	/// </summary>
	Dictionary<string, string> itemsCatalogue = new Dictionary<string, string>
    {
        {"unpassable","@T"},
        {"free","."}
    };

	// Unity Start Function
	void Awake ()
	{
		try {
			LoadData();
		} catch (FileNotFoundException) {
			Debug.LogWarning("No map in cache. I'll load it now. Wait please.");
			LoadMapFromFile();
			MapPartitioning();
			ConnectivityGraph ();
		}
		if (ComputeGraphOnStart) {
			ConnectivityGraph ();
		}
	}

	public void ComputeMap() {
		LoadMapFromFile();
		MapPartitioning();
		ConnectivityGraph();
	}

	/// <summary>
	/// Loads the map data from class TextAsset
	/// </summary>
	public void LoadMapFromFile ()
	{
		// Load text file from TextAsset.
		string map_string = MapFile.text;
		string[] lines = map_string.Split ('\n');
		// Parse map size.
		int lidx = 0; // Parsing line index
		string current = "";
		int height = 0;
		int width = 0;
		while (current != "map") {
			current = lines [lidx];
			string[] par = current.Split (' '); // Map Parameters, Ignore "type".
			switch (par [0]) {
			case "height":
				height = int.Parse (par [1]);
				break;
			case "width":
				width = int.Parse (par [1]);
				break;
			default:
				break;
			}
			lidx++;
		}
		if (height == 0 || width == 0) {
			Debug.LogError ("Invalid Map Format!");
			return;
		}
		// Initialize map array
		rawMap = new Grid<char>(height,width);
		// Fill the map 
		for (int line = lidx; line < lines.Length; line++) {
			string map_items = lines [line];
			for (int j = 0; j < map_items.Length; j++) {
				rawMap [j,line-lidx] = map_items [j];
			}
		}
//		if (i < heigth * width - 1) {
//			Debug.LogError ("Invalid Map Format!");
//			return;
//		}
	}

	/// <summary>
	/// Perform a map partitioning algorithm on the map.
	/// </summary>
	/// <remarks>
	/// The algorithm is described in the paper
	/// "Improved Heuristics for Optimal Pathfinding on Game Maps"
	/// </remarks>
	// TODO: Maybe it's better to return an areaMap and than assign it. It's more clear.
	public void MapPartitioning ()
	{
		areaMap = new Grid<int>(Height,Width);
		int currentAreaLabel = 1;
		bool shrunkR; // The current area shrunk on the right side?
		bool shrunkL; // The current area shrunk on the left side?
		int x, xLeft, y;
		var topMostFreeIdx = GetFirstFreeUnlabeled ();
		// While there are still unlabeled squares.
		while (topMostFreeIdx != null) {
			xLeft = topMostFreeIdx.First;
			y = topMostFreeIdx.Second;
			shrunkR = false;
			shrunkL = false;
			while (true) {
				x = xLeft;
				// Fill the current horizontal line.
				areaMap [x, y] = currentAreaLabel;
				while (IsLabelFree(x+1,y) && !IsLabelFree(x+1,y-1)) {
					x++;
					areaMap [x, y] = currentAreaLabel;
				}
				// If the line is smaller then the upper one, area is shrunk right.
				if (areaMap[x + 1, y - 1] == currentAreaLabel) {
					shrunkR = true;
					// Else, if it is already shrunk and try to get bigger, end the area.
				} else if (areaMap [x, y - 1] != currentAreaLabel && shrunkR) {
					while (areaMap[x,y] ==  currentAreaLabel) {
						// This while undo the last line.
						areaMap [x, y] = 0;
						x--;
					}
					break;
				}
				// Come back to the first square and go to the next line.
				x = xLeft;
				y = y + 1;
				if (y >= Height)
					break;
				// If the quare is not free, move to the right.
				while (!IsLabelFree(x,y) && areaMap[x,y-1] == currentAreaLabel) {
					x++;
				}
				// If you can move to the left, move to the left.
				while (IsLabelFree(x - 1, y) && !IsLabelFree(x - 1, y - 1)) {
					x--;
				}
				// If the line is smaller then the upper one, area is shrunk left.
				if (areaMap[x - 1, y - 1] == currentAreaLabel) {
					shrunkL = true;
					// Else, if it is already shrunk and try to get bigger, end the area.
				} else if (areaMap[x, y - 1] != currentAreaLabel && shrunkL) {
					break;
				}
				// If you cannot find a valid free spot on this line, end the area.
				if (!IsLabelFree (x, y))
					break;
				xLeft = x;
			}
			// Go to the next area.
			currentAreaLabel++;
			topMostFreeIdx = GetFirstFreeUnlabeled ();
		}
	}

	/// <summary>
	/// Compute the connectivity graph of the map
	/// </summary>
	public void ConnectivityGraph ()
	{
		int currentArea;
		int leftArea;
		int topArea;
		MapSquare currentSquare;
		MapSquare leftSquare;
		MapSquare topSquare;
		reversePortalDict = new Dictionary<MapSquare, List<PortalGroup>>();
		//var connectivityGraph = new Dictionary<int,HashSet<int>> ();
		areaConnectivity = new UndirectedGraph<int>();
		portalConnectivity = new UndirectedLabeledGraph<PortalGroup,bool,double>();
		portalSquares = new Dictionary<MapSquare, bool>();
		// Column Scan
		bool portalStrike = false;
		var pg = new PortalGroup();
		int area1 = 0;
		int area2 = 0;
		for (int x=0; x<Width; x++) {
			for (int y=0; y<Height; y++) {
				// Check left for other areas.
				// UNDONE: If I move right and I don't change area I can avoid left checking.
				currentArea = GetArea (x, y);
				leftArea = GetArea (x - 1, y);
				currentSquare = new MapSquare(x,y);
				leftSquare = new MapSquare(x-1,y);
				if (IsFree(x,y) && leftArea != currentArea && leftArea != 0) {
					if (area1 == 0 || (currentArea == area1 && leftArea == area2)) {
						if (area1==0) {
							area1 = currentArea;
							area2 = leftArea;
						}
						pg.Add(new Portal (
							currentSquare,
							leftSquare,
							currentArea,
							leftArea));
						if (!reversePortalDict.ContainsKey(currentSquare)) {
							reversePortalDict[currentSquare] = new List<PortalGroup>();
						}
						reversePortalDict[currentSquare].Add(pg);
						if (!reversePortalDict.ContainsKey(leftSquare)) {
							reversePortalDict[leftSquare] = new List<PortalGroup>();
						}
						reversePortalDict[leftSquare].Add(pg);
						// TODO: Write a generic function for this operation.
						areaConnectivity.AddVertex(currentArea);
						areaConnectivity.AddVertex(leftArea);
						areaConnectivity.AddEdge(currentArea,leftArea);
						if (!portalSquares.ContainsKey(currentSquare))
							portalSquares.Add(currentSquare,true);
						if (!portalSquares.ContainsKey(leftSquare))
							portalSquares.Add(leftSquare,true);
						portalStrike = true;
					} else {
						if (portalStrike) {
							area1=0;
							area2=0;
							portalStrike = false;
							portalConnectivity.AddVertex(pg);
							pg = new PortalGroup();
						}
					}
				} else {
					if (portalStrike) {
						area1=0;
						area2=0;
						portalStrike = false;
						portalConnectivity.AddVertex(pg);
						pg = new PortalGroup();
					}
				}
			}
		}
		// Row Scan
		portalStrike = false;
		pg = new PortalGroup();
		area1=0;
		area2=0;
		for (int y=0; y<Height; y++) {
			for (int x=0; x<Width; x++) {
				currentArea = GetArea (x, y);
				topArea = GetArea (x, y - 1);
				currentSquare = new MapSquare(x,y);
				topSquare = new MapSquare(x,y-1);
				if (IsFree(x,y) && topArea != currentArea && topArea != 0) {
					if (area1 == 0 || (currentArea == area1 && topArea == area2)) {
						if (area1==0) {
							area1 = currentArea;
							area2 = topArea;
						}
						pg.Add (new Portal (
							currentSquare,
							topSquare,
							currentArea,
							topArea));
						if (!reversePortalDict.ContainsKey(currentSquare)) {
							reversePortalDict[currentSquare] = new List<PortalGroup>();
						}
						reversePortalDict[currentSquare].Add(pg);
						if (!reversePortalDict.ContainsKey(topSquare)) {
							reversePortalDict[topSquare] = new List<PortalGroup>();
						}
						reversePortalDict[topSquare].Add(pg);
						areaConnectivity.AddVertex(currentArea);
						areaConnectivity.AddVertex(topArea);
						areaConnectivity.AddEdge(currentArea,topArea);
						if (!portalSquares.ContainsKey(currentSquare))
							portalSquares.Add(currentSquare,true);
						if (!portalSquares.ContainsKey(topSquare))
							portalSquares.Add(topSquare,true);
						portalStrike = true;
					} else {
						if (portalStrike) {
							area1=0;
							area2=0;
							portalStrike = false;
							portalConnectivity.AddVertex(pg);
							pg = new PortalGroup();
						}
					}
				} else {
					if (portalStrike) {
						portalStrike = false;
						portalConnectivity.AddVertex(pg);
						pg = new PortalGroup();
					}
				}
			}
		}
		pg = null;

		foreach (PortalGroup pg1 in portalConnectivity.Vertices) {
			foreach (PortalGroup pg2 in portalConnectivity.Vertices) {
				if (pg1.IsLinkedWith(pg2)) {
					portalConnectivity.AddEdge(pg1,pg2);
					portalConnectivity.SetEdgeLabel(pg1,pg2,PortalGroup.Distance(pg1,pg2));
				}
			}
		}
	}

	/// <summary>
	/// Returns the first unlabeled free top-leftmost square.
	/// </summary>
	/// <returns>The coordinates of the first free unlabeled top-leftmost square.</returns>
	Tuple<int,int> GetFirstFreeUnlabeled ()
	{
		for (int y = 0; y < Height; y++) {
			for (int x=0; x < Width; x++) {
				if (ElementIs("free", rawMap[x,y]) && areaMap[x,y] == 0) {
					return new Tuple<int,int>(x,y);
				}
			}
		}
		return null;
	}

	/// <summary>
	/// Determines whether the <x,y> label is free and unlabeled.
	/// </summary>
	/// <returns><c>true</c> if the <x,y> label is free and unlabeled; otherwise, <c>false</c>.</returns>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	bool IsLabelFree (int x, int y)
	{
		if (x >= Width || x < 0 || y >= Height || y < 0)
			return false;
		return ElementIs ("free", rawMap [x, y]) &&
			areaMap [x, y] == 0;
	}

	/// <summary>
	/// Determines whether the <x,y> square is free.
	/// </summary>
	/// <returns><c>true</c> if the <x,y> square is free; otherwise, <c>false</c>.</returns>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	public bool IsFree (int x, int y)
	{
		return IsFree(new MapSquare(x,y));
	}

	public bool IsFree (MapSquare ms)
	{
		if (ms.x >= Width || ms.x < 0 || ms.y >= Height || ms.y < 0)
			return false;
		if (portalSquares.ContainsKey(ms)) {
			return ElementIs ("free", rawMap [ms.x, ms.y]) && portalSquares[ms];
		}
		return ElementIs ("free", rawMap [ms.x, ms.y]);

	}

	public bool CanBeFree (MapSquare ms) {
		if (ms.x >= Width || ms.x < 0 || ms.y >= Height || ms.y < 0)
			return false;
		return ElementIs ("free", rawMap [ms.x, ms.y]);
	}

	public void SetPortalGroup(PortalGroup pg, bool state, int area) {
		foreach(Portal p in pg.Portals) {
			MapSquare pms = p[area];
			if (pms != null)
				portalSquares[pms] = state;
		}
	}

	public bool GetPortalGroupState(PortalGroup pg, int area) {
		foreach(Portal p in pg.Portals) {
			return portalSquares[p[area]];
		}
		return false;
	}

	/// <summary>
	/// Gets the area of the <x,y> square.
	/// </summary>
	/// <returns>The area label of the <x,y> square.</returns>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	int GetArea (int x, int y)
	{
		if (x >= Width || x < 0 || y >= Height || y < 0)
			return 0;
		return areaMap [x, y];
	}

	/// <summary>
	/// Checks if the given element belong to type.
	/// </summary>
	/// <returns><c>true</c>, if element is of type, <c>false</c> otherwise.</returns>
	/// <param name="type">The elements type.</param>
	/// <param name="element">The tested element.</param>
	public bool ElementIs (string type, char element)
	{
		string elementsList = itemsCatalogue [type];
		return elementsList.IndexOf (element) != -1;
	}
	
	/// <summary>
	/// Gets the portal group in PortalConnectivity in a given area.
	/// </summary>
	/// <returns>The portal group by areas.</returns>
	/// <param name="area">Area.</param>
	public List<PortalGroup> GetPortalGroupByAreas(int area) {
		var result = new List<PortalGroup>();
		foreach (PortalGroup pg in portalConnectivity.Vertices) {
			if (pg.BelongTo(area)) result.Add(pg);
		}
		return result;
	}

	/// <summary>
	/// Gets the portal groups in PortalConnectivity that link the given areas.
	/// </summary>
	/// <returns>The portal group by areas.</returns>
	/// <param name="area1">Area1.</param>
	/// <param name="area2">Area2.</param>
	public List<PortalGroup> GetPortalGroupByAreas(int area1, int area2) {
		var result = new List<PortalGroup>();
		foreach (PortalGroup pg in portalConnectivity.Vertices) {
			if (pg.Connect(area1,area2)) result.Add(pg);
		}
		return result;
	}

	/// <summary>
	/// Gets the portal group by square.
	/// </summary>
	/// <returns>The portal group by square.</returns>
	/// <param name="ms">Ms.</param>
	public List<PortalGroup> GetPortalGroupBySquare(MapSquare ms) {
		if (reversePortalDict.ContainsKey(ms)) {
			return reversePortalDict[ms];
		}
		return null;
//		var result = new List<PortalGroup>();
//		foreach (PortalGroup pg in portalConnectivity.Vertices) {
//			if (pg.Contains(ms)) result.Add(pg);
//		}
//		return result;
	}

	public void PrintStateAround(MapSquare ms,int range) {
		string result = "  ";
		// Build Header
		for (int x=ms.x - range;x<ms.x+range;x++) {
			result += " " + x;
		}
		result += "\n";
		for (int y=ms.y - range;y<ms.y+range;y++) {
			result += y + " ";
			for (int x=ms.x - range;x<ms.x+range;x++) {
				if (IsFree(x,y)) {
					result += " " + Areas[x,y];
				} else {
					result += " @";
				}
			}
			result += "\n";
		}
		Debug.Log(result);
	}

#region SertializationMethods
	public void SerializeData() {
		var data = new Map2DSerialization ();
		data.RealMap = rawMap;
		data.AreaMap = areaMap;
		data.PortalConnectivity = portalConnectivity;
		data.AreaConnectivity = areaConnectivity;
		
		Stream stream = File.Open("MapCache.cache", FileMode.Create);
		var bformatter = new BinaryFormatter();
		bformatter.Binder = new VersionDeserializationBinder(); 
		Debug.Log ("Writing Information on MapCache.cache");
		bformatter.Serialize(stream, data);
		stream.Close();
	}

	public bool LoadData() {
		Map2DSerialization data;
		Stream stream = File.Open("MapCache.cache", FileMode.Open);
		if (stream == null) return false;
		var bformatter = new BinaryFormatter();
		bformatter.Binder = new VersionDeserializationBinder(); 
		Debug.Log ("Reading Data from MapCache.cache");
		data = (Map2DSerialization)bformatter.Deserialize(stream);
		rawMap = data.RealMap;
		areaMap = data.AreaMap;
		portalConnectivity = data.PortalConnectivity;
		areaConnectivity = data.AreaConnectivity;
		stream.Close();
		Debug.Log ("Reading Complete");
		//DebugMessages();
		return true;
	}
#endregion

//	/// <summary>
//	/// Write Debugs the messages on the Unity console.
//	/// </summary>
//	public void DebugMessages() {
//		//* TMP *//
//		// TODO: Remove debug code.
//		int countPG = 0;
//		foreach (PortalGroup pgg in portalConnectivity.Vertices) {
//			pgg.Equals(pgg);
//			countPG++;
//		}
//		Debug.Log("PG = " + countPG);
//		Debug.Log("--------------");
//		foreach (int area in areaConnectivity.Vertices) {
//			string output = "AREA: " + area + " { ";
//			foreach (int otherArea in areaConnectivity.GetNeighbours(area)) {
//				output += otherArea + " ";
//			}
//			output += " }";
//			Debug.Log (output);
//		}
//		Debug.Log("--------------");
//		foreach (PortalGroup p in portalConnectivity.Vertices) {
//			string output = "PG: " + p + " { ";
//			foreach (PortalGroup otherp in portalConnectivity.GetNeighbours(p)) {
//				output += otherp + " " + portalConnectivity.GetEdgeLabel(p,otherp) + " ";
//			}
//			output += " }";
//			Debug.Log (output);
//		}
//		Debug.Log("--------------");
//	}


	[Serializable]
	class Map2DSerialization : ISerializable {

		public Grid<char> RealMap;
		public Grid<int> AreaMap;
		public UndirectedGraph<int> AreaConnectivity;
		public UndirectedLabeledGraph<PortalGroup,bool,double> PortalConnectivity;

		public Map2DSerialization() {}

		public Map2DSerialization (SerializationInfo info, StreamingContext context) {
			RealMap = (Grid<char>) info.GetValue("RealMap",typeof(Grid<char>));
			AreaMap = (Grid<int>) info.GetValue("AreaMap",typeof(Grid<int>));
			AreaConnectivity = (UndirectedGraph<int>) info.GetValue("AreaGraph",typeof(UndirectedGraph<int>));
			PortalConnectivity = (UndirectedLabeledGraph<PortalGroup,bool,double>) 
				info.GetValue("PortalGraph",typeof(UndirectedLabeledGraph<PortalGroup,bool,double>));
		}

		public void GetObjectData (SerializationInfo info, StreamingContext context) {
			info.AddValue("RealMap",RealMap);
			info.AddValue("AreaMap",AreaMap);
			info.AddValue("AreaGraph",AreaConnectivity);
			info.AddValue("PortalGraph",PortalConnectivity);
		}
	}

}

// This is a SerializationBinder. It is needed by Unity to match the serialized assembly 
// with the current assembly. It is a standard class, no modification should be ever needed.
public sealed class VersionDeserializationBinder : SerializationBinder
{
	public override Type BindToType (string assemblyName, string typeName)
	{
		if (!string.IsNullOrEmpty (assemblyName) && !string.IsNullOrEmpty (typeName)) {			
			assemblyName = Assembly.GetExecutingAssembly ().FullName;
			// The following line of code returns the type.
			Type typeToDeserialize = Type.GetType (String.Format ("{0}, {1}", typeName, assemblyName));
			return typeToDeserialize;
		}
		return null;
	}
}