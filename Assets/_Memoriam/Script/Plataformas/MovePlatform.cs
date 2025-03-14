using _Memoriam.Script.Managers;
using DG.Tweening;
using UnityEngine;

public class MovePlatform : MonoBehaviour, IPlatforms
{
    [SerializeField] private Vector2 _moveTo = Vector2.zero;
    [SerializeField] private float _moveTime = 1f;
    [SerializeField] private Ease ease = Ease.InOutQuad;

    private Vector2 _startPosition;

    private void Start()
    {
        _startPosition = transform.position;
        Move();
    }

    public void Move()
    {
        if (GameStateManager.Instance.GameCurrentState != GameStateManager.GameState.OnGameplay)
            return;
        
        transform.DOMove(_startPosition + _moveTo, _moveTime)
            .SetEase(ease)
            .SetLoops(-1, LoopType.Yoyo);
    }

   
}
