using System.Collections.Generic;
using Data;
using UnityEngine;

namespace Managers
{
    public class GridGenerator
    {
        private readonly LevelData levelData;
        private readonly GameObject cubeTilePrefab;
        private readonly GameObject passiveTilePrefab;
        private readonly float tileSize;
        private readonly Transform parent;

        public GridGenerator(LevelData levelData, GameObject cubeTilePrefab, GameObject passiveTilePrefab,
            float tileSize, Transform parent)
        {
            this.levelData = levelData;
            this.cubeTilePrefab = cubeTilePrefab;
            this.passiveTilePrefab = passiveTilePrefab;
            this.tileSize = tileSize;
            this.parent = parent;
        }

        public Tile[,] GenerateGrid()
        {
            int rows = levelData.gridSize.y;
            int columns = levelData.gridSize.x;
            Tile[,] grid = new Tile[rows, columns];

            float startX = -columns * tileSize / 2 + tileSize / 2;
            float startY = rows * tileSize / 2 - tileSize / 2;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    Tile tile;

                    // Check if there is predefined tile data for this row
                    if (levelData.startingRows != null && r < levelData.startingRows.Length)
                    {
                        var rowData = levelData.startingRows[r];
                        if (rowData.tiles != null && c < rowData.tiles.Length)
                            tile = SpawnTileFromData(rowData.tiles[c], r, c, startX, startY); // Spawn tile according to data
                        else
                            tile = SpawnRandomTile(r, c, startX, startY); // Otherwise spawn randomly
                    }
                    else
                    {
                        tile = SpawnRandomTile(r, c, startX, startY);
                    }

                    grid[r, c] = tile;
                }
            }

            return grid;
        }

        private Tile SpawnTileFromData(TileData data, int r, int c, float startX, float startY)
        {
            GameObject tileObj;
            Tile tile;

            switch (data.tileType)
            {
                case TileType.Cube:
                    // Use object pool if available
                    tileObj = PoolManager.Instance.SpawnFromPool(cubeTilePrefab.name, parent);
                    if (tileObj == null)
                        tileObj = Object.Instantiate(cubeTilePrefab, parent);

                    CubeTile cube = tileObj.GetComponent<CubeTile>();
                    cube.tileType = TileType.Cube;
                    cube.tileColor = data.tileColor;
                    cube.UpdateSprite();
                    tile = cube;
                    break;

                case TileType.Balloon:
                case TileType.Duck:
                    // Spawn passive tiles like Balloon or Duck
                    tileObj = PoolManager.Instance.SpawnFromPool(passiveTilePrefab.name, parent);
                    if (tileObj == null)
                        tileObj = Object.Instantiate(passiveTilePrefab, parent);

                    tile = tileObj.GetComponent<Tile>();
                    tile.tileType = data.tileType;
                    tile.UpdateSprite();
                    break;

                default:
                    tileObj = PoolManager.Instance.SpawnFromPool(passiveTilePrefab.name, parent);
                    if (tileObj == null)
                        tileObj = Object.Instantiate(passiveTilePrefab, parent);

                    tile = tileObj.GetComponent<Tile>();
                    tile.tileType = TileType.None;
                    break;
            }

            // Set tile position in grid
            tile.row = r;
            tile.column = c;
            tileObj.transform.localPosition = new Vector3(startX + c * tileSize, startY - r * tileSize, 0);
            tileObj.transform.localScale = Vector3.one * tileSize;

            tile.OnSpawn(); // Call tile's OnSpawn for initialization
            tile.ownerGrid = GameManager.Instance.currentGridManager;
            tile.InitializeBehavior(); // Initialize specific tile behavior
            tile.SetIsMatchable();

            return tile;
        }

        public Tile SpawnRandomTile(int r, int c, float startX, float startY, bool isBottom = false)
        {
            Tile tile;
            float random = Random.value;

            List<TileType> passiveTypes = new List<TileType>(levelData.spawnPool.passivePool.tileType);
            float balloonChance = passiveTypes.Contains(TileType.Balloon) ? 0.2f : 0f;
            float duckChance = passiveTypes.Contains(TileType.Duck) ? 0.15f : 0f;

            if (isBottom)
                duckChance = 0f; // Prevent ducks spawning at the bottom row

            GameObject obj;

            if (random < balloonChance && passiveTypes.Contains(TileType.Balloon))
            {
                obj = PoolManager.Instance.SpawnFromPool(passiveTilePrefab.name, parent);
                if (obj == null) obj = Object.Instantiate(passiveTilePrefab, parent);
                tile = obj.GetComponent<Tile>();
                tile.tileType = TileType.Balloon;
            }
            else if (random < balloonChance + duckChance && passiveTypes.Contains(TileType.Duck))
            {
                obj = PoolManager.Instance.SpawnFromPool(passiveTilePrefab.name, parent);
                if (obj == null) obj = Object.Instantiate(passiveTilePrefab, parent);
                tile = obj.GetComponent<Tile>();
                tile.tileType = TileType.Duck;
            }
            else
            {
                obj = PoolManager.Instance.SpawnFromPool(cubeTilePrefab.name, parent);
                if (obj == null) obj = Object.Instantiate(cubeTilePrefab, parent);

                CubeTile cube = obj.GetComponent<CubeTile>();
                cube.tileType = TileType.Cube;

                // Assign a random color from the cube pool
                TileColor[] cubeColors = levelData.spawnPool.cubePool.tileColors;
                cube.tileColor = cubeColors.Length > 0 ? cubeColors[Random.Range(0, cubeColors.Length)] : TileColor.Red;
                cube.UpdateSprite();
                tile = cube;
            }

            obj.transform.localScale = Vector3.one * tileSize;
            tile.row = r;
            tile.column = c;
            tile.transform.localPosition = new Vector3(startX + c * tileSize, startY - r * tileSize, 0);
            tile.UpdateSprite();
            tile.ownerGrid = GameManager.Instance.currentGridManager;
            tile.InitializeBehavior(); // Initialize behavior after spawning
            tile.SetIsMatchable();

            return tile;
        }
    }
}
