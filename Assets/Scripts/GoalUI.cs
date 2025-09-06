using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GoalUI : MonoBehaviour
{
    public Image tileImage;
    public TMP_Text countText;

    private int currentCount;

    public void Setup(Sprite sprite, int targetCount)
    {
        tileImage.sprite = sprite;
        currentCount = targetCount;
        UpdateUI();
    }

    public void ReduceCount(int amount)
    {
        currentCount -= amount;
        if (currentCount < 0) currentCount = 0;
        UpdateUI();
    }

    private void UpdateUI()
    {
        countText.text = currentCount.ToString();
    }

    public int GetCurrentCount()
    {
        return currentCount;
    }
}
