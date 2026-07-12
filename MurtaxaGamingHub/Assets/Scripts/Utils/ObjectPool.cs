// ============================================================
//  ObjectPool.cs
//  Place in: Assets/Scripts/Utils/
//  Generic object pool for projectiles, particles, damage text, etc.
//  Avoids runtime Instantiate/Destroy calls (key for Android perf).
// ============================================================
using System.Collections.Generic;
using UnityEngine;

namespace MurtaxaGaming.Utils
{
    [System.Serializable]
    public class Pool
    {
        public string tag;          // Identifier for this pool
        public GameObject prefab;   // Prefab to pool
        public int size;            // Initial pool size
    }

    public class ObjectPool : Singleton<ObjectPool>
    {
        [Header("Pool Definitions")]
        [Tooltip("Define each pool: tag, prefab, and initial size.")]
        public List<Pool> pools;

        // Dictionary: tag -> queue of GameObjects
        private Dictionary<string, Queue<GameObject>> _poolDictionary;

        protected override void Awake()
        {
            base.Awake();
            InitializePools();
        }

        /// <summary>Creates all pools at startup.</summary>
        private void InitializePools()
        {
            _poolDictionary = new Dictionary<string, Queue<GameObject>>();

            foreach (Pool pool in pools)
            {
                Queue<GameObject> objectQueue = new Queue<GameObject>();

                for (int i = 0; i < pool.size; i++)
                {
                    GameObject obj = Instantiate(pool.prefab);
                    obj.SetActive(false);
                    obj.transform.SetParent(transform); // keep hierarchy clean
                    objectQueue.Enqueue(obj);
                }

                _poolDictionary[pool.tag] = objectQueue;
                Debug.Log($"[ObjectPool] Initialized pool '{pool.tag}' with {pool.size} objects.");
            }
        }

        /// <summary>
        /// Retrieve a pooled object by tag, position, and rotation.
        /// If the pool is exhausted, a new object is created and logged.
        /// </summary>
        public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
        {
            if (!_poolDictionary.ContainsKey(tag))
            {
                Debug.LogError($"[ObjectPool] Pool with tag '{tag}' not found!");
                return null;
            }

            Queue<GameObject> queue = _poolDictionary[tag];

            // Expand pool if empty
            if (queue.Count == 0)
            {
                Debug.LogWarning($"[ObjectPool] Pool '{tag}' exhausted. Expanding.");
                Pool pool = pools.Find(p => p.tag == tag);
                if (pool == null) return null;
                GameObject newObj = Instantiate(pool.prefab);
                newObj.transform.SetParent(transform);
                queue.Enqueue(newObj);
            }

            GameObject objectToSpawn = queue.Dequeue();
            objectToSpawn.SetActive(true);
            objectToSpawn.transform.SetPositionAndRotation(position, rotation);

            // Auto-return support: implement IPooledObject on prefab
            IPooledObject pooledObj = objectToSpawn.GetComponent<IPooledObject>();
            pooledObj?.OnObjectSpawn();

            return objectToSpawn;
        }

        /// <summary>Returns an object back to its pool.</summary>
        public void ReturnToPool(string tag, GameObject obj)
        {
            if (!_poolDictionary.ContainsKey(tag))
            {
                Debug.LogError($"[ObjectPool] Cannot return: pool '{tag}' not found.");
                Destroy(obj);
                return;
            }

            obj.SetActive(false);
            _poolDictionary[tag].Enqueue(obj);
        }
    }

    /// <summary>Implement on pooled prefabs to receive spawn callbacks.</summary>
    public interface IPooledObject
    {
        void OnObjectSpawn();
    }
}
