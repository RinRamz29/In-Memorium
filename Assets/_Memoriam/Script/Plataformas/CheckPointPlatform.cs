using System;
using UnityEngine;

namespace _Memoriam.Script.Plataformas
{
    public class CheckPointPlatform : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<Player.Player>(out var player))
            {
                player.LastCheckPoint = transform.position;
            }
        }
    }
}