using UnityEngine;

public class RoomView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Transform[] _monstersContainer;
    [SerializeField] private Transform[] _heroesContainers;
    public Vector2Int Position { get; private set; }
    public RoomType Type { get; private set; }

    public void Initialize(Vector2Int position, RoomType type)
    {
        Position = position;
        Type = type;
        name = $"{type}Room_{position.x}_{position.y}";

        // Настраиваем внешний вид в зависимости от типа
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = GetRoomColor(Type);
        }
    }

    private Color GetRoomColor(RoomType type)
    {
        return type switch
        {
            RoomType.Combat => new Color(1, 0.5f, 0.5f, 1), // Красноватый
            RoomType.Rest => new Color(0.5f, 0.5f, 1, 1),   // Синеватый
            RoomType.Treasure => new Color(1, 1, 0.5f, 1),  // Желтоватый
            RoomType.Stairs => new Color(0.5f, 1, 0.5f, 1), // Зеленоватый
            _ => Color.white
        };
    }

    public void AddMonsterView(MonsterView monsterView)
    {
        if(monsterView == null)
            return;

        foreach (var container in _monstersContainer)
        {
            if (container.childCount == 0)
            {
                monsterView.transform.SetParent(container);
                monsterView.transform.localPosition = Vector3.zero;
                break;
            }
        }
    }

    public void AddHeroView(HeroView heroView)
    {
        if (heroView == null)
            return;

        for (int i = 0; i < _heroesContainers.Length; i++)
        {
            if (_heroesContainers[i].childCount == 0)
            {
                heroView.transform.SetParent(_heroesContainers[i]);
            }
        }
    }

    public void DestroyRoom()
    {
        Destroy(gameObject);
    }
}