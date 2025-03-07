using System.Linq;
using UnityEngine;

namespace _Memoriam.Script.General
{
    public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T _instance;
        private static bool _isQuitting;
        
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    if (_isQuitting)
                    {
                        return null;
                    }
                    _instance = FindObjectsByType<T>(FindObjectsSortMode.None).FirstOrDefault();
                    
                    if (_instance == null)
                    {
                        var obj = new GameObject(typeof(T).Name);
                        _instance = obj.AddComponent<T>();
                    }
                }
                return _instance;
            }
        }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        protected virtual void OnApplicationQuit()
        {
            _isQuitting = true;
            _instance = null;
        }
    }

    public class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        public static T Instance { get; private set; }

        protected virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = this as T;
            }
            else
            {
                Destroy(gameObject);
            }
        }
    } 
}
