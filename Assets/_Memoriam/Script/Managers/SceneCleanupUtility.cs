using _Memoriam.Script.Enemies;
using _Memoriam.Script.Powerups;
using UnityEngine;

namespace _Memoriam.Script.Managers
{
    public static class SceneCleanupUtility
    {
        public static void CleanupScene()
        {
            foreach (var player in Object.FindObjectsByType<Player.Player>(FindObjectsInactive.Include,
                         FindObjectsSortMode.None))
            {
                Object.Destroy(player.gameObject);
            }
            
            foreach (var enemy in Object.FindObjectsByType<BaseEnemy>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Object.Destroy(enemy.gameObject);
            }

            foreach (var projectile in GameObject.FindGameObjectsWithTag("Projectile"))
            {
                Object.Destroy(projectile);
            }
        }
    }

}