using UnityEngine;

public class MonsterView : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    public void Initialize(MonsterType type)
    {
        // Setup sprite and animations based on type
    }

    public void Die()
    {
        _animator.SetTrigger("Die");
        Destroy(gameObject, 1f);
    }
}