using System.Collections.Generic;
using Data;
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
        private Dictionary<GoalUI, int> pendingAnimations = new Dictionary<GoalUI, int>();

        void Start()
        {
            levelData = GameManager.Instance.currentLevelData;
            SetupGoals();
        }

        private void OnEnable()
        {
            Tile.OnTilesMatched += CollectTiles;
            Tile.OnTileMatched += CollectTile;
        }

        private void OnDisable()
        {
            Tile.OnTilesMatched -= CollectTiles;
            Tile.OnTileMatched -= CollectTile;
        }

        private void SetupGoals()
        {
            if (goalContainer == null || goalPrefab == null || sampleTilePrefab == null)
            {
                Debug.LogError("GoalManager references are missing!");
                return;
            }

            int uiCount = 0;

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

            if (uiCount < 2 && levelData.ballonGoalCount > 0)
            {
                GameObject go = Instantiate(goalPrefab, goalContainer);
                GoalUI goalUI = go.GetComponent<GoalUI>();
                if (goalUI != null)
                {
                    goalUI.Setup(sampleTilePrefab.balloonSprite, levelData.ballonGoalCount);
                    activeGoalUIs.Add(goalUI);
                    uiCount++;
                }
            }

            if (uiCount < 2 && levelData.duckGoalCount > 0)
            {
                GameObject go = Instantiate(goalPrefab, goalContainer);
                GoalUI goalUI = go.GetComponent<GoalUI>();
                if (goalUI != null)
                {
                    goalUI.Setup(sampleTilePrefab.duckSprite, levelData.duckGoalCount);
                    activeGoalUIs.Add(goalUI);
                    uiCount++;
                }
            }
        }

        private void CollectTiles(List<Tile> tiles, List<Vector3> positions)
        {
            for (int i = 0; i < tiles.Count; i++)
            {
                Tile tile = tiles[i];
                tile.behavior.Behave(GameManager.Instance.currentGridManager,tile);

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
                            isMatching = ui.tileImage.sprite == sampleTilePrefab.balloonSprite &&
                                         ui.GetCurrentCount() > 0;
                            break;
                        case TileType.Duck:
                            isMatching = ui.tileImage.sprite == sampleTilePrefab.duckSprite && ui.GetCurrentCount() > 0;
                            break;
                    }

                    if (isMatching)
                    {
                        if (ui.GetCurrentCount() <= 0)
                        {
                            PoolManager.Instance.ReturnToPool(tile.gameObject);
                            break;
                        }

                        ui.ReduceCount(1);
                        PoolManager.Instance.ReturnToPool(tile.gameObject);
                        if (tile.tileType != TileType.Cube)
                            break;

                        float delay = i * 0.05f;

                        if (!pendingAnimations.ContainsKey(ui))
                            pendingAnimations[ui] = 0;

                        pendingAnimations[ui]++;

                        tileGoalAnimator.AnimateToGoal(
                            tile.sr.sprite,
                            positions[i], 
                            ui.tileImage.transform,
                            () =>
                            {
                                pendingAnimations[ui]--;
                                if (pendingAnimations[ui] <= 0)
                                    tileGoalAnimator.SpawnGoalParticle(ui.tileImage.transform);
                            },
                            delay
                        );
                        break;
                    }

                    PoolManager.Instance.ReturnToPool(tile.gameObject);
                }
            }
        }

        public void CollectTile(Tile tile, Vector3 position)
        {
            if (tile == null) return;

            CollectTiles(new List<Tile> { tile }, new List<Vector3> { position });
        }
    }
}
