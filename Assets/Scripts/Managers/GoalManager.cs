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
        private Dictionary<GoalUI, int> pendingAnimations = new Dictionary<GoalUI, int>(); // Track ongoing animations per goal UI

        void Start()
        {
            levelData = GameManager.Instance.currentLevelData;
            SetupGoals(); // Initialize the goal UI elements
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

            // Balloon goal setup
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

            // Duck goal setup
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
                tile.SetIsMatchable();
                
                if (tile.GetIsMatchable())
                {
                    // Trigger tile behavior (like duck or balloon special effects)
                    tile.behavior.Behave(GameManager.Instance.currentGridManager, tile);
                    Debug.Log("deneme");
                }

                foreach (var ui in activeGoalUIs)
                {
                    bool isMatching = false;

                    // Determine if this tile matches the current goal UI
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

                        PoolManager.Instance.ReturnToPool(tile.gameObject);

                        // For non-cube tiles, just reduce the goal count immediately
                        if (tile.tileType != TileType.Cube)
                        {
                            ui.ReduceCount(1);
                            break;
                        }

                        float delay = i * 0.05f;

                        // Initialize pending animation count if not already present
                        if (!pendingAnimations.ContainsKey(ui))
                            pendingAnimations[ui] = 0;

                        // Limit the number of cube animations to remaining goal count
                        if (pendingAnimations[ui] >= ui.GetCurrentCount())
                        {
                            break;
                        }

                        pendingAnimations[ui]++;

                        // Animate the tile flying to the goal UI
                        tileGoalAnimator.AnimateToGoal(
                            tile.sr.sprite,
                            positions[i],
                            ui.tileImage.transform,
                            () =>
                            {
                                ui.ReduceCount(1);
                                pendingAnimations[ui]--;
                                if (pendingAnimations[ui] <= 0)
                                    tileGoalAnimator.SpawnGoalParticle(ui.tileImage.transform); // Spawn particle only when all animations complete
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
