using Data;
using Managers;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
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
            CalculateTileSize(); // calculate the size of each tile based on board dimensions
            UpdateBackgroundSize(); // adjust the background image to fit the board
        }

        private void CalculateTileSize()
        {
            levelData = GameManager.Instance.currentLevelData;
            int rows = levelData.gridSize.y;
            int columns = levelData.gridSize.x;

            const float boardSizeRatio = 4f;

            tileSize = Mathf.Min(boardSizeRatio / columns, boardSizeRatio / rows); 
            // ensures tiles fit within the board area
        }

        private void UpdateBackgroundSize()
        {
            // multipliers for additional padding/scaling
            float xMultiplier = 1f / (levelData.gridSize.x * 0.2f + 0.1f) * 20;
            float yMultiplier = 1f / (levelData.gridSize.y * 0.2f + 0.1f) * 30;

            // set the background size to cover the entire grid
            backgroundImage.rectTransform.sizeDelta = new Vector2(
                levelData.gridSize.x * (tileSize * 100) * 2 + xMultiplier,
                levelData.gridSize.y * (tileSize * 100) * 2 + yMultiplier
            );
            backgroundImage.rectTransform.localScale = Vector3.one;
            backgroundImage.rectTransform.localPosition = Vector3.zero;
        }
    }
}
