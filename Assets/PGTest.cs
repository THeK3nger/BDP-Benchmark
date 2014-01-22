using UnityEngine;
using System.Collections;

public class PGTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
		TestVertical();
		TestHorizontal();
	}

	void TestVertical() {
		PortalGroup pg = new PortalGroup();
		pg.Add(new Portal(new MapSquare(5,5), new MapSquare(6,5),1,2) );
		pg.Add(new Portal(new MapSquare(5,6), new MapSquare(6,6),1,2) );
		pg.Add(new Portal(new MapSquare(5,7), new MapSquare(6,7),1,2) );
		pg.Add(new Portal(new MapSquare(5,8), new MapSquare(6,8),1,2) );
		Debug.Log(pg.first);
		Debug.Log(pg.last);
		Debug.Log(pg.Horizontal);
		for (int i=0;i<10;i++) {
			for (int j=0;j<10;j++) {
				if (pg.Contains(new MapSquare(i,j)) != pg.ContainsOld(new MapSquare(i,j))) {
					Debug.Log("BOOM! " + i + " " +j);
					return;
				}
			}
		}
		Debug.Log("Test Pass");
	}

	void TestHorizontal() {
		PortalGroup pg = new PortalGroup();
		pg.Add(new Portal(new MapSquare(5,5), new MapSquare(5,6),1,2) );
		pg.Add(new Portal(new MapSquare(6,5), new MapSquare(6,6),1,2) );
		pg.Add(new Portal(new MapSquare(7,5), new MapSquare(7,6),1,2) );
		pg.Add(new Portal(new MapSquare(8,5), new MapSquare(8,6),1,2) );
		Debug.Log(pg.first);
		Debug.Log(pg.last);
		Debug.Log(pg.Horizontal);
		for (int i=0;i<10;i++) {
			for (int j=0;j<10;j++) {
				if (pg.Contains(new MapSquare(i,j)) != pg.ContainsOld(new MapSquare(i,j))) {
					Debug.Log("BOOM! " + i + " " +j);
					return;
				}
			}
		}
		Debug.Log("Test Pass");
	}

	// Update is called once per frame
	void Update () {
	
	}
}
