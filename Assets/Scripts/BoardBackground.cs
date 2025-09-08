using UnityEngine;
using UnityEngine.UI;

public class BoardBackground : MonoBehaviour
{
    public GridManager gridManager;
    public Image backgroundImage;
    private LevelData levelData;
    private float tileSize;
    private float xMultiplier;
    private float yMultiplier;

    void Start()
    {
        CalculateTileSize();
        UpdateBackgroundSize();
    }

    private void CalculateTileSize()
    {
        levelData = gridManager.levelData;
        int rows = levelData.gridSize.y;
        int columns = levelData.gridSize.x;

        const float boardSizeRatio = 4f;

        tileSize = Mathf.Min(boardSizeRatio / columns, boardSizeRatio / rows);
    }

    private void UpdateBackgroundSize()
    {
        float xMultiplier = 1f / (levelData.gridSize.x * 0.2f + 0.1f) * 20;
        float yMultiplier = 1f / (levelData.gridSize.y * 0.2f + 0.1f) * 30;

        backgroundImage.rectTransform.sizeDelta = new Vector2(levelData.gridSize.x * (tileSize * 100)*2 + xMultiplier,
            levelData.gridSize.y * (tileSize * 100)*2 + yMultiplier);
        backgroundImage.rectTransform.localScale = Vector3.one;
        backgroundImage.rectTransform.localPosition = Vector3.zero;
    }
}