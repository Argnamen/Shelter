using UnityEngine;
using System.Collections.Generic;

public class DungeonView : MonoBehaviour
{
    [Header("Containers")]
    [SerializeField] private Transform _roomsContainer;
    [SerializeField] private Transform _heroesContainer;
    [SerializeField] private Transform _monstersContainer;

    [Header("References")]
    [SerializeField] private Transform _entryPoint; // Точка входа героев
    [SerializeField] private Transform _exitPoint;  // Точка выхода героев

    private Dictionary<RoomModel, RoomView> _roomViews = new();
    private Dictionary<HeroModel, HeroView> _heroViews = new();
    private Dictionary<MonsterModel, MonsterView> _monsterViews = new();

    public Vector3 EntryPosition => _entryPoint.position;
    public Vector3 ExitPosition => _exitPoint.position;

    private void Awake()
    {
        // Автоматически находим контейнеры если не установлены
        if (_roomsContainer == null)
            _roomsContainer = transform.Find("RoomsContainer");
        if (_heroesContainer == null)
            _heroesContainer = transform.Find("HeroesContainer");
        if (_monstersContainer == null)
            _monstersContainer = transform.Find("MonstersContainer");
    }

    // Создание комнаты
    public RoomView CreateRoomView(RoomModel roomModel, Vector3 position)
    {
        var prefabPath = $"Rooms/{roomModel.Type}Room";
        var prefab = Resources.Load<RoomView>(prefabPath);

        if (prefab == null)
        {
            Debug.LogError($"Room prefab not found: {prefabPath}");
            return null;
        }

        var roomView = Instantiate(prefab, _roomsContainer);
        roomView.transform.position = new Vector2(position.x, position.y);
        roomView.name = $"{roomModel.Type}Room_{roomModel.Position.x}_{roomModel.Position.y}";

        _roomViews[roomModel] = roomView;
        return roomView;
    }

    // Создание героя
    public HeroView CreateHeroView(HeroModel heroModel)
    {
        var prefab = Resources.Load<HeroView>("Heroes/Hero");
        if (prefab == null)
        {
            Debug.LogError("Hero prefab not found!");
            return null;
        }

        var heroView = Instantiate(prefab, _heroesContainer);
        heroView.transform.position = _entryPoint.position;
        heroView.name = $"Hero_{System.Guid.NewGuid()}";

        _heroViews[heroModel] = heroView;
        return heroView;
    }

    // Создание монстра в комнате
    public MonsterView CreateMonsterView(MonsterModel monsterModel, RoomView roomView)
    {
        var prefabPath = $"Monsters/{monsterModel.Type}Monster";
        var prefab = Resources.Load<MonsterView>(prefabPath);

        if (prefab == null)
        {
            Debug.LogError($"Monster prefab not found: {prefabPath}");
            return null;
        }

        var monsterView = Instantiate(prefab, roomView.MonstersContainer);
        monsterView.name = $"{monsterModel.Type}Monster";

        // Случайная позиция внутри комнаты
        var randomOffset = new Vector3(
            Random.Range(-0.5f, 0.5f),
            Random.Range(-0.5f, 0.5f),
            0
        );
        monsterView.transform.localPosition = randomOffset;

        _monsterViews[monsterModel] = monsterView;
        return monsterView;
    }

    // Удаление героя
    public void RemoveHeroView(HeroModel heroModel)
    {
        if (_heroViews.TryGetValue(heroModel, out var heroView))
        {
            Destroy(heroView.gameObject);
            _heroViews.Remove(heroModel);
        }
    }

    // Удаление монстра
    public void RemoveMonsterView(MonsterModel monsterModel)
    {
        if (_monsterViews.TryGetValue(monsterModel, out var monsterView))
        {
            Destroy(monsterView.gameObject);
            _monsterViews.Remove(monsterModel);
        }
    }

    // Получение View по Model
    public RoomView GetRoomView(RoomModel roomModel) => _roomViews.GetValueOrDefault(roomModel);
    public HeroView GetHeroView(HeroModel heroModel) => _heroViews.GetValueOrDefault(heroModel);
    public MonsterView GetMonsterView(MonsterModel monsterModel) => _monsterViews.GetValueOrDefault(monsterModel);

    // Очистка всей сцены
    public void ClearDungeon()
    {
        foreach (var roomView in _roomViews.Values)
            Destroy(roomView.gameObject);
        foreach (var heroView in _heroViews.Values)
            Destroy(heroView.gameObject);
        foreach (var monsterView in _monsterViews.Values)
            Destroy(monsterView.gameObject);

        _roomViews.Clear();
        _heroViews.Clear();
        _monsterViews.Clear();
    }

    // Для отладки
    private void OnDrawGizmos()
    {
        if (_entryPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(_entryPoint.position, 0.3f);
            Gizmos.DrawLine(_entryPoint.position, _entryPoint.position + Vector3.up * 1f);
        }

        if (_exitPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_exitPoint.position, 0.3f);
            Gizmos.DrawLine(_exitPoint.position, _exitPoint.position + Vector3.down * 1f);
        }
    }
}