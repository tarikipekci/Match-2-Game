using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class BoardBackground : MonoBehaviour
{
    public GridManager gridManager;
    public Image backgroundImage;
    public Canvas canvas;

    void Start()
    {
        if (canvas == null)
            canvas = backgroundImage.canvas;

        UpdateBackgroundSize();
    }

    private void UpdateBackgroundSize()
    {
        int rows = gridManager.levelData.gridSize.y;
        int columns = gridManager.levelData.gridSize.x;

        float boardWidthWorld = columns * gridManager.tileSize;
        float boardHeightWorld = rows * gridManager.tileSize;

        float canvasHeight = canvas.pixelRect.height;
        float canvasWidth = canvas.pixelRect.width;

        float scaleX = canvasWidth / boardWidthWorld;
        float scaleY = canvasHeight / boardHeightWorld;
        float scale = Mathf.Min(scaleX, scaleY);

        float width = boardWidthWorld * scale;
        float height = boardHeightWorld * scale;

        backgroundImage.rectTransform.sizeDelta = new Vector2(width, height);
        backgroundImage.rectTransform.localPosition = Vector3.zero;
    }
}