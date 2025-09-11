using System.Collections.Generic;
using Interfaces;
using Managers;

namespace TileBehaviors
{
    public class CubeBehavior : ITileBehavior
    {
        public void Behave(GridManager grid, Tile tile)
        {
            // Cube behavior
            if (tile.tileType == TileType.Cube)
            {
                tile.SetIsMatchable(true);
                SoundManager.Instance.PlaySound(SoundManager.Instance.cubeExplode);
            }
        }

        public bool CanMatch(Tile self, Tile startTile, List<Tile> connected, GridManager grid)
        {
            if (self is not CubeTile cube || startTile is not CubeTile startCube)
                return false;

            return cube.tileColor == startCube.tileColor;
        }
    }
}