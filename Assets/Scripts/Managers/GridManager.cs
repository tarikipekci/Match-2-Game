using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using static Tile;

namespace Managers
{
    public class GridManager : MonoBehaviour
    {
        private LevelData levelData;
        public GameObject cubeTilePrefab;
        public GameObject rocketTilePrefab;
        public GameObject passiveTilePrefab;
        public float tileSize = 1f;
        public MoveManager moveManager;
        public GoalManager goalManager;
        private int activeTilesCount;

        private Tile[,] grid;
        private bool[,] visited;

        void Start()
        {
            levelData = GameManager.Instance.currentLevelData;
            GenerateGrid();
        }

        private void CalculateTileSize()
        {
            int rows = levelData.gridSize.y;
            int columns = levelData.gridSize.x;
            const float boardSizeRatio = 4f;
            tileSize = Mathf.Min(boardSizeRatio / columns, boardSizeRatio / rows);
        }

        private void GenerateGrid()
        {
            CalculateTileSize();

            int rows = levelData.gridSize.y;
            int columns = levelData.gridSize.x;
            grid = new Tile[rows, columns];

            float startX = -columns * tileSize / 2 + tileSize / 2;
            float startY = rows * tileSize / 2 - tileSize / 2;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    Tile tile;

                    if (levelData.startingRows != null && r < levelData.startingRows.Length)
                    {
                        var rowData = levelData.startingRows[r];
                        if (rowData.tiles != null && c < rowData.tiles.Length)
                        {
                            tile = SpawnTileFromData(rowData.tiles[c], r, c, startX, startY);
                        }
                        else
                        {
                            tile = SpawnRandomTile(r, c, startX, startY);
                        }
                    }
                    else
                    {
                        tile = SpawnRandomTile(r, c, startX, startY);
                    }

                    grid[r, c] = tile;
                }
            }
        }

        private Tile SpawnTileFromData(TileData data, int r, int c, float startX, float startY)
        {
            GameObject tileObj = Instantiate(cubeTilePrefab, transform);
            tileObj.transform.localScale = Vector3.one * tileSize;

            Tile tile;
            switch (data.tileType)
            {
                case TileType.Cube:
                    CubeTile cube = tileObj.GetComponent<CubeTile>();
                    cube.tileType = TileType.Cube;
                    cube.tileColor = data.tileColor;
                    cube.UpdateSprite();
                    tile = cube;
                    break;
                case TileType.Balloon:
                case TileType.Duck:
                    tile = tileObj.GetComponent<Tile>();
                    tile.tileType = data.tileType;
                    tile.UpdateSprite();
                    break;
                case TileType.None:
                case TileType.Rocket:
                default:
                    tile = tileObj.GetComponent<Tile>();
                    tile.tileType = TileType.None;
                    break;
            }

            tile.row = r;
            tile.column = c;

            float startTileY = startY - r * tileSize + tileSize * levelData.gridSize.y;
            tileObj.transform.localPosition = new Vector3(startX + c * tileSize, startTileY, 0);
            Vector3 endPos = new Vector3(startX + c * tileSize, startY - r * tileSize, 0);
            PlayTileDropAnimation(tileObj.transform, endPos);

            return tile;
        }

        private Tile SpawnRandomTile(int r, int c, float startX, float startY)
        {
            Tile tile;
            float random = Random.value;

            List<TileType> passiveTypes = new List<TileType>(levelData.spawnPool.passivePool.tileType);
            float balloonChance = passiveTypes.Contains(TileType.Balloon) ? 0.2f : 0f;
            float duckChance = passiveTypes.Contains(TileType.Duck) ? 0.15f : 0f;

            if (random < balloonChance && passiveTypes.Contains(TileType.Balloon))
            {
                GameObject obj = Instantiate(passiveTilePrefab, transform);
                obj.transform.localScale = Vector3.one * tileSize;
                tile = obj.GetComponent<Tile>();
                tile.tileType = TileType.Balloon;
            }
            else if (random < balloonChance + duckChance && passiveTypes.Contains(TileType.Duck))
            {
                GameObject obj = Instantiate(passiveTilePrefab, transform);
                obj.transform.localScale = Vector3.one * tileSize;
                tile = obj.GetComponent<Tile>();
                tile.tileType = TileType.Duck;
            }
            else
            {
                GameObject obj = Instantiate(cubeTilePrefab, transform);
                obj.transform.localScale = Vector3.one * tileSize;
                CubeTile cube = obj.GetComponent<CubeTile>();
                cube.tileType = TileType.Cube;

                TileColor[] cubeColors = levelData.spawnPool.cubePool.tileColors;
                if (cubeColors.Length > 0)
                {
                    int randIndex = Random.Range(0, cubeColors.Length);
                    cube.tileColor = cubeColors[randIndex];
                }
                else cube.tileColor = TileColor.Red;

                cube.UpdateSprite();
                tile = cube;
            }

            tile.row = r;
            tile.column = c;

            float startTileY = startY - r * tileSize + tileSize * levelData.gridSize.y;
            tile.transform.localPosition = new Vector3(startX + c * tileSize, startTileY, 0);
            Vector3 endPos = new Vector3(startX + c * tileSize, startY - r * tileSize, 0);
            PlayTileDropAnimation(tile.transform, endPos);

            tile.UpdateSprite();
            return tile;
        }

        public void TryMatch(Tile startTile)
        {
            if (startTile is not CubeTile cubeTile) return;

            Vector3 startPos = startTile.transform.localPosition;
            visited = new bool[levelData.gridSize.y, levelData.gridSize.x];
            List<Tile> connected = new List<Tile>();
            bool stop = false;

            DFS(cubeTile.row, cubeTile.column, startTile, connected, ref stop, out var cubeCount);

            if (cubeCount >= 2)
            {
                if (!moveManager.UseMove())
                {
                    Debug.Log("Game Over!");
                    return;
                }

                foreach (var t in connected)
                {
                    grid[t.row, t.column] = null;
                }

                if (cubeCount >= 5)
                {
                    SpawnRocket(cubeTile.row, cubeTile.column, startPos);
                }

                OnTilesMatched?.Invoke(connected);
                CollapseColumns();
                RefillGrid();
            }
        }

        private void DFS(int r, int c, Tile startTile, List<Tile> connected, ref bool stop, out int cubeCount)
        {
            cubeCount = 0;

            if (r < 0 || r >= levelData.gridSize.y || c < 0 || c >= levelData.gridSize.x) return;
            if (visited[r, c]) return;

            Tile tile = grid[r, c];
            if (tile == null || tile.isItObstacle) return;

            visited[r, c] = true;

            if (tile.behavior.CanMatch(tile, startTile, connected, this))
            {
                connected.Add(tile);
                if (tile is CubeTile) cubeCount = 1;

                if (tile.StopFurtherSearch)
                {
                    stop = true;
                    return;
                }
            }
            else
            {
                stop = true;
                return;
            }

            bool stopUp = false, stopDown = false, stopLeft = false, stopRight = false;

            DFS(r + 1, c, startTile, connected, ref stopUp, out var tempCube);
            cubeCount += tempCube;

            DFS(r - 1, c, startTile, connected, ref stopDown, out tempCube);
            cubeCount += tempCube;

            DFS(r, c + 1, startTile, connected, ref stopRight, out tempCube);
            cubeCount += tempCube;

            DFS(r, c - 1, startTile, connected, ref stopLeft, out tempCube);
            cubeCount += tempCube;
        }

        private void CollapseColumns()
        {
            int rows = levelData.gridSize.y;
            int columns = levelData.gridSize.x;
            float startX = -columns * tileSize / 2 + tileSize / 2;
            float startY = rows * tileSize / 2 - tileSize / 2;

            List<Tile> ducksToRemove = new List<Tile>();
            int CurrentTweens = 0;

            for (int c = 0; c < columns; c++)
            {
                int emptyRow = rows - 1;
                for (int r = rows - 1; r >= 0; r--)
                {
                    Tile tile = grid[r, c];
                    if (tile != null)
                    {
                        if (r != emptyRow)
                        {
                            grid[emptyRow, c] = tile;
                            grid[r, c] = null;

                            tile.row = emptyRow;
                            tile.column = c;

                            Vector3 endPos = new Vector3(startX + c * tileSize, startY - emptyRow * tileSize, 0);
                            CurrentTweens++;

                            PlayTileDropAnimation(tile.transform, endPos).OnComplete(() =>
                            {
                                CurrentTweens--;
                                if (tile.tileType == TileType.Duck && tile.row == rows - 1)
                                    ducksToRemove.Add(tile);

                                if (CurrentTweens == 0)
                                {
                                    foreach (var duck in ducksToRemove)
                                    {
                                        goalManager.CollectTile(duck);
                                        grid[duck.row, duck.column] = null;
                                    }

                                    CollapseColumns();
                                    RefillGrid();
                                }
                            });
                        }

                        emptyRow--;
                    }
                }
            }

            if (CurrentTweens == 0)
            {
                RefillGrid();
            }
        }

        private void RefillGrid()
        {
            int rows = levelData.gridSize.y;
            int columns = levelData.gridSize.x;
            float startX = -columns * tileSize / 2 + tileSize / 2;
            float startY = rows * tileSize / 2 - tileSize / 2;

            for (int c = 0; c < columns; c++)
            {
                for (int r = 0; r < rows; r++)
                {
                    if (grid[r, c] == null)
                    {
                        grid[r, c] = SpawnRandomTile(r, c, startX, startY);
                    }
                }
            }
        }

        public Tile GetTile(int r, int c) => grid[r, c];
        public Tile[,] GetGrid() => grid;

        private void SpawnRocket(int row, int column, Vector3 spawnPos)
        {
            GameObject rocketObj = Instantiate(rocketTilePrefab, transform);
            rocketObj.transform.localScale = Vector3.one * tileSize;

            RocketTile rocket = rocketObj.GetComponent<RocketTile>();
            rocket.Initialize();
            rocketObj.transform.localPosition = spawnPos;

            grid[row, column] = rocket;
            rocket.row = row;
            rocket.column = column;
        }

        public void TileActivated() => activeTilesCount++;

        public void TileFinished()
        {
            activeTilesCount--;
            if (activeTilesCount <= 0)
            {
                CollapseColumns();
                RefillGrid();
            }
        }

        private Sequence PlayTileDropAnimation(Transform tileTransform, Vector3 endPos)
        {
            const float bounceHeight = 0.15f;
            const float moveDownTime = 0.3f;
            const float bounceUpTime = 0.06f;
            const float settleTime = 0.15f;
            const float scaleAmount = 0.95f;

            Sequence seq = DOTween.Sequence();

            Vector3 originalScale = tileTransform.localScale;

            // First fall
            seq.Append(tileTransform.DOLocalMove(endPos, moveDownTime).SetEase(Ease.OutCubic));

            // little bounce
            seq.Append(tileTransform.DOLocalMove(endPos + Vector3.up * bounceHeight, bounceUpTime).SetEase(Ease.OutQuad));

            // last fall
            seq.Append(tileTransform.DOLocalMove(endPos, settleTime).SetEase(Ease.InQuad));

            // Scale updating relative to original scale
            seq.Join(tileTransform.DOScale(originalScale * scaleAmount, settleTime * 0.5f)
                .SetLoops(2, LoopType.Yoyo));

            return seq;
        }

        public bool IsNeighbor(Tile cube, Tile other)
        {
            int dr = Mathf.Abs(cube.row - other.row);
            int dc = Mathf.Abs(cube.column - other.column);
            return (dr == 1 && dc == 0) || (dr == 0 && dc == 1);
        }
    }
}