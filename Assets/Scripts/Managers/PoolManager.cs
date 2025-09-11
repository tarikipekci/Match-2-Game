using System.Collections.Generic;
using UnityEngine;
using Interfaces;

namespace Managers
{
    [System.Serializable]
    public class Pool
    {
        public GameObject prefab;
        public int size;
    }

    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Instance { get; private set; }
        [SerializeField] private List<Pool> pools;
        private Dictionary<string, Queue<GameObject>> poolDictionary;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            InitializePools();
        }

        private void InitializePools()
        {
            poolDictionary = new Dictionary<string, Queue<GameObject>>();
            foreach (Pool pool in pools)
            {
                Queue<GameObject> queue = new Queue<GameObject>();
                for (int i = 0; i < pool.size; i++)
                {
                    GameObject obj = Instantiate(pool.prefab);
                    obj.name = pool.prefab.name;
                    obj.SetActive(false);
                    queue.Enqueue(obj);
                }

                poolDictionary.Add(pool.prefab.name, queue);
            }
        }

        public GameObject SpawnFromPool(string prefabName, Transform parent)
        {
            if (!poolDictionary.TryGetValue(prefabName, out var queue))
            {
                Debug.LogWarning($"Pool for {prefabName} doesn't exist! Creating new pool entry.");
                var prefab = pools.Find(p => p.prefab.name == prefabName)?.prefab;
                if (prefab == null)
                {
                    Debug.LogError($"Prefab {prefabName} not found!");
                    return null;
                }

                queue = new Queue<GameObject>();
                poolDictionary[prefabName] = queue;
            }

            GameObject obj = null;

            if (queue.Count > 0)
            {
                obj = queue.Dequeue();

                if (obj.activeInHierarchy)
                {
                    var prefab = pools.Find(p => p.prefab.name == prefabName)?.prefab;
                    if (prefab != null)
                    {
                        obj = Instantiate(prefab);
                        obj.name = prefabName;
                    }
                }
            }
            else
            {
                var prefab = pools.Find(p => p.prefab.name == prefabName)?.prefab;
                if (prefab != null)
                {
                    obj = Instantiate(prefab);
                    obj.name = prefabName;
                }
            }

            if (obj != null && obj.TryGetComponent<IPoolable>(out var poolable))
                poolable.OnSpawn();
            else if (obj != null) obj.SetActive(true);

            if (obj != null)
            {
                obj.transform.SetParent(parent);
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.identity;
                obj.transform.localScale = Vector3.one;
            }

            return obj;
        }

        public void ReturnToPool(GameObject obj)
        {
            if (obj == null) return;

            if (obj.TryGetComponent<IPoolable>(out var poolable))
            {
                poolable.OnDespawn();
            }
            else
            {
                obj.SetActive(false);
            }

            obj.transform.SetParent(transform);
            obj.transform.localPosition = Vector3.zero;

            string key = obj.name.Replace("(Clone)", "").Trim();

            if (!poolDictionary.ContainsKey(key))
                poolDictionary[key] = new Queue<GameObject>();

            poolDictionary[key].Enqueue(obj);
        }
    }
}