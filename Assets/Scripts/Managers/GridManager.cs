using Data;
using DG.Tweening;
using UnityEngine;
using static Tile;

namespace Managers
{
    public class GridManager : MonoBehaviour
    {
        private LevelData levelData;
        public GameObject cubeTilePrefab;
        public GameObject rocketTilePrefab;
        public GameObject passiveTilePrefab;
        public float tileSize = 1f;
        public MoveManager moveManager;
        public GoalManager goalManager;
        private int activeTilesCount;
        public GridGenerator gridGenerator;
        private CollapseManager collapseManager;

        private Tile[,] grid;
        private bool[,] visited;

        void Start()
        {
            levelData = GameManager.Instance.currentLevelData;
            GenerateGrid();
            collapseManager = new CollapseManager(grid, levelData.gridSize, this, goalManager, tileSize);
        }

        private void GenerateGrid()
        {
            CalculateTileSize();
            gridGenerator = new GridGenerator(levelData, cubeTilePrefab, passiveTilePrefab, tileSize, transform);
            grid = gridGenerator.GenerateGrid();
        }

        private void CalculateTileSize()
        {
            int rows = levelData.gridSize.y;
            int columns = levelData.gridSize.x;
            const float boardSizeRatio = 4f;
            tileSize = Mathf.Min(boardSizeRatio / columns, boardSizeRatio / rows);
        }

        public void TryMatch(Tile startTile)
        {
            MatchFinder matchFinder = new MatchFinder(grid, levelData.gridSize, this);
            var connected = matchFinder.FindMatches(startTile, out var cubeCount);

            if (connected == null || cubeCount < 2) return;

            if (!moveManager.UseMove())
            {
                Debug.Log("Game Over!");
                return;
            }

            foreach (var t in connected)
            {
                grid[t.row, t.column] = null;
            }

            if (cubeCount >= 5)
            {
                SpawnRocket(((CubeTile)startTile).row, ((CubeTile)startTile).column, startTile.transform.localPosition);
            }

            OnTilesMatched?.Invoke(connected);
            collapseManager.CollapseAndRefill();
        }

        public Tile GetTile(int r, int c) => grid[r, c];
        public Tile[,] GetGrid() => grid;

        private void SpawnRocket(int row, int column, Vector3 spawnPos)
        {
            GameObject rocketObj = Instantiate(rocketTilePrefab, transform);
            rocketObj.transform.localScale = Vector3.one * tileSize;

            RocketTile rocket = rocketObj.GetComponent<RocketTile>();
            rocket.Initialize();
            rocketObj.transform.localPosition = spawnPos;

            grid[row, column] = rocket;
            rocket.row = row;
            rocket.column = column;
        }

        public void TileActivated() => activeTilesCount++;

        public void TileFinished()
        {
            activeTilesCount--;
            if (activeTilesCount <= 0)
            {
                collapseManager.CollapseAndRefill();
            }
        }

        public Sequence PlayTileDropAnimation(Transform tileTransform, Vector3 endPos)
        {
            const float bounceHeight = 0.15f;
            const float moveDownTime = 0.3f;
            const float bounceUpTime = 0.06f;
            const float settleTime = 0.15f;
            const float scaleAmount = 0.95f;

            Sequence seq = DOTween.Sequence();

            Vector3 originalScale = tileTransform.localScale;

            // First fall
            seq.Append(tileTransform.DOLocalMove(endPos, moveDownTime).SetEase(Ease.OutCubic));

            // little bounce
            seq.Append(
                tileTransform.DOLocalMove(endPos + Vector3.up * bounceHeight, bounceUpTime).SetEase(Ease.OutQuad));

            // last fall
            seq.Append(tileTransform.DOLocalMove(endPos, settleTime).SetEase(Ease.InQuad));

            // Scale updating relative to original scale
            seq.Join(tileTransform.DOScale(originalScale * scaleAmount, settleTime * 0.5f)
                .SetLoops(2, LoopType.Yoyo));

            return seq;
        }

        public bool IsNeighbor(Tile cube, Tile other)
        {
            int dr = Mathf.Abs(cube.row - other.row);
            int dc = Mathf.Abs(cube.column - other.column);
            return (dr == 1 && dc == 0) || (dr == 0 && dc == 1);
        }
    }
}