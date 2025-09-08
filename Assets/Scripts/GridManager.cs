using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public LevelData levelData;
    public GameObject tilePrefab;
    public GameObject rocketPrefab;
    public float tileSize = 1f;
    public MoveManager moveManager;
    public GoalManager goalManager;
    private int activeTilesCount;

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
        Vector3 startPos = startTile.transform.localPosition;

        visited = new bool[levelData.gridSize.y, levelData.gridSize.x];
        List<Tile> connected = new List<Tile>();

        DFS(startTile.row, startTile.column, startTile.tileColor, connected, out var cubeCount);

        if (cubeCount >= 2)
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

            if (cubeCount >= 5)
            {
                SpawnRocket(startTile.row, startTile.column, startPos);
            }

            CollapseColumns();
            RefillGrid();
            FindObjectOfType<GoalManager>()?.CollectTiles(connected);
        }
    }

    private void DFS(int r, int c, TileColor color, List<Tile> connected, out int cubeCount, bool ballonFound = false)
    {
        cubeCount = 0;

        if (r < 0 || r >= levelData.gridSize.y || c < 0 || c >= levelData.gridSize.x)
            return;

        if (visited[r, c]) return;

        Tile tile = grid[r, c];
        if (tile == null) return;

        if (tile.isItObstacle) return;

        if (tile.tileType == TileType.Cube && tile.tileColor != color) return;

        if (tile.tileType is TileType.Duck or TileType.Rocket) return;

        if (ballonFound) return;

        visited[r, c] = true;

        if (tile.tileType == TileType.Cube)
        {
            connected.Add(tile);
            cubeCount = 1;
        }
        else if (tile.tileType == TileType.Balloon)
        {
            connected.Add(tile);
            ballonFound = true;
        }

        DFS(r + 1, c, color, connected, out var tempCount, ballonFound);
        cubeCount += tempCount;

        DFS(r - 1, c, color, connected, out tempCount, ballonFound);
        cubeCount += tempCount;

        DFS(r, c + 1, color, connected, out tempCount, ballonFound);
        cubeCount += tempCount;

        DFS(r, c - 1, color, connected, out tempCount, ballonFound);
        cubeCount += tempCount;
    }

  private void CollapseColumns()
{
    int rows = levelData.gridSize.y;
    int columns = levelData.gridSize.x;

    float boardWidth = columns * tileSize;
    float boardHeight = rows * tileSize;
    float startX = -boardWidth / 2 + tileSize / 2;
    float startY = boardHeight / 2 - tileSize / 2;

    List<Tile> ducksToRemove = new List<Tile>();
    int CurrentTweens = 0;

    for (int c = 0; c < columns; c++)
    {
        int emptyRow = rows - 1;
        for (int r = rows - 1; r >= 0; r--)
        {
            Tile tile = grid[r, c];
            if (tile != null)
            {
                if (r != emptyRow)
                {
                    grid[emptyRow, c] = tile;
                    grid[r, c] = null;

                    tile.row = emptyRow;
                    tile.column = c;

                    Vector3 endPos = new Vector3(startX + c * tileSize, startY - emptyRow * tileSize, 0);

                    CurrentTweens++;

                    PlayTileDropAnimation(tile.transform, endPos).OnComplete(() =>
                    {
                        CurrentTweens--;
                        if (tile.tileType == TileType.Duck && tile.row == rows - 1)
                            ducksToRemove.Add(tile);

                        if (CurrentTweens == 0)
                        {
                            foreach (var duck in ducksToRemove)
                            {
                                goalManager.CollectTile(duck);
                                grid[duck.row, duck.column] = null;
                            }

                            CollapseColumns();
                            RefillGrid();
                        }
                    });
                }

                emptyRow--;
            }
        }
    }

    if (CurrentTweens == 0)
    {
        for (int c = 0; c < columns; c++)
        {
            Tile bottomTile = grid[rows - 1, c];
            if (bottomTile != null && bottomTile.tileType == TileType.Duck)
            {
                Destroy(bottomTile.gameObject);
                grid[rows - 1, c] = null;
            }
        }

        RefillGrid();
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

                float random = Random.value;

                float balloonChance = levelData.targetBalloonCount > 0 ? 0.2f : 0.05f;
                float duckChance = levelData.targetDuckCount > 0 ? 0.15f : 0.03f;

                if (random < balloonChance) tile.tileType = TileType.Balloon;
                else if (random < balloonChance + duckChance) tile.tileType = TileType.Duck;
                else tile.tileType = TileType.Cube;

                if (tile.tileType == TileType.Cube)
                {
                    if (levelData.cubeGoals.Length > 0)
                    {
                        int randIndex = Random.Range(0, levelData.cubeGoals.Length);
                        tile.tileColor = levelData.cubeGoals[randIndex].color;
                    }
                    else tile.tileColor = TileColor.Red;
                }

                tile.UpdateSprite();
                grid[r, c] = tile;

                float startTileY = startY - r * tileSize + tileSize * boardHeight;
                tileObj.transform.localPosition = new Vector3(startX + c * tileSize, startTileY, 0);

                Vector3 endPos = new Vector3(startX + c * tileSize, startY - r * tileSize, 0);
                PlayTileDropAnimation(tileObj.transform, endPos);
            }
        }
    }
}

    public Tile GetTile(int r, int c)
    {
        return grid[r, c];
    }

    public Tile[,] GetGrid()
    {
        return grid;
    }

    private void SpawnRocket(int row, int column, Vector3 spawnPos)
    {
        GameObject rocketObj = Instantiate(rocketPrefab, transform);
        RocketTile rocket = rocketObj.GetComponent<RocketTile>();
        rocket.Initialize();

        rocketObj.transform.localPosition = spawnPos;
        grid[row, column] = rocket.GetComponent<Tile>();
        rocket.GetComponent<Tile>().row = row;
        rocket.GetComponent<Tile>().column = column;
    }

    public void TileActivated()
    {
        activeTilesCount++;
    }

    public void TileFinished()
    {
        activeTilesCount--;
        if (activeTilesCount <= 0)
        {
            CollapseColumns();
            RefillGrid();
        }
    }

    private Sequence PlayTileDropAnimation(Transform tileTransform, Vector3 endPos)
    {
        const float bounceHeight = 0.15f;
        const float moveDownTime = 0.3f;
        const float bounceUpTime = 0.06f;
        const float settleTime = 0.15f;

        Sequence seq = DOTween.Sequence();

        // First fall
        seq.Append(tileTransform.DOLocalMove(endPos, moveDownTime).SetEase(Ease.OutCubic));

        // little bounce
        seq.Append(tileTransform.DOLocalMove(endPos + Vector3.up * bounceHeight, bounceUpTime).SetEase(Ease.OutQuad));

        // last fall
        seq.Append(
            tileTransform.DOLocalMove(endPos, settleTime).SetEase(Ease.InQuad)
        );

        // Scale updating
        seq.Join(tileTransform.DOScale(0.95f, settleTime * 0.5f).SetLoops(2, LoopType.Yoyo));

        return seq;
    }
}