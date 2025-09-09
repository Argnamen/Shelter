using UnityEngine;
using Zenject;
using System.Collections.Generic;

public class RoomFactory
{
    private readonly DungeonView _dungeonView;
    private readonly DiContainer _container;
    private readonly GridModel _gridModel;
    private readonly GameData _gameData;
    private readonly MonsterSpawner _monsterSpawner;
    
    private List<RoomPresenter> roomPresenters = new List<RoomPresenter>();

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

        if (roomView == null)
        {
            Debug.LogError($"Failed to create room view for type: {type}");
            return null;
        }

        // Создаем презентер
        _container.Instantiate<RoomPresenter>(new object[] { roomModel, roomView });

        AddMonstersToRoom(roomModel);

        Debug.Log($"Room created at {position} - {type}");
        return roomModel;
    }

    private void AddMonstersToRoom(RoomModel room)
    {
        switch (room.Type)
        {
            case RoomType.Combat:
                AddMonsters(room, room.Monster, 2);
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

    private void AddMonsters(RoomModel room, MonsterType type, int count)
    {
        for (int i = 0; i < count; i++)
        {
            room.Monsters.Add(_monsterSpawner.Spawn(type, room));
        }
    }
}