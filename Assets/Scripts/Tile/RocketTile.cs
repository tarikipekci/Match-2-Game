using DG.Tweening;
using Interfaces;
using Managers;
using UnityEngine;

public class RocketTile : Tile, IActivatable
{
    public enum RocketDirection
    {
        Horizontal,
        Vertical
    }

    public RocketDirection direction;

    public Transform leftPart;
    public Transform rightPart;

    private bool hasActivated;

    public void Initialize()
    {
        direction = Random.value < 0.5f ? RocketDirection.Horizontal : RocketDirection.Vertical;
        transform.rotation = direction == RocketDirection.Vertical
            ? Quaternion.Euler(0, 0, 90)
            : Quaternion.identity;
    }

    public void Activate(GridManager board)
    {
        if (hasActivated) return;
        hasActivated = true;

        if (direction == RocketDirection.Horizontal)
            Launch(board, true);
        else
            Launch(board, false);
        
        board.TileActivated();
    }

    private void Launch(GridManager board, bool horizontal)
    {
        int row = this.row;
        int col = column;

        const float duration = 0.5f;
        const float distanceMultiplier = 3f;
        float totalDist = horizontal
            ? GameManager.Instance.currentLevelData.gridSize.x * board.tileSize * distanceMultiplier
            : GameManager.Instance.currentLevelData.gridSize.y * board.tileSize * distanceMultiplier;

        if (horizontal)
        {
            for (int c = 0; c < GameManager.Instance.currentLevelData.gridSize.x; c++)
            {
                Tile tile = board.GetTile(row, c);
                if (tile == null || tile.isItObstacle) continue;

                float dist = Mathf.Abs(c - col) * board.tileSize;
                float delay = (dist / totalDist) * duration;

                DOVirtual.DelayedCall(delay, () => HandleTile(tile, board));
            }
        }
        else
        {
            for (int r = 0; r < GameManager.Instance.currentLevelData.gridSize.y; r++)
            {
                Tile tile = board.GetTile(r, col);
                if (tile == null || tile.isItObstacle) continue;

                float dist = Mathf.Abs(r - row) * board.tileSize;
                float delay = (dist / totalDist) * duration;

                DOVirtual.DelayedCall(delay, () => HandleTile(tile, board));
            }
        }

        Vector3 move1 = horizontal
            ? -transform.right * totalDist
            : transform.up * totalDist;
        Vector3 move2 = -move1;

        leftPart.DOLocalMove(leftPart.localPosition + move1, duration).SetEase(Ease.Linear);
        rightPart.DOLocalMove(rightPart.localPosition + move2, duration).SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                board.GetGrid()[row, col] = null;
                Destroy(gameObject);
                board.TileFinished();
            });
    }

    private void HandleTile(Tile tile, GridManager board)
    {
        if (tile == null || tile.isItObstacle) return;

        if (tile is IActivatable activatableTile)
            activatableTile.Activate(board);
        else
        {
            tile.particleEffect = particleEffect;
            OnTileMatched?.Invoke(tile, tile.transform.position); 
            board.GetGrid()[tile.row, tile.column] = null;
        }
    }
}
