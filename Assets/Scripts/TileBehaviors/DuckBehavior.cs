using System.Collections.Generic;
using Interfaces;
using Managers;

namespace TileBehaviors
{
    public class DuckBehavior : ITileBehavior
    {
        public DuckBehavior()
        {
            CollapseManager.OnTileLanded += Behave;
        }

        public void Behave(GridManager grid, Tile tile)
        {
            if (tile.tileType == TileType.Duck)
            {
                int lastRow = GameManager.Instance.currentLevelData.gridSize.y - 1;

                if (tile.row == lastRow)
                {
                    SoundManager.Instance.PlaySound(SoundManager.Instance.duckSound);
                    PoolManager.Instance.ReturnToPool(tile.gameObject);
                    grid.GetGrid()[tile.row, tile.column] = null;
                }
            }
        }

        public bool CanMatch(Tile self, Tile startTile, List<Tile> connected, GridManager grid)
        {
            return false; // duck cannot match
        }
    }
}