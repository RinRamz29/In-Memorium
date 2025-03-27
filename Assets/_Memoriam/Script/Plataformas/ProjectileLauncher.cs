using System;
using System.Collections.Generic;
using _Memoriam.Script.General;
using UnityEngine;

namespace _Memoriam.Script.Plataformas
{
    public class ProjectileLauncher : MonoBehaviour
    {
        [SerializeField] private string iDForPool;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float timeBetweenProjectiles;
        private float _lastLaunchTime;

        private void Update()
        {
            if (Time.time - _lastLaunchTime > timeBetweenProjectiles)
            {
                SpawnProjectile();
            }
        }

        private void SpawnProjectile()
        {
            if (projectilePrefab == null) 
                return;

            _lastLaunchTime = Time.time;
            ObjectPool.Instance.SpawnFromPool(iDForPool, transform.position, Quaternion.identity, true);
        }
    }
}