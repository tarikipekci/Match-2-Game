using System.Collections.Generic;
using UnityEngine;

public class GoalManager : MonoBehaviour
{
    public LevelData levelData;
    public Transform goalContainer;
    public GameObject goalPrefab;
    public Tile sampleTilePrefab;

    private List<GoalUI> activeGoalUIs = new List<GoalUI>();

    void Start()
    {
        SetupGoals();
    }

    private void SetupGoals()
    {
        if (goalContainer == null || goalPrefab == null || sampleTilePrefab == null)
        {
            Debug.LogError("GoalManager references are missing!");
            return;
        }

        CubeGoal[] goalsToUse;

        if (levelData.cubeGoals is { Length: > 0 })
        {
            int goalCount = Mathf.Min(2, levelData.cubeGoals.Length);
            goalsToUse = new CubeGoal[goalCount];
            for (int i = 0; i < goalCount; i++)
            {
                goalsToUse[i] = levelData.cubeGoals[i];
            }
        }
        else
        {
            int goalCount = Random.Range(1, 3);
            goalsToUse = new CubeGoal[goalCount];
            for (int i = 0; i < goalCount; i++)
            {
                int randIndex = Random.Range(0, 5);
                goalsToUse[i] = new CubeGoal
                {
                    color = (TileColor)(randIndex + 1),
                    targetCount = Random.Range(5, 11)
                };
            }
        }

        foreach (var goal in goalsToUse)
        {
            GameObject go = Instantiate(goalPrefab, goalContainer);
            GoalUI goalUI = go.GetComponent<GoalUI>();
            if (goalUI != null)
            {
                Sprite sprite = sampleTilePrefab.GetSpriteForColor(goal.color);
                goalUI.Setup(sprite, goal.targetCount);
                activeGoalUIs.Add(goalUI);
            }
            else
            {
                Debug.LogError("GoalPrefab missing GoalUI component!");
            }
        }
    }

    public void CollectTile(Tile tile)
    {
        foreach (var ui in activeGoalUIs)
        {
            if (ui.tileImage.sprite == tile.GetSpriteForColor(tile.tileColor) && ui.GetCurrentCount() > 0)
            {
                ui.ReduceCount(1);
                Destroy(tile.gameObject);
                return;
            }
        }

        Destroy(tile.gameObject);
    }
}