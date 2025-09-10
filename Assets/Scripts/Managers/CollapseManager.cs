using System.Collections.Generic;
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

        public CollapseManager(Tile[,] grid, Vector2Int gridSize, GridManager gridManager, GoalManager goalManager, float tileSize)
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
            int rows = gridSize.y;
            int columns = gridSize.x;
            float startX = -columns * tileSize / 2 + tileSize / 2;
            float startY = rows * tileSize / 2 - tileSize / 2;

            List<Tile> ducksToRemove = new List<Tile>();
            int activeTweens = 0;

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
                            activeTweens++;

                            gridManager.PlayTileDropAnimation(tile.transform, endPos).OnComplete(() =>
                            {
                                activeTweens--;

                                if (tile.tileType == TileType.Duck && tile.row == rows - 1)
                                    ducksToRemove.Add(tile);

                                if (activeTweens == 0)
                                {
                                    foreach (var duck in ducksToRemove)
                                    {
                                        goalManager.CollectTile(duck);
                                        grid[duck.row, duck.column] = null;
                                    }

                                    if (CheckForEmptySpaces())
                                        CollapseAndRefill();
                                    else
                                        RefillGrid();
                                }
                            });
                        }
                        emptyRow--;
                    }
                }
            }

            if (activeTweens == 0)
                RefillGrid();
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

            for (int c = 0; c < columns; c++)
            {
                for (int r = 0; r < rows; r++)
                {
                    if (grid[r, c] == null)
                    {
                        Tile newTile = gridManager.gridGenerator.SpawnRandomTile(r, c, startX, startY);
                        grid[r, c] = newTile;

                        Vector3 spawnPos = new Vector3(startX + c * tileSize, startY + tileSize * gridSize.y, 0);
                        newTile.transform.localPosition = spawnPos;

                        Vector3 endPos = new Vector3(startX + c * tileSize, startY - r * tileSize, 0);
                        gridManager.PlayTileDropAnimation(newTile.transform, endPos);
                    }
                }
            }
        }
    }
}
