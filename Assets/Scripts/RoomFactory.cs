using UnityEngine;
using Zenject;

public class RoomFactory
{
    private readonly DungeonView _dungeonView;
    private readonly DiContainer _container;
    private readonly GridModel _gridModel;
    private readonly GameData _gameData;
    private readonly MonsterSpawner _monsterSpawner;

    public RoomFactory(
        DungeonView dungeonView,
        DiContainer container,
        GridModel gridModel,
        GameData gameData,
        MonsterSpawner monsterSpawner)
    {
        _dungeonView = dungeonView;
        _container = container;
        _gridModel = gridModel;
        _gameData = gameData;
        _monsterSpawner = monsterSpawner;
    }

    public RoomModel CreateRoom(RoomType type, Vector2Int position, MonsterType monsterType = MonsterType.None)
    {
        var roomModel = new RoomModel(type, monsterType, position);

        // Создаем визуальное представление
        var worldPosition = _gridModel.GridToWorldPosition(position);
        var roomView = _dungeonView.CreateRoomView(type, worldPosition);

        // Добавляем монстров в зависимости от типа комнаты
        AddMonstersToRoom(roomModel, roomView);

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

    private void AddMonstersToRoom(RoomModel room, RoomView roomView)
    {
        switch (room.Type)
        {
            case RoomType.Combat:
                AddMonsters(room, roomView, room.Monster, 2);
                break;
            case RoomType.Rest:
                
                break;
            case RoomType.Treasure:
                
                break;
            case RoomType.Stairs:
                // Лестницы без монстров
                break;
        }
    }

    private void AddMonsters(RoomModel room, RoomView roomView, MonsterType type, int count)
    {
        for (int i = 0; i < count; i++)
        {
            room.Monsters.Add(_monsterSpawner.Spawn(type, 20, 0, 1, room, roomView.AddMonsterView));
        }
    }
}