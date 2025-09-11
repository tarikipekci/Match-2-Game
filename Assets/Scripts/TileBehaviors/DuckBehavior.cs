using System.Collections.Generic;
using Interfaces;
using Managers;

namespace TileBehaviors
{
    public class DuckBehavior : ITileBehavior
    {
        private static bool isSubscribed;

        public DuckBehavior()
        {
            if (!isSubscribed)
            {
                CollapseManager.OnTileLanded += Behave;
                isSubscribed = true;
            }
        }

        public void Behave(GridManager grid, Tile tile)
        {
            tile.SetIsMatchable(false);
            if (tile.tileType == TileType.Duck)
            {
                int lastRow = GameManager.Instance.currentLevelData.gridSize.y - 1;

                if (tile.row == lastRow)
                {
                    SoundManager.Instance.PlaySound(SoundManager.Instance.duckSound);
                    tile.particleEffect = tile.duckParticleEffect;
                    ParticleManager.Instance.SpawnPassiveParticles(tile, tile.transform.localPosition);
                    grid.goalManager.CollectTile(tile, tile.transform.localPosition);
                    grid.GetGrid()[tile.row, tile.column] = null;
                    InputManager.DisableInput();
                }
            }
        }

        public bool CanMatch(Tile self, Tile startTile, List<Tile> connected, GridManager grid)
        {
            return false; // duck cannot match
        }
    }
}