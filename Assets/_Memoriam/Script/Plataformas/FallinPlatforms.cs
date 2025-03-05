using DG.Tweening;
using UnityEngine;
using System.Threading.Tasks;

public class FallinPlatforms : MonoBehaviour, IPlatforms
{
    [SerializeField] private float _moveTime = 1f;
    [SerializeField] private Ease ease = Ease.InOutQuad;

    [SerializeField] private Vector2 pointA = Vector2.zero;

    [SerializeField] private float waitTime = 1.5f;

    private Vector2 _startPosition;  

    void Start()
    {
        _startPosition = transform.position;
    }

    private async void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            await PlatformFall();
        }
    }

    private async Task PlatformFall()
    {
        await Task.Delay(3000);
        Move();
    }

    public void Move()
    {
        DOTween.Sequence()
            .Append(transform.DOMove(_startPosition + pointA, _moveTime).SetEase(ease))
            .AppendInterval(waitTime)
            .Append(transform.DOMove(_startPosition, _moveTime).SetEase(ease));
    }
}
