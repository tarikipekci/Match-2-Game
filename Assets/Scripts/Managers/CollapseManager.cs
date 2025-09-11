using System;
using DG.Tweening;
using UnityEngine;

namespace Managers
{
    public class CollapseManager
    {
        private Tile[,] grid;
        private Vector2Int gridSize;
        private GridManager gridManager;
        private GoalManager goalManager;
        private float tileSize;

        // Triggered when a tile lands after collapsing or refill
        public static Action<GridManager, Tile> OnTileLanded;

        public CollapseManager(Tile[,] grid, Vector2Int gridSize, GridManager gridManager, GoalManager goalManager,
            float tileSize)
        {
            this.grid = grid;
            this.gridSize = gridSize;
            this.gridManager = gridManager;
            this.goalManager = goalManager;
            this.tileSize = tileSize;
        }

        public void CollapseAndRefill()
        {
            CollapseColumns();
        }

        private void CollapseColumns()
        {
            InputManager.DisableInput(); // Prevent player input during collapse

            int rows = gridSize.y;
            int columns = gridSize.x;
            float startX = -columns * tileSize / 2 + tileSize / 2;
            float startY = rows * tileSize / 2 - tileSize / 2;

            int activeTweens = 0;

            for (int c = 0; c < columns; c++)
            {
                int emptyRow = rows - 1;
                for (int r = rows - 1; r >= 0; r--)
                {
                    Tile tile = grid[r, c];
                    if (tile != null && r != emptyRow)
                    {
                        // Move tile to the lowest empty row
                        grid[emptyRow, c] = tile;
                        grid[r, c] = null;
                        tile.row = emptyRow;
                        tile.column = c;

                        Vector3 endPos = new Vector3(startX + c * tileSize, startY - emptyRow * tileSize, 0);
                        activeTweens++;

                        // Animate tile falling
                        gridManager.PlayTileDropAnimation(tile.transform, endPos).OnComplete(() =>
                        {
                            activeTweens--;
                            tile.InitializeBehavior();
                            OnTileLanded?.Invoke(gridManager, tile);

                            // After all animations, check if refill is needed
                            if (activeTweens == 0)
                            {
                                if (CheckForEmptySpaces())
                                    CollapseAndRefill();
                                else
                                {
                                    RefillGrid();
                                    if (!CheckForEmptySpaces())
                                        GridManager.OnBoardReady?.Invoke(gridManager);
                                }
                            }
                        });
                    }
                    if (tile != null) emptyRow--;
                }
            }

            if (activeTweens == 0)
            {
                RefillGrid();
                if (!CheckForEmptySpaces())
                    GridManager.OnBoardReady?.Invoke(gridManager);
            }
        }

        private bool CheckForEmptySpaces()
        {
            for (int r = 0; r < gridSize.y; r++)
            for (int c = 0; c < gridSize.x; c++)
                if (grid[r, c] == null)
                    return true;
            return false;
        }

        private void RefillGrid()
        {
            int rows = gridSize.y;
            int columns = gridSize.x;
            float startX = -columns * tileSize / 2 + tileSize / 2;
            float startY = rows * tileSize / 2 - tileSize / 2;

            int activeTweens = 0;

            for (int c = 0; c < columns; c++)
            {
                for (int r = 0; r < rows; r++)
                {
                    if (grid[r, c] == null)
                    {
                        bool isBottom = (r == rows - 1); // Needed for spawn logic in GridGenerator
                        Tile newTile = gridManager.gridGenerator.SpawnRandomTile(r, c, startX, startY, isBottom);
                        grid[r, c] = newTile;

                        Vector3 spawnPos = new Vector3(startX + c * tileSize, startY + tileSize * gridSize.y, 0);
                        newTile.transform.localPosition = spawnPos;

                        Vector3 endPos = new Vector3(startX + c * tileSize, startY - r * tileSize, 0);
                        activeTweens++;

                        // Animate newly spawned tile falling
                        gridManager.PlayTileDropAnimation(newTile.transform, endPos).OnComplete(() =>
                        {
                            activeTweens--;
                            newTile.InitializeBehavior();
                            OnTileLanded?.Invoke(gridManager, newTile);

                            if (activeTweens <= 0)
                                GridManager.OnBoardReady?.Invoke(gridManager);
                        });
                    }
                }
            }

            if (activeTweens == 0)
                GridManager.OnBoardReady?.Invoke(gridManager);
        }
    }
}
