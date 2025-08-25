using UnityEngine;

public class MonsterView : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    public void Initialize(MonsterType type)
    {
        // Настройка спрайта и анимаций
    }

    public void Die()
    {
        _animator.Play("Die");

        // Автоматически уничтожаем через 1 секунду
        Destroy(gameObject, 1f);
    }

    // Добавляем DisposableCollector если его нет
    private void Awake()
    {
        if (GetComponent<DisposableCollector>() == null)
        {
            gameObject.AddComponent<DisposableCollector>();
        }
    }
}