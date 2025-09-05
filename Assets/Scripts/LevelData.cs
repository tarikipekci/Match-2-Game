using UnityEngine;

[System.Serializable]
public class TileRow
{
    public TileData[] tiles;
}

[System.Serializable]
public class TileData
{
    public TileType tileType;
    public TileColor tileColor;
}

[System.Serializable]
public class CubeGoal
{
    public TileColor color;
    public int targetCount;
}

[CreateAssetMenu(fileName = "LevelData", menuName = "Match2/LevelData")]
public class LevelData : ScriptableObject
{
    public int moveCount;
    public Vector2Int gridSize;
    public CubeGoal[] cubeGoals;
    public int targetBalloonCount;
    public int targetDuckCount;

    [Header("Initial State")] 
    public TileRow[] startingRows; 
}