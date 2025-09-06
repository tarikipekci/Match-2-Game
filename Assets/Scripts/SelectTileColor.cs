using System.Collections.Generic;
using UnityEngine;

public class SelectTileColor : MonoBehaviour
{
    private static readonly Dictionary<TileColor, Color> TileColorMap = new()
    {
        { TileColor.Red, Color.red },
        { TileColor.Blue, Color.blue },
        { TileColor.Green, Color.green },
        { TileColor.Yellow, Color.yellow },
        { TileColor.Purple, new Color(0.5f, 0f, 0.5f) } 
    };

    public static Color GetColor(TileColor tileColor)
    {
        if (TileColorMap.TryGetValue(tileColor, out var color))
            return color;
        return Color.white; // default
    }
}