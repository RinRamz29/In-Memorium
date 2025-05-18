using System.Threading.Tasks;
using _Memoriam.Script.Enemies;
using _Memoriam.Script.General;
using _Memoriam.Script.Plataformas;
using _Memoriam.Script.Player;
using _Memoriam.Script.Powerups;
using UnityEngine;

namespace _Memoriam.Script.Managers
{
    public class SceneCleanupUtility : Singleton<SceneCleanupUtility>
    {
        public async Task CleanupScene()
        {
            Debug.Log("Scene cleanup");

            var players = FindObjectsByType<Player.Player>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Debug.Log("Cleaning Players: " + players.Length);
            foreach (var player in players)
            {
                player.StateMachine.ForceTerminate();
                Destroy(player.gameObject);
            }

            var enemies = FindObjectsByType<BaseEnemy>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Debug.Log("Cleaning Enemies: " + enemies.Length);
            foreach (var enemy in enemies)
                Destroy(enemy.gameObject);

            var projectiles = FindObjectsByType<Projectile>(FindObjectsInactive.Include, FindObjectsSortMode.None);;
            foreach (var projectile in projectiles)
                Destroy(projectile.gameObject);

            await Task.Yield();

            while (FindObjectsByType<Player.Player>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length > 0 ||
                   FindObjectsByType<BaseEnemy>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length > 0 ||
                   FindObjectsByType<Projectile>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length > 0)
            {
                await Task.Yield();
            }

            Debug.Log("Scene cleanup complete");
        }

    }

}