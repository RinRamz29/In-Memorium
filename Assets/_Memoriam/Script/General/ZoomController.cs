using System.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;

namespace _Memoriam.Script.General
{
    public class ZoomController : MonoBehaviour
    {
        private CinemachineCamera _camera;
        private float zoomIn = 2;
        private float zoomOut = 4;
        private float zoomSpeed = 1.5f;

        private bool _IsZooming = false;

        private void Awake()
        {
            _camera = GetComponent<CinemachineCamera>();
        }

        private async void ChangeZoom(float target)
        {
            if (_IsZooming) return;
            _IsZooming = true;

            float startSize = _camera.Lens.OrthographicSize;
            float elapsedTime = 0;

            while (elapsedTime < zoomSpeed)
            {
                _camera.Lens.OrthographicSize = Mathf.Lerp(startSize, target, elapsedTime / zoomSpeed);
                elapsedTime += Time.deltaTime;
                await Task.Yield();
            }

            _camera.Lens.OrthographicSize = target;
            _IsZooming = false;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.transform.TryGetComponent<Player.Player>(out var player))
            {
                ChangeZoom(zoomIn);
            }
        }
        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.transform.TryGetComponent<Player.Player>(out var player))
            {
                ChangeZoom(zoomOut);
            }
        }

    }
}
