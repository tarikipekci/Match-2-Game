using System.Collections.Generic;
using Interfaces;
using Managers;

namespace TileBehaviors
{
    public class BalloonBehavior : ITileBehavior
    {
        public void Behave(GridManager grid, Tile tile)
        {
            // Balloon behavior
        }

        public bool CanMatch(Tile self, Tile startTile, List<Tile> connected, GridManager grid)
        {
            if (self.tileType != TileType.Balloon) return false;
            if (connected == null || connected.Count == 0) return false;

            foreach (var cube in connected)
            {
                if (cube is CubeTile && grid.IsNeighbor(cube, self))
                {
                    self.StopFurtherSearch = true;
                    return true;
                }
            }

            return false;
        }
    }
}