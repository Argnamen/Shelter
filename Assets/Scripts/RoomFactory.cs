using UnityEngine;
using Zenject;

public class RoomFactory
{
    private readonly DungeonView _dungeonView;
    private readonly MonsterFactory _monsterFactory;
    private readonly DiContainer _container;
    private readonly GridModel _gridModel;

    public RoomFactory(
        DungeonView dungeonView,
        MonsterFactory monsterFactory,
        DiContainer container,
        GridModel gridModel)
    {
        _dungeonView = dungeonView;
        _monsterFactory = monsterFactory;
        _container = container;
        _gridModel = gridModel;
    }

    public RoomModel CreateRoom(RoomType type, Vector2Int position)
    {
        var roomModel = new RoomModel(type, position);

        // Добавляем монстров в зависимости от типа комнаты
        AddMonstersToRoom(roomModel);

        // Создаем визуальное представление
        var worldPosition = _gridModel.GridToWorldPosition(position);
        var roomView = _dungeonView.CreateRoomView(type, worldPosition);

        if (roomView == null)
        {
            Debug.LogError($"Failed to create room view for type: {type}");
            return null;
        }

        // Создаем презентер
        _container.Instantiate<RoomPresenter>(new object[] { roomModel, roomView });

        Debug.Log($"Room created at {position} - {type}");
        return roomModel;
    }

    private void AddMonstersToRoom(RoomModel room)
    {
        switch (room.Type)
        {
            case RoomType.Combat:
                AddMonsters(room, MonsterType.Slime, Random.Range(2, 5));
                break;
            case RoomType.Rest:
                AddMonsters(room, MonsterType.Skeleton, Random.Range(1, 3));
                break;
            case RoomType.Treasure:
                AddMonsters(room, MonsterType.Goblin, Random.Range(1, 2));
                break;
            case RoomType.Stairs:
                // Лестницы без монстров
                break;
        }
    }

    private void AddMonsters(RoomModel room, MonsterType type, int count)
    {
        for (int i = 0; i < count; i++)
        {
            room.Monsters.Add(new MonsterModel(type, 30));
        }
    }
}