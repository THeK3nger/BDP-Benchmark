using UnityEngine;
using System.Collections;

public interface IMapHierarchicalBelief : IMapBelief {

    //Path<MapSquare> ExpandPath(Path<MapSquare> path);
    void OpenOldPortals(float timeLimit);
}
