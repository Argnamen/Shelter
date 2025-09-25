using UnityEngine;

public class RoomView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Transform[] _monstersContainer;
    [SerializeField] private Transform[] _heroesContainers;
    public Vector2Int Position { get; private set; }
    public RoomType Type { get; private set; }

    public void Initialize(Vector2Int position, RoomType type, Faction faction)
    {
        Position = position;
        Type = type;
        name = $"{faction}_{type}Room_{position.x}_{position.y}";

        UpdateVisuals(faction);
    }

    private void UpdateVisuals(Faction faction)
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = GetRoomColor(Type, faction);
        }
    }

    private Color GetRoomColor(RoomType type, Faction faction)
    {
        Color baseColor = type switch
        {
            RoomType.Combat => new Color(1, 0.5f, 0.5f, 1),
            RoomType.Rest => new Color(0.5f, 0.5f, 1, 1),
            RoomType.Treasure => new Color(1, 1, 0.5f, 1),
            RoomType.Stairs => new Color(0.5f, 1, 0.5f, 1),
            _ => Color.white
        };

        // Добавляем оттенок фракции
        return faction switch
        {
            Faction.Player => baseColor,
            Faction.Enemy1 => Color.Lerp(baseColor, Color.red, 0.3f),
            Faction.Enemy2 => Color.Lerp(baseColor, Color.blue, 0.3f),
            Faction.Enemy3 => Color.Lerp(baseColor, Color.green, 0.3f),
            _ => baseColor
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