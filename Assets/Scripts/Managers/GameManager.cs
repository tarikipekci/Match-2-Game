using System.Collections;
using Data;
using DG.Tweening;
using UnityEngine;

namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public LevelData currentLevelData;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        
            DOTween.SetTweensCapacity(500, 500);
            DOTween.Init();
        }

        void Start()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;

            StartCoroutine(DOTweenWarmup());
        }

        private IEnumerator DOTweenWarmup()
        {
            float dummy = 0f;
            DOTween.To(() => dummy, x => dummy = x, 1f, 0.01f);
            yield return null;
        }
    }
}
