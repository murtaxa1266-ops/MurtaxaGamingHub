// ============================================================
//  Singleton.cs
//  Place in: Assets/Scripts/Utils/
//  Generic singleton base class for manager objects.
// ============================================================
using UnityEngine;

namespace MurtaxaGaming.Utils
{
    /// <summary>
    /// Generic persistent singleton. Derive from this for any manager
    /// that must survive scene loads (GameManager, AudioManager, etc.).
    /// </summary>
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static readonly object _lock = new object();
        private static bool _applicationIsQuitting = false;

        public static T Instance
        {
            get
            {
                if (_applicationIsQuitting)
                {
                    Debug.LogWarning($"[Singleton] Instance '{typeof(T)}' already destroyed on application quit. Returning null.");
                    return null;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = (T)FindObjectOfType(typeof(T));

                        if (FindObjectsOfType(typeof(T)).Length > 1)
                        {
                            Debug.LogError($"[Singleton] Multiple instances of {typeof(T)} found. Check your scene setup.");
                            return _instance;
                        }

                        if (_instance == null)
                        {
                            GameObject singletonObject = new GameObject();
                            _instance = singletonObject.AddComponent<T>();
                            singletonObject.name = $"[Singleton] {typeof(T)}";
                            DontDestroyOnLoad(singletonObject);
                            Debug.Log($"[Singleton] Created new instance of {typeof(T)}.");
                        }
                    }

                    return _instance;
                }
            }
        }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Debug.LogWarning($"[Singleton] Duplicate instance of {typeof(T)} found. Destroying duplicate.");
                Destroy(gameObject);
            }
        }

        private void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
        }
    }
}
