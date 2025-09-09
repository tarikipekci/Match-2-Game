using System.Collections.Generic;
using UnityEngine;

namespace Helper
{
    public class SelectTileColor : MonoBehaviour
    {
        private static readonly Dictionary<TileColor, Color> TileColorMap = new()
        {
            { TileColor.Red, Color.red },
            { TileColor.Blue, Color.cyan },
            { TileColor.Green, Color.green },
            { TileColor.Yellow, Color.yellow },
            { TileColor.Purple, new Color(0.7f, 0.4f, 0.8f) } 
        };

        public static Color GetColor(TileColor tileColor)
        {
            if (TileColorMap.TryGetValue(tileColor, out var color))
                return color;
            return Color.white; // default
        }
    }
}