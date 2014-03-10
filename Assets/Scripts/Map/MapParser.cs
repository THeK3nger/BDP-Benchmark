// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MapParser.cs" company="Davide Aversa">
//   MIT License
// </copyright>
// <summary>
//   Implement a static map parser for map in the MovingAI database.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

#define MAPPARSER_DEBUG_LOG

namespace BDP.Map
{
    using RoomOfRequirement.Generic;

    using UnityEngine;

    /// <summary>
    /// Implement a static map parser for map in the MovingAI database.
    /// </summary>
    public static class MapParser
    {
        /// <summary>
        /// The parse map from string.
        /// </summary>
        /// <param name="mapstring">
        /// The map string.
        /// </param>
        /// <returns>
        /// The <see cref="Map2D"/>.
        /// </returns>
        public static Map2D ParseMapFromString(string mapstring)
        {
            var lines = mapstring.Split('\n');

            // Parse map size.
            var lidx = 0; // Parsing line index
            var current = string.Empty;
            var height = 0;
            var width = 0;
            while ("map" != current)
            {
                current = lines[lidx];

                // Map Parameters, Ignore "type".
                var par = current.Split(' ');
                switch (par[0])
                {
                    case "height":
                        height = int.Parse(par[1]);
                        break;
                    case "width":
                        width = int.Parse(par[1]);
                        break;
                }

                lidx++;
            }

            if (height == 0 || width == 0)
            {
#if MAPPARSER_DEBUG_LOG
                Debug.LogError("[MAP-PARSER]: Invalid Map Format!");
#endif
                return null;
            }

            // Initialize map array
            var rawMap = new Grid<char>(height, width);

            // Fill the map 
            for (var line = lidx; line < lines.Length; line++)
            {
                var mapItems = lines[line];
                for (var j = 0; j < mapItems.Length; j++)
                {
                    rawMap[j, line - lidx] = mapItems[j];
                }
            }

            return new Map2D(rawMap);
        }
    }
}
