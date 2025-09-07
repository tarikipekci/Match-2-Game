using UnityEngine;
using TMPro;

public class MoveManager : MonoBehaviour
{
    public LevelData levelData; 
    private int remainingMoves;
    public TMP_Text movesText;

    void Start()
    {
        remainingMoves = levelData.moveCount;
        UpdateUI();
    }

    public bool UseMove()
    {
        if (remainingMoves <= 0) return false;

        remainingMoves--;
        UpdateUI();
        return true;
    }

    private void UpdateUI()
    {
        if (movesText != null)
            movesText.text = remainingMoves.ToString();
    }
}
