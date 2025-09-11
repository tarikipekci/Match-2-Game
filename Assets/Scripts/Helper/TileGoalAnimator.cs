using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Helper
{
    public class TileGoalAnimator : MonoBehaviour
    {
        [SerializeField] private GameObject uiTilePrefab;
        [SerializeField] private GameObject goalParticleEffect;
        [SerializeField] private Transform uiParent;

        public void AnimateToGoal(Sprite sprite, Vector3 worldStartPosition, Transform goalTarget,
            System.Action onComplete,
            float delay = 0f)
        {
            GameObject uiTile = Instantiate(uiTilePrefab, uiParent, false);
            Image img = uiTile.GetComponent<Image>();
            img.sprite = sprite;

            RectTransform rect = uiTile.GetComponent<RectTransform>();
            RectTransform goalRect = goalTarget.GetComponent<RectTransform>();
            RectTransform parentRect = uiParent as RectTransform;

            Canvas canvas = uiParent.GetComponentInParent<Canvas>();
            Camera cam = canvas.renderMode == RenderMode.ScreenSpaceCamera ? canvas.worldCamera : null;

            Vector3 startScreen = Camera.main!.WorldToScreenPoint(worldStartPosition);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, startScreen, cam,
                out Vector2 startLocal);
            rect.anchoredPosition = startLocal;
            rect.localScale = Vector3.one;

            Vector3 goalScreen = Camera.main.WorldToScreenPoint(goalTarget.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, goalScreen, cam, out Vector2 goalLocal);

            Vector2 downLocal = startLocal + Vector2.down * 30f;

            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(delay);
            seq.Append(rect.DOAnchorPos(downLocal, .15f).SetEase(Ease.OutQuad));
            seq.Append(rect.DOAnchorPos(goalLocal, .4f).SetEase(Ease.InOutQuad));
            seq.Join(rect.DOScale(goalRect.localScale, .4f).SetEase(Ease.InOutQuad));

            seq.OnComplete(() =>
            {
                Destroy(uiTile);
                onComplete?.Invoke();
            });
        }

        public void SpawnGoalParticle(Transform goalTarget)
        {
            if (goalParticleEffect == null || goalTarget == null) return;

            GameObject fx = Instantiate(goalParticleEffect, goalTarget, false);
            RectTransform fxRect = fx.GetComponent<RectTransform>();
            if (fxRect != null)
                fxRect.anchoredPosition = Vector2.zero;

            ParticleSystem ps = fx.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
                Destroy(fx, ps.main.duration + ps.main.startLifetime.constantMax);
            }
            else
            {
                Destroy(fx, 2f);
            }
        }
    }
}