using UnityEngine;

public class HeroView : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    public void SetMoving(bool isMoving)
    {
        if (_animator != null)
        {
            _animator.SetBool("IsMoving", isMoving);
        }
    }

    public void SetFighting(bool isFighting = true)
    {
        if (_animator != null)
        {
            if (isFighting)
            {
                _animator.SetTrigger("Fight");
            }
        }
    }

    public void Die()
    {
        if (_animator != null)
        {
            _animator.SetTrigger("Die");
        }

        // Уничтожаем объект через 1 секунду
        Destroy(gameObject, 1f);
    }
}