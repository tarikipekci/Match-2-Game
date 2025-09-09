using System.Collections.Generic;

public class DuckBehavior : ITileBehavior
{
    public void Behave(GridManager grid, Tile tile)
    {
        if (tile == null) return;

        if (tile.row == GameManager.Instance.currentLevelData.gridSize.y - 1)
        {
            // match
        }
    }

    public bool CanMatch(Tile self, Tile startTile, List<Tile> connected, GridManager grid)
    {
        return false;
    }
}