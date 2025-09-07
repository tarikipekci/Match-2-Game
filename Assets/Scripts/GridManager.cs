using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public LevelData levelData;
    public GameObject tilePrefab;
    public float tileSize = 1f;
    public MoveManager moveManager;

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

        float boardWidth = columns * tileSize;
        float boardHeight = rows * tileSize;
        float startX = -boardWidth / 2 + tileSize / 2;
        float startY = boardHeight / 2 - tileSize / 2;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                GameObject tileObj = Instantiate(tilePrefab, transform);
                tileObj.transform.localPosition = new Vector3(startX + c * tileSize, startY - r * tileSize, 0);

                Tile tile = tileObj.GetComponent<Tile>();

                tile.row = r;
                tile.column = c;

                if (levelData.startingRows != null && r < levelData.startingRows.Length)
                {
                    var rowData = levelData.startingRows[r];
                    if (rowData.tiles != null && c < rowData.tiles.Length)
                    {
                        tile.tileType = rowData.tiles[c].tileType;
                        tile.tileColor = rowData.tiles[c].tileColor;
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
            if (!moveManager.UseMove())
            {
                Debug.Log("Game Over!");
                return;
            }

            foreach (var t in connected)
            {
                Color tileColor = SelectTileColor.GetColor(t.tileColor);
                ParticleManager.Instance.SpawnCubeParticles(t.transform.position, tileColor);
                grid[t.row, t.column] = null;
            }

            CollapseColumns();
            RefillGrid();
            FindObjectOfType<GoalManager>()?.CollectTiles(connected);
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

    private void CollapseColumns()
    {
        int rows = levelData.gridSize.y;
        int columns = levelData.gridSize.x;

        float boardWidth = columns * tileSize;
        float boardHeight = rows * tileSize;
        float startX = -boardWidth / 2 + tileSize / 2;
        float startY = boardHeight / 2 - tileSize / 2;

        for (int c = 0; c < columns; c++)
        {
            int emptyRow = rows - 1;
            for (int r = rows - 1; r >= 0; r--)
            {
                if (grid[r, c] != null)
                {
                    if (r != emptyRow)
                    {
                        Tile tile = grid[r, c];
                        grid[emptyRow, c] = tile;
                        grid[r, c] = null;

                        tile.row = emptyRow;
                        tile.column = c;

                        Vector3 endPos = new Vector3(startX + c * tileSize, startY - emptyRow * tileSize, 0);
                        tile.transform.DOLocalMove(endPos, 1f + Random.Range(0f, 0.1f)).SetEase(Ease.OutBounce);
                    }
                    emptyRow--;
                }
            }
        }
    }

    private void RefillGrid()
    {
        int rows = levelData.gridSize.y;
        int columns = levelData.gridSize.x;

        float boardWidth = columns * tileSize;
        float boardHeight = rows * tileSize;
        float startX = -boardWidth / 2 + tileSize / 2;
        float startY = boardHeight / 2 - tileSize / 2;

        for (int c = 0; c < columns; c++)
        {
            for (int r = 0; r < rows; r++)
            {
                if (grid[r, c] == null)
                {
                    GameObject tileObj = Instantiate(tilePrefab, transform);
                    Tile tile = tileObj.GetComponent<Tile>();

                    tile.row = r;
                    tile.column = c;
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

                    tile.UpdateSprite();
                    grid[r, c] = tile;

                    float startTileY = startY - r * tileSize + tileSize * boardHeight;
                    tileObj.transform.localPosition = new Vector3(startX + c * tileSize, startTileY, 0);

                    Vector3 endPos = new Vector3(startX + c * tileSize, startY - r * tileSize, 0);
                    tileObj.transform.DOLocalMove(endPos, 0.8f + Random.Range(0f, 0.05f)).SetEase(Ease.OutBounce);
                }
            }
        }
    }
}
