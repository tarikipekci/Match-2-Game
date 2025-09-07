using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TileGoalAnimator : MonoBehaviour
{
    [SerializeField] private GameObject uiTilePrefab;
    [SerializeField] private Transform uiParent;

    public void AnimateToGoal(Sprite sprite, Vector3 worldPos, Transform goalTarget, System.Action onComplete,
        float delay = 0f)
    {
        GameObject uiTile = Instantiate(uiTilePrefab, uiParent);
        Image img = uiTile.GetComponent<Image>();
        img.sprite = sprite;

        if (Camera.main != null)
        {
            Vector3 startPos = Camera.main.WorldToScreenPoint(worldPos);
            uiTile.transform.position = startPos;

            RectTransform rect = uiTile.GetComponent<RectTransform>();
            Vector3 downPos = startPos + Vector3.down * 30f;

            Sequence seq = DOTween.Sequence();

            seq.AppendInterval(delay) 
                .Append(rect.DOMove(downPos, 0.15f).SetEase(Ease.OutQuad))
                .Append(rect.DOMove(goalTarget.position, 0.4f).SetEase(Ease.InOutQuad))
                .OnComplete(() =>
                {
                    Destroy(uiTile);
                    onComplete?.Invoke();
                });
        }
    }
}