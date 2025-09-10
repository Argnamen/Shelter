using UnityEngine;

public class HeroView : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    private void Start()
    {
        if (_animator != null)
        {
            _animator.Play("Idle");
        }
        
    }
    public void SetMoving(bool isMoving)
    {
        if (_animator != null)
        {
            if (isMoving)
            {
                _animator.Play("Run");
            }
            else
            {
                _animator.Play("Idle");
            }
        }
    }

    public void SetFighting(bool isFighting = true)
    {
        if (_animator != null)
        {
            if (isFighting)
            {
                _animator.Play("Attack");
            }
        }
    }

    public void Die()
    {
        if (_animator != null)
        {
            _animator.Play("Die");
        }

        // Уничтожаем объект через 1 секунду
        Destroy(gameObject, 1f);
    }
}