using System.Collections;
using System.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;

namespace _Memoriam.Script.General
{
    public class ZoomController : MonoBehaviour
    {
        public CinemachineCamera _camera;
        private float zoomIn = 2;
        private float zoomOut = 4;
        private float zoomSpeed = 1.5f;
         

        private Coroutine zoomCoroutine;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Zoom"))
            {
                if (zoomCoroutine != null)
                    StopCoroutine(zoomCoroutine);

               zoomCoroutine = StartCoroutine(ChangeZoom(zoomIn));
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Zoom"))
            {
                if (zoomCoroutine != null)
                    StopCoroutine(zoomCoroutine);

               zoomCoroutine = StartCoroutine(ChangeZoom(zoomOut));
            }
        }

        private IEnumerator ChangeZoom(float targetSize)
        {
            float startSize = _camera.Lens.OrthographicSize;
            float elapsedTime = 0f;

            while (elapsedTime < zoomSpeed)
            {
                _camera.Lens.OrthographicSize = Mathf.Lerp(startSize, targetSize, elapsedTime / zoomSpeed);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            _camera.Lens.OrthographicSize = targetSize;
        }

    }
}
