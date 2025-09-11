using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
    public class MatchFinder
    {
        private Tile[,] grid;
        private Vector2Int gridSize;
        private bool[,] visited;
        private GridManager gridManager;

        public MatchFinder(Tile[,] grid, Vector2Int gridSize, GridManager gridManager)
        {
            this.grid = grid;
            this.gridSize = gridSize;
            this.gridManager = gridManager;
        }

        public List<Tile> FindMatches(Tile startTile, out int cubeCount)
        {
            cubeCount = 0;
            if (startTile == null || startTile is not CubeTile tile) return null;

            visited = new bool[gridSize.y, gridSize.x];
            List<Tile> connected = new List<Tile>();
            bool stop = false;

            DFS(tile.row, tile.column, tile, connected, ref stop, ref cubeCount); // Start recursive depth-first search

            return connected;
        }

        private void DFS(int r, int c, Tile startTile, List<Tile> connected, ref bool stop, ref int cubeCount)
        {
            // Out of bounds check
            if (r < 0 || r >= gridSize.y || c < 0 || c >= gridSize.x) return;
            if (visited[r, c]) return;

            Tile tile = grid[r, c];
            if (tile == null || tile.isItObstacle) return; // Skip empty or obstacle tiles

            visited[r, c] = true;

            // Check if this tile can match with the starting tile
            if (tile.behavior.CanMatch(tile, startTile, connected, gridManager))
            {
                connected.Add(tile);
                if (tile is CubeTile) cubeCount++;

                if (tile.StopFurtherSearch)
                {
                    stop = true; // Stop further DFS in this direction
                    return;
                }
            }
            else
            {
                stop = true;
                return;
            }

            // Continue DFS in all four directions
            bool stopUp = false, stopDown = false, stopLeft = false, stopRight = false;

            DFS(r + 1, c, startTile, connected, ref stopUp, ref cubeCount);
            DFS(r - 1, c, startTile, connected, ref stopDown, ref cubeCount);
            DFS(r, c + 1, startTile, connected, ref stopRight, ref cubeCount);
            DFS(r, c - 1, startTile, connected, ref stopLeft, ref cubeCount);
        }
    }
}
