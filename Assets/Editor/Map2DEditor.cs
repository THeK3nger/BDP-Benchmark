using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(Map2D))]
public class Map2DEditor : Editor {
	public override void OnInspectorGUI() {
		DrawDefaultInspector();
		if (GUILayout.Button("Precompute Map")) {
			Map2D map = (Map2D)target;
			map.LoadMapFromFile ();
			map.MapPartitioning();
			map.ConnectivityGraph();
			//map.DebugMessages();
			map.SerializeData();
		}
	}
}
