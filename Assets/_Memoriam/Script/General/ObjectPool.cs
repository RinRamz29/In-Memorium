using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Memoriam.Script.General
{
    public class ObjectPool : Singleton<ObjectPool>
    {
        [Serializable]
        public class PoolObject
        {
            public GameObject prefab;
            public string id;
            public int size;
        }

        [field: SerializeField] public List<PoolObject> ObjectsPool { get; set; }
        private Dictionary<string, Queue<GameObject>> _poolDictionary = new();

        protected override void Awake()
        {
            base.Awake();
            foreach (var objc in ObjectsPool)
            {
                var objectPool = new Queue<GameObject>();

                for (int i = 0; i < objc.size; i++)
                {
                    var obj = Instantiate(objc.prefab);
                    obj.SetActive(false);
                    objectPool.Enqueue(obj);
                }

                Debug.Log(objc.id);
                _poolDictionary.Add(objc.id, objectPool);
                
            }
        }

        public GameObject SpawnFromPool(string id, Vector3 position, Quaternion rotation)
        {
            if (!_poolDictionary.TryGetValue(id, out var objectPool))
            {
                Debug.LogError($"No pool found for {id}");
                return null;
            }

            if (objectPool.Count == 0)
            {
                Debug.LogError($"No pool found for {id}");
                return null;
            }

            var objectToSpawn = objectPool.Dequeue();
        
            objectToSpawn.SetActive(true);
            objectToSpawn.transform.position = position;
            objectToSpawn.transform.rotation = rotation;

            return objectToSpawn;
        }

        public void ReturnToPool(string id, GameObject objectToReturn)
        {
            if (!_poolDictionary.TryGetValue(id, out var objectPool))
                return;

            objectPool.Enqueue(objectToReturn);
            objectToReturn.SetActive(false);
        }
    }
}