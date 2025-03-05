using DG.Tweening;
using UnityEngine;

public class TrianglePlatforms : MonoBehaviour, IPlatforms
{
    [SerializeField] private float _moveTime = 1f;
    [SerializeField] private Ease ease = Ease.InOutQuad;

    [SerializeField] private Vector2 pointA = Vector2.zero;
    [SerializeField] private Vector2 pointB;
    [SerializeField] private Vector2 pointC;

    private Vector2 _startPosition;

    private void Start()
    {
        _startPosition = transform.position;
        Move();
    }

    public void Move()
    {
         DOTween.Sequence()
            .Append(transform.DOMove(_startPosition + pointA, _moveTime).SetEase(ease))
            .Append(transform.DOMove(_startPosition + pointB, _moveTime).SetEase(ease))
            .Append(transform.DOMove(_startPosition, _moveTime).SetEase(ease))
            .SetLoops(-1, LoopType.Restart);
    }
}
