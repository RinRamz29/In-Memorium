using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace _Memoriam.Script.General
{
    public class ObjectPool : Singleton<ObjectPool>
    {
        #region structs y fields

        [Serializable]
        public class PoolObject
        {
            public GameObject prefab;
            public string id;
            public int size;
        }

        [field: SerializeField] public List<PoolObject> ObjectsPool { get; set; }
        private readonly Dictionary<string, Queue<GameObject>> _poolDictionary = new();
        private readonly Dictionary<string, List<GameObject>> _spawned = new();
        private Dictionary<string, int> _referenceCounters = new();

        #endregion

        #region creación de pools

        public async Task Initialize()
        {
            _poolDictionary.Clear();
            _spawned.Clear();
            _referenceCounters.Clear();
            
            foreach (var poolObj in ObjectsPool)
            {
                Queue<GameObject> queue = new();
                List<GameObject> prefabs = new();
                GameObject prefab = poolObj.prefab;

                if (prefab == null)
                {
                    Debug.LogWarning($"[ObjectPool] Prefab is null for ID: {poolObj.id}");
                    continue;
                }

                for (int i = 0; i < poolObj.size; i++)
                {
                    var obj = Instantiate(prefab);
                    obj.SetActive(false);
                    prefabs.Add(obj);
                    queue.Enqueue(obj);
                }

                _poolDictionary.Add(poolObj.id, queue);
                _spawned.Add(poolObj.id, prefabs);
                _referenceCounters[poolObj.id] = 0;
                await Task.Yield();
            }
        }


        #endregion


        #region spawn & return

        public GameObject GetReferenceFromPool(string id, int counter, Vector3 pos, Quaternion rot, bool setTransform)
        {
            if (_spawned.TryGetValue(id, out List<GameObject> prefabs))
            {
                int index = counter - 1;
                if (index >= 0 && index < prefabs.Count)
                {
                    GameObject obj = prefabs[index];

                    if (setTransform)
                    {
                        obj.transform.SetPositionAndRotation(pos, rot);
                    }

                    obj.SetActive(true); 
                    return obj;
                }
            }
            
            Debug.LogWarning($"[ObjectPool] No reference found for ID: {id}");
            return null;
        }


        public void ReturnToPool(string id, GameObject obj)
        {
            if (obj == null || !_poolDictionary.TryGetValue(id, out var q)) 
                return;
            
            obj.SetActive(false);
            q.Enqueue(obj);
        }

        #endregion


        #region utilidades
        public async Task ResetAllPools()
        {
            foreach (var list in _spawned.Values)
            {
                foreach (var obj in list)
                {
                    if (obj != null)
                        Destroy(obj);
                }
            }

            await Task.Yield(); // Allow Unity to process Destroy calls

            _poolDictionary.Clear();
            _spawned.Clear();           // You must clear BEFORE checking count
            _referenceCounters.Clear();

            Debug.Log("All pools reset.");
        }
        
        public int GetNextCounter(string id)
        {
            if (!_referenceCounters.ContainsKey(id))
                _referenceCounters[id] = 0;

            if (!_spawned.TryGetValue(id, out var list) || list.Count == 0)
            {
                Debug.LogWarning($"[ObjectPool] No spawned objects for ID: {id}");
                return 0;
            }

            int counter = _referenceCounters[id];
            _referenceCounters[id] = (counter + 1) % list.Count; // loop around
            return counter;
        }

        #endregion
    }
}