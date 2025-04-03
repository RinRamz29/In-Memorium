using System.Collections;
using _Memoriam.Script.General;
using UnityEngine;

namespace _Memoriam.Script.Plataformas
{
    public class ProjectileLauncher : MonoBehaviour
    {
        [SerializeField] private string projectilePoolId;
        [SerializeField] private float fireRate = 1f;
        [SerializeField] private Transform firePoint;
        [SerializeField] private bool autoFire = false;
        [SerializeField] private Vector2 fireDirection = Vector2.right;
        
        private float _lastFireTime;
        
        private void Start()
        {
            if (autoFire)
            {
                StartCoroutine(AutoFireRoutine());
            }
        }
        
        public void FireProjectile(Vector2 direction)
        {
            if (Time.time - _lastFireTime < 1f / fireRate) return;
            
            GameObject projectileObj = ObjectPool.Instance.SpawnFromPool(projectilePoolId, firePoint.position, Quaternion.identity, true);
            if (projectileObj.TryGetComponent<Projectile>(out var projectile))
            {
                projectile.Direction = direction.normalized;
            }
            _lastFireTime = Time.time;
        }
        
        private IEnumerator AutoFireRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f / fireRate);
                FireProjectile(fireDirection);
            }
        }
    }
}
