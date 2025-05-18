using System;
using System.Collections;
using _Memoriam.Script.Audio;
using _Memoriam.Script.InputLogic;
using _Memoriam.Script.Managers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Memoriam.Script.Plataformas
{
    public class CheckPointPlatform : MonoBehaviour
    {
        [SerializeField] private Transform targetToTeleport;
        [SerializeField] private CanvasGroup fadeCanvasGroup; 
        [SerializeField] private float fadeDuration = 1f;
        [SerializeField] private ParticleSystem tpParticles;
        [SerializeField] private ParticleSystem tpParticlesArrived;

        private Player.Player _player;
        private bool _playerInRange;
        private bool _isTeleporting;
        private bool _interactPressed;

        private void OnInteractPressed(InputAction.CallbackContext ctx)
        {
            if (ctx.performed && _playerInRange)
            {
                tpParticles.Play();
                _interactPressed = true; 
            } 
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
                AudioManager.Instance.PlayOneShotSFX("PlayerTeleport");
                Player.Player.onPlayerFirstTp?.Invoke(true);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.TryGetComponent<Player.Player>(out var player) && player == _player)
            {
                _playerInRange = false;
                _player = null;
            }
        }

        private IEnumerator TeleportPlayerWithFade()
        {
            _isTeleporting = true;

            yield return StartCoroutine(Fade(1f));

            _player.transform.position = targetToTeleport.position;
            _player.LastCheckPoint = targetToTeleport.position;
            PlayerSpawner.Instance.PlayerSpawnPoint.transform.position = targetToTeleport.position;
            tpParticlesArrived.transform.parent = targetToTeleport;
            tpParticlesArrived.transform.localPosition = new Vector3(0f, -2f, 0f);
            tpParticlesArrived.Play();

            yield return StartCoroutine(Fade(0f));

            tpParticlesArrived.Stop();
            tpParticles.Stop();
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