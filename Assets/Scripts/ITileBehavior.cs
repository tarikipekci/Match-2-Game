using System.Collections.Generic;

public interface ITileBehavior
{
    void Behave(GridManager grid, Tile tile);
    bool CanMatch(Tile self, Tile startTile, List<Tile> connected, GridManager grid);
}
