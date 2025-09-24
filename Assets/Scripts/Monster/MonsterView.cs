using UnityEngine;

public class MonsterView : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    private bool _isDie = false;

    public void Initialize(MonsterType type)
    {
        _animator.Play("Idle");
    }

    public void Fight()
    {
        if (_isDie) return;
        _animator.Play("Attack");
    }

    public void Idle()
    {
        if (_isDie) return;
        _animator.Play("Idle");
    }

    public void SetPause(bool pause)
    {
        if (pause)
        {
            _animator.StopPlayback();
        }
        else
        {
            _animator.StartPlayback();
        }
    }

    public void Die()
    {
        _isDie = true;
        _animator.Play("Die");

        // Автоматически уничтожаем через 1 секунду
        Destroy(gameObject, 1f);
    }
}