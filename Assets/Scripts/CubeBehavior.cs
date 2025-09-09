using System.Collections.Generic;

public class CubeBehavior : ITileBehavior
{
    public void Behave(GridManager grid, Tile tile)
    {
        // Cube behavior
    }

    public bool CanMatch(Tile self, Tile startTile, List<Tile> connected, GridManager grid)
    {
        if (self is not CubeTile cube || startTile is not CubeTile startCube)
            return false;

        return cube.tileColor == startCube.tileColor;
    }
}