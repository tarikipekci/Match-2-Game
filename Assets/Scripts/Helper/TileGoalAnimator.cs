using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Helper
{
    public class TileGoalAnimator : MonoBehaviour
    {
        [SerializeField] private GameObject uiTilePrefab;
        [SerializeField] private Transform uiParent;

        public void AnimateToGoal(Sprite sprite, Tile boardTile, Transform goalTarget, System.Action onComplete,
            float delay = 0f)
        {
            GameObject uiTile = Instantiate(uiTilePrefab, uiParent);
            Image img = uiTile.GetComponent<Image>();
            img.sprite = sprite;

            RectTransform rect = uiTile.GetComponent<RectTransform>();
            RectTransform goalRect = goalTarget.GetComponent<RectTransform>();

            rect.localScale = boardTile.transform.lossyScale;

            if (Camera.main != null)
            {
                Vector3 startPos = Camera.main.WorldToScreenPoint(boardTile.transform.position);
                rect.position = startPos;

                Vector3 downPos = startPos + Vector3.down * 30f;

                Sequence seq = DOTween.Sequence();
                seq.AppendInterval(delay);

                seq.Append(rect.DOMove(downPos, 0.15f).SetEase(Ease.OutQuad));

                seq.Append(rect.DOMove(goalTarget.position, 0.4f).SetEase(Ease.InOutQuad));
                seq.Join(rect.DOScale(goalRect.localScale, 0.4f).SetEase(Ease.InOutQuad));

                seq.OnComplete(() =>
                {
                    Destroy(uiTile);
                    onComplete?.Invoke();
                });
            }
        }
    }
}