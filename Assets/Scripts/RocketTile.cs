using DG.Tweening;
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

        board.TileActivated();

        if (direction == RocketDirection.Horizontal)
            Launch(board, true);
        else
            Launch(board, false);
    }

    private void Launch(GridManager board, bool horizontal)
    {
        int row = this.row;
        int col = this.column;

        if (horizontal)
        {
            for (int c = 0; c < board.levelData.gridSize.x; c++)
                HandleTile(board.GetTile(row, c), board);
        }
        else
        {
            for (int r = 0; r < board.levelData.gridSize.y; r++)
                HandleTile(board.GetTile(r, col), board);
        }

        Vector3 move1 = horizontal
            ? -transform.right * board.levelData.gridSize.x * board.tileSize
            : transform.up * board.levelData.gridSize.y * board.tileSize;
        Vector3 move2 = -move1;

        leftPart.DOLocalMove(leftPart.localPosition + move1, 0.5f).SetEase(Ease.Linear);
        rightPart.DOLocalMove(rightPart.localPosition + move2, 0.5f).SetEase(Ease.Linear)
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
            board.GetGrid()[tile.row, tile.column] = null;
            Destroy(tile.gameObject);
        }
    }
}