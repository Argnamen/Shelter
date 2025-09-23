using UnityEngine;

public class HeroView : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private SpriteRenderer _healthSprite;

    private float _oneHPSize;
    private float _oneHPImage;
    private void Start()
    {
        _animator.Play("Idle");
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
        if (isFighting)
        {
            _animator.Play("Attack");
        }
    }

    public void SetHealth(float health)
    {
        _oneHPSize = health;
    }

    public void UpdateHealth(int health)
    {
        if (_healthSprite == null)
            return;

        Debug.Log(health);

        _healthSprite.size = new Vector2(1 / _oneHPSize * health, _healthSprite.size.y);
    }

    public void Die()
    {
        _animator.Play("Die");
        // Уничтожаем объект через 1 секунду
        Destroy(gameObject, 1f);
    }
}