using System.Linq;
using _Memoriam.Script.General;
using _Memoriam.Script.Player;
using _Memoriam.Script.Tutorial;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

namespace _Memoriam.Script.Managers
{
    public class PlayerSpawner : Singleton<PlayerSpawner>
    {
        [field: SerializeField] public GameObject PlayerSpawnPoint {get; set; }
        [field: SerializeField] public GameplayMenuManager GameplayMenuManager {get; set; }

        public GameObject SpawnPlayer(bool newGame)
        {
            var player = ObjectPool.Instance.GetReferenceFromPool("Player", 1, PlayerSpawnPoint.transform.position, PlayerSpawnPoint.transform.rotation, newGame); 

            var script = player?.GetComponent<Player.Player>();
            SetState(newGame, script);
            script?.ResetPlayer();
            GameplayMenuManager.Player = script;

            return player;
        }

        private void SetState(bool newGame, Player.Player player)
        {
            if (newGame && !Loader.Instance.SetTutorial)
            {
                player.ForceCombat = true;
            }
            else
            {
                player.ForceCombat = false;
            }
        }
    }
}