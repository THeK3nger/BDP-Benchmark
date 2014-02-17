#define MAPPARSER_DEBUG_LOG

using UnityEngine;
using System.Collections;
using RoomOfRequirement.Generic;

namespace BDP.Map {

    /// <summary>
    /// Implement a static map parser for map in the MovingAI database.
    /// </summary>
    public static class MapParser {

        public static Map2D ParseMapFromString(string mapstring) {
            string[] lines = mapstring.Split('\n');
            // Parse map size.
            int lidx = 0; // Parsing line index
            string current = "";
            int height = 0;
            int width = 0;
            while (current != "map") {
                current = lines[lidx];
                // Map Parameters, Ignore "type".
                string[] par = current.Split(' ');
                switch (par[0]) {
                    case "height":
                        height = int.Parse(par[1]);
                        break;
                    case "width":
                        width = int.Parse(par[1]);
                        break;
                    default:
                        break;
                }
                lidx++;
            }
            if (height == 0 || width == 0) {
#if MAPPARSER_DEBUG_LOG
                Debug.LogError("[MAP-PARSER]: Invalid Map Format!");
#endif
                return null;
            }
            // Initialize map array
            var rawMap = new Grid<char>(height, width);
            // Fill the map 
            for (int line = lidx; line < lines.Length; line++) {
                string map_items = lines[line];
                for (int j = 0; j < map_items.Length; j++) {
                    rawMap[j, line - lidx] = map_items[j];
                }
            }
            return new Map2D(rawMap);
        }

    }
}
