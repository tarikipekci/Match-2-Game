using System.Collections.Generic;
using Helper;
using UI;
using UnityEngine;

namespace Managers
{
    public class GoalManager : MonoBehaviour
    {
        private LevelData levelData;
        public Transform goalContainer;
        public GameObject goalPrefab;
        public CubeTile sampleTilePrefab;
        public TileGoalAnimator tileGoalAnimator;

        private List<GoalUI> activeGoalUIs = new List<GoalUI>();

        void Start()
        {
            levelData = GameManager.Instance.currentLevelData;
            SetupGoals();
        }

        private void OnEnable()
        {
            Tile.OnTilesMatched += CollectTiles;
        }

        private void OnDisable()
        {
            Tile.OnTilesMatched -= CollectTiles;
        }

        private void SetupGoals()
        {
            if (goalContainer == null || goalPrefab == null || sampleTilePrefab == null)
            {
                Debug.LogError("GoalManager references are missing!");
                return;
            }

            int uiCount = 0;

            // --- Cube Goals ---
            if (levelData.cubeGoals is { Length: > 0 })
            {
                int cubeGoalCount = Mathf.Min(2, levelData.cubeGoals.Length);
                for (int i = 0; i < cubeGoalCount; i++)
                {
                    if (uiCount >= 2) break;

                    CubeGoal goal = levelData.cubeGoals[i];
                    GameObject go = Instantiate(goalPrefab, goalContainer);
                    GoalUI goalUI = go.GetComponent<GoalUI>();
                    if (goalUI != null)
                    {
                        Sprite sprite = sampleTilePrefab is { } cubeSample
                            ? cubeSample.GetSpriteForColor(goal.color)
                            : sampleTilePrefab.GetSpriteForColor(goal.color);

                        goalUI.Setup(sprite, goal.targetCount);
                        activeGoalUIs.Add(goalUI);
                        uiCount++;
                    }
                }
            }

            // --- Balloon Goal ---
            if (uiCount < 2 && levelData.targetBalloonCount > 0)
            {
                GameObject go = Instantiate(goalPrefab, goalContainer);
                GoalUI goalUI = go.GetComponent<GoalUI>();
                if (goalUI != null)
                {
                    goalUI.Setup(sampleTilePrefab.balloonSprite, levelData.targetBalloonCount);
                    activeGoalUIs.Add(goalUI);
                    uiCount++;
                }
            }

            // --- Duck Goal ---
            if (uiCount < 2 && levelData.targetDuckCount > 0)
            {
                GameObject go = Instantiate(goalPrefab, goalContainer);
                GoalUI goalUI = go.GetComponent<GoalUI>();
                if (goalUI != null)
                {
                    goalUI.Setup(sampleTilePrefab.duckSprite, levelData.targetDuckCount);
                    activeGoalUIs.Add(goalUI);
                    uiCount++;
                }
            }
        }

        private void CollectTiles(List<Tile> tiles)
        {
            for (int i = 0; i < tiles.Count; i++)
            {
                Tile tile = tiles[i];

                foreach (var ui in activeGoalUIs)
                {
                    bool isMatching = false;

                    switch (tile.tileType)
                    {
                        case TileType.Cube:
                            if (tile is CubeTile cubeTile)
                                isMatching = ui.tileImage.sprite == cubeTile.GetSpriteForColor(cubeTile.tileColor) &&
                                             ui.GetCurrentCount() > 0;
                            break;
                        case TileType.Balloon:
                            isMatching = ui.tileImage.sprite == sampleTilePrefab.balloonSprite && ui.GetCurrentCount() > 0;
                            break;
                        case TileType.Duck:
                            isMatching = ui.tileImage.sprite == sampleTilePrefab.duckSprite && ui.GetCurrentCount() > 0;
                            break;
                    }

                    if (isMatching)
                    {
                        Destroy(tile.gameObject);

                        float delay = i * 0.05f;

                        tileGoalAnimator.AnimateToGoal(
                            tile.sr.sprite,
                            tile,
                            ui.tileImage.transform,
                            () => ui.ReduceCount(1),
                            delay
                        );
                        break;
                    }

                    Destroy(tile.gameObject);
                }
            }
        }

        public void CollectTile(Tile tile)
        {
            if (tile == null) return;

            CollectTiles(new List<Tile> { tile });
        }
    }
}