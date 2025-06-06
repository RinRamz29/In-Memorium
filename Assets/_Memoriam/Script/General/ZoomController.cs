using System.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Memoriam.Script.General
{
    public class ZoomController : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera cinemachineCamera;
        [SerializeField] private float zoomIn = 2;
        [SerializeField] private float zoomOut = 4;
        [SerializeField] private float zoomSpeed = 1.5f;
        [SerializeField] private bool shouldZoomIn = true;

        private bool _isZooming = false;

        private async void ChangeZoom(float target)
        {
            if (_isZooming) 
                return;
            
            _isZooming = true;

            var startSize = cinemachineCamera.Lens.OrthographicSize;
            float elapsedTime = 0;

            while (elapsedTime < zoomSpeed)
            {
                cinemachineCamera.Lens.OrthographicSize = Mathf.Lerp(startSize, target, elapsedTime / zoomSpeed);
                elapsedTime += Time.deltaTime;
                await Task.Yield();
            }

            cinemachineCamera.Lens.OrthographicSize = target;
            _isZooming = false;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.transform.TryGetComponent<Player.Player>(out var player))
            {
                if (shouldZoomIn)
                {
                    ChangeZoom(zoomIn);
                }
                else
                {
                    ChangeZoom(zoomOut);
                }
            }
        }
    }
}
