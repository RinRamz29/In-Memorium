using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Memoriam.Script.Plataformas
{
    public class CheckPointPlatform : MonoBehaviour
    {
        [SerializeField] private Transform targetToTeleport;
        
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<Player.Player>(out var player))
            {
                player.transform.position = targetToTeleport.position;
                player.LastCheckPoint = targetToTeleport.position;
            }
        }
    }
}