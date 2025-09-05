using System;
using System.Collections.Generic;
using UnityEngine;

public class GoalManager : MonoBehaviour
{
    public LevelData levelData;

    private Dictionary<TileColor, int> collectedCubesPerColor;
    private int collectedBalloons;
    private int collectedDucks;

    void Start()
    {
        collectedCubesPerColor = new Dictionary<TileColor, int>();
        foreach (var cubeGoal in levelData.cubeGoals)
            collectedCubesPerColor[cubeGoal.color] = 0;

        collectedBalloons = 0;
        collectedDucks = 0;
        UpdateUI();
    }

    public void CollectTile(Tile tile)
    {
        switch(tile.tileType)
        {
            case TileType.Cube:
                var goal = Array.Find(levelData.cubeGoals, g => g.color == tile.tileColor);
                if (goal != null)
                {
                    collectedCubesPerColor[tile.tileColor]++;
                    if (collectedCubesPerColor[tile.tileColor] > goal.targetCount)
                        collectedCubesPerColor[tile.tileColor] = goal.targetCount; // do not exceed the limit
                }
                break;

            case TileType.Balloon:
                collectedBalloons++;
                if (collectedBalloons > levelData.targetBalloonCount)
                    collectedBalloons = levelData.targetBalloonCount;
                break;

            case TileType.Duck:
                collectedDucks++;
                if (collectedDucks > levelData.targetDuckCount)
                    collectedDucks = levelData.targetDuckCount;
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
       // Updating UI
    }

    public bool IsLevelComplete()
    {
        foreach(var cubeGoal in levelData.cubeGoals)
        {
            if (collectedCubesPerColor[cubeGoal.color] < cubeGoal.targetCount)
                return false;
        }

        return collectedBalloons >= levelData.targetBalloonCount &&
               collectedDucks >= levelData.targetDuckCount;
    }
}
