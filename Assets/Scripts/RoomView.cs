using UnityEngine;

public class RoomView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Transform _monstersContainer;
    [SerializeField] private Transform _heroesContainer;

    public Transform MonstersContainer => _monstersContainer;
    public Transform HeroesContainer => _heroesContainer;

    public Vector2Int Position { get; private set; }
    public RoomType Type { get; private set; }

    public void Initialize(Vector2Int position, RoomType type)
    {
        Position = position;
        Type = type;
    }

    public void AddMonsterView(MonsterView monsterView)
    {
        monsterView.transform.SetParent(_monstersContainer);
    }

    public void AddHeroView(HeroView heroView)
    {
        heroView.transform.SetParent(_heroesContainer);
    }
}
