using UnityEngine;

public class HeroView : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    public void SetMoving(bool isMoving)
    {
        _animator.SetBool("IsMoving", isMoving);
    }

    public void SetFighting()
    {
        _animator.SetTrigger("Fight");
    }
}