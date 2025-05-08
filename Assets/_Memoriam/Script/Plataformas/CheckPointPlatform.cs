using System;
using System.Collections;
using _Memoriam.Script.InputLogic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Memoriam.Script.Plataformas
{
    public class CheckPointPlatform : MonoBehaviour
    {
        [SerializeField] private Transform targetToTeleport;
        [SerializeField] private CanvasGroup fadeCanvasGroup; // Assign a full screen UI CanvasGroup for fade
        [SerializeField] private float fadeDuration = 1f;

        private Player.Player _player;
        private bool _playerInRange;
        private bool _isTeleporting;
        private bool _interactPressed;

        private void OnInteractPressed(InputAction.CallbackContext ctx)
        {
            if (ctx.performed && _playerInRange) 
                _interactPressed = true; 
        } 
        

        private void OnEnable()
        {
            InputReader.Instance.PlayerActions.Player.Interact.performed += OnInteractPressed;
        }

        private void Update()
        {
            if (_playerInRange && _interactPressed && !_isTeleporting)
                StartCoroutine(TeleportPlayerWithFade());
        }

        private void OnDisable()
        {
            InputReader.Instance.PlayerActions.Player.Interact.performed -= OnInteractPressed;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<Player.Player>(out var player))
            {
                _player = player;
                _playerInRange = true;
                // Show UI prompt "Press E to teleport"
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.TryGetComponent<Player.Player>(out var player) && player == _player)
            {
                _playerInRange = false;
                _player = null;
                // Hide UI prompt
            }
        }

        private IEnumerator TeleportPlayerWithFade()
        {
            _isTeleporting = true;

            yield return StartCoroutine(Fade(1f));

            _player.transform.position = targetToTeleport.position;
            _player.LastCheckPoint = targetToTeleport.position;

            yield return StartCoroutine(Fade(0f));

            _isTeleporting = false;
        }

        private IEnumerator Fade(float targetAlpha)
        {
            if (fadeCanvasGroup == null) 
                yield break;

            var startAlpha = fadeCanvasGroup.alpha;
            var elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
                yield return null;
            }

            fadeCanvasGroup.alpha = targetAlpha;
        }
    }
}