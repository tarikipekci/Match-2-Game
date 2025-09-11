using UnityEngine;

namespace Data
{
    [System.Serializable]
    public class TileRow
    {
        public TileData[] tiles; // A row of tiles, defines the initial state for this row
    }

    [System.Serializable]
    public class TileData
    {
        public TileType tileType; // Type of the tile (Cube, Balloon, Duck, etc.)
        public TileColor tileColor; // Only relevant for Cube tiles
    }

    [System.Serializable]
    public class CubeGoal
    {
        public TileColor color; // Target color to collect
        public int targetCount; // Number of tiles to collect for this goal
    }

    [System.Serializable]
    public class CubePool
    {
        public TileColor[] tileColors; // Colors available for random Cube spawning
    }

    [System.Serializable]
    public class PassivePool
    {
        public TileType[] tileType; // Types of passive tiles for random spawning (Balloon, Duck)
    }

    [System.Serializable]
    public class PoolData
    {
        [Header("For Cubes")] public CubePool cubePool;
        [Header("For Passives")] public PassivePool passivePool;
        // Contains data for spawning random tiles during gameplay
    }

    [CreateAssetMenu(fileName = "LevelData", menuName = "Match2/LevelData")]
    public class LevelData : ScriptableObject
    {
        public int moveCount; // Total number of moves allowed for this level
        public Vector2Int gridSize; // Board size (rows x columns)
        public CubeGoal[] cubeGoals; // Goals for cube tiles
        public int ballonGoalCount; // Goal count for balloons
        public int duckGoalCount; // Goal count for ducks

        [Header("Initial State")] public TileRow[] startingRows; 
        // Defines the starting layout of tiles on the board

        [Header("Spawn Pool")] public PoolData spawnPool; 
        // Pools for randomly spawning tiles during the game
    }
}
