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

[System.Serializable]
public class CubePool
{
    public TileColor[] tileColors;
}

[System.Serializable]
public class PassivePool
{
    public TileType[] tileType;
}

[System.Serializable]
public class PoolData
{
    [Header("For Cubes")] public CubePool cubePool;
    [Header("For Passives")] public PassivePool passivePool;
}

[CreateAssetMenu(fileName = "LevelData", menuName = "Match2/LevelData")]
public class LevelData : ScriptableObject
{
    public int moveCount;
    public Vector2Int gridSize;
    public CubeGoal[] cubeGoals;
    public int targetBalloonCount;
    public int targetDuckCount;

    [Header("Initial State")] public TileRow[] startingRows;

    [Header("Spawn Pool")] public PoolData spawnPool;
}