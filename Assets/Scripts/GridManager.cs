using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public LevelData levelData;
    public GameObject tilePrefab;
    public float tileSize = 1f;

    private Tile[,] grid;
    private bool[,] visited;

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        int rows = levelData.gridSize.y;
        int columns = levelData.gridSize.x;
        grid = new Tile[rows, columns];

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                GameObject tileObj = Instantiate(tilePrefab, transform);
                tileObj.transform.localPosition = new Vector3(c * tileSize, -r * tileSize, 0);

                Tile tile = tileObj.GetComponent<Tile>();

                tile.row = r;
                tile.column = c;

                if (levelData.startingRows != null && r < levelData.startingRows.Length)
                {
                    var row = levelData.startingRows[r];
                    if (row.tiles != null && c < row.tiles.Length)
                    {
                        tile.tileType = row.tiles[c].tileType;
                        tile.tileColor = row.tiles[c].tileColor;
                    }
                    else
                    {
                        SetRandomCube(tile);
                    }
                }
                else
                {
                    SetRandomCube(tile);
                }

                tile.UpdateSprite();
                grid[r, c] = tile;
            }
        }
    }

    private void SetRandomCube(Tile tile)
    {
        tile.tileType = TileType.Cube;
        if (levelData.cubeGoals.Length > 0)
        {
            int randIndex = Random.Range(0, levelData.cubeGoals.Length);
            tile.tileColor = levelData.cubeGoals[randIndex].color;
        }
        else
        {
            tile.tileColor = TileColor.Red;
        }
    }

    public void TryMatch(Tile startTile)
    {
        if (startTile.tileType != TileType.Cube) return;

        visited = new bool[levelData.gridSize.y, levelData.gridSize.x];
        List<Tile> connected = new List<Tile>();

        DFS(startTile.row, startTile.column, startTile.tileColor, connected);

        if (connected.Count >= 2)
        {
            foreach (var t in connected)
            {
                Destroy(t.gameObject);
                grid[t.row, t.column] = null;
            }
        }
    }

    private void DFS(int r, int c, TileColor color, List<Tile> connected)
    {
        if (r < 0 || r >= levelData.gridSize.y || c < 0 || c >= levelData.gridSize.x)
            return;

        if (visited[r, c]) return;
        Tile tile = grid[r, c];
        if (tile == null || tile.tileType != TileType.Cube || tile.tileColor != color) return;

        visited[r, c] = true;
        connected.Add(tile);

        DFS(r + 1, c, color, connected);
        DFS(r - 1, c, color, connected);
        DFS(r, c + 1, color, connected);
        DFS(r, c - 1, color, connected);
    }
}
