using System.Collections.Generic;

public class RocketBehavior : ITileBehavior
{
    public void Behave(GridManager grid, Tile tile)
    {
        if (tile is RocketTile rocketTile)
        {
            rocketTile.Activate(grid);
        }
    }

    public bool CanMatch(Tile self, Tile startTile, List<Tile> connected, GridManager grid)
    {
        return false;
    }
}