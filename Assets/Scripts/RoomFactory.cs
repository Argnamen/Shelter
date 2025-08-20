using R3;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class RoomFactory
{
    private readonly DungeonView _dungeonView;
    private readonly MonsterFactory _monsterFactory;
    private readonly DiContainer _container;
    private readonly GameModel _gameModel;

    // Конфигурация комнат
    private readonly Dictionary<RoomType, RoomConfig> _roomConfigs = new()
    {
        {
            RoomType.Combat, new RoomConfig
            {
                MonsterTypes = new List<MonsterType> { MonsterType.Slime, MonsterType.Slime, MonsterType.Slime },
                MonsterHealth = 30,
                Cost = 50
            }
        },
        {
            RoomType.Rest, new RoomConfig
            {
                MonsterTypes = new List<MonsterType> { MonsterType.Skeleton },
                MonsterHealth = 20,
                Cost = 30
            }
        },
        {
            RoomType.Treasure, new RoomConfig
            {
                MonsterTypes = new List<MonsterType> { MonsterType.Goblin, MonsterType.Goblin },
                MonsterHealth = 25,
                Cost = 70
            }
        },
        {
            RoomType.Stairs, new RoomConfig
            {
                MonsterTypes = new List<MonsterType>(),
                MonsterHealth = 0,
                Cost = 100
            }
        }
    };

    public RoomFactory(
        DungeonView dungeonView,
        MonsterFactory monsterFactory,
        DiContainer container,
        GameModel gameModel)
    {
        _dungeonView = dungeonView;
        _monsterFactory = monsterFactory;
        _container = container;
        _gameModel = gameModel;
    }

    public bool CanBuildRoom(RoomType roomType, Vector2Int gridPosition)
    {
        // Проверяем достаточно ли денег
        if (_gameModel.Gold.Value < _roomConfigs[roomType].Cost)
            return false;

        // Проверяем не занята ли позиция
        foreach (var existingRoom in _gameModel.Rooms)
        {
            if (existingRoom.Position == gridPosition)
                return false;
        }

        return true;
    }

    public RoomModel BuildRoom(RoomType roomType, Vector2Int gridPosition)
    {
        if (!CanBuildRoom(roomType, gridPosition))
        {
            Debug.LogWarning($"Cannot build {roomType} at {gridPosition}");
            return null;
        }

        // Списываем стоимость
        _gameModel.Gold.Value -= _roomConfigs[roomType].Cost;

        // Создаём модель комнаты
        var roomModel = new RoomModel(roomType, gridPosition);

        // Добавляем монстров
        AddMonstersToRoom(roomModel, roomType);

        // Создаём View комнаты
        var worldPosition = GridToWorldPosition(gridPosition);
        var roomView = _dungeonView.CreateRoomView(roomModel, worldPosition);

        if (roomView == null)
        {
            Debug.LogError($"Failed to create room view for {roomType}");
            return null;
        }

        // Создаём монстров вьюхи
        CreateMonsterViews(roomModel, roomView);

        // Создаём презентер комнаты (без монстров)
        _container.Instantiate<RoomPresenter>(new object[] { roomModel, roomView });

        // Добавляем комнату в модель игры
        _gameModel.Rooms.Add(roomModel);

        Debug.Log($"Built {roomType} room at {gridPosition}");
        return roomModel;
    }

    private void AddMonstersToRoom(RoomModel roomModel, RoomType roomType)
    {
        var config = _roomConfigs[roomType];

        foreach (var monsterType in config.MonsterTypes)
        {
            var monsterModel = new MonsterModel(monsterType, config.MonsterHealth);
            roomModel.Monsters.Add(monsterModel);
        }
    }

    private void CreateMonsterViews(RoomModel roomModel, RoomView roomView)
    {
        foreach (var monsterModel in roomModel.Monsters.ToList()) // ToList() для безопасной итерации
        {
            var monsterView = _monsterFactory.Create(monsterModel.Type);
            if (monsterView != null)
            {
                roomView.AddMonsterView(monsterView);

                // Гарантируем наличие DisposableCollector
                var disposableCollector = monsterView.GetComponent<DisposableCollector>()
                    ?? monsterView.gameObject.AddComponent<DisposableCollector>();

                // Создаем локальную копию для замыкания
                var currentMonsterModel = monsterModel;
                var currentMonsterView = monsterView;
                var currentRoomModel = roomModel;

                disposableCollector.Disposables.Add(
                    currentMonsterModel.Health.Subscribe(health =>
                    {
                        if (health <= 0)
                        {
                            currentMonsterView.Die();
                            currentRoomModel.Monsters.Remove(currentMonsterModel);
                        }
                    })
                );
            }
        }
    }


    private Vector3 GridToWorldPosition(Vector2Int gridPosition)
    {
        return new Vector3(gridPosition.x * 2f, gridPosition.y * 2f, 0);
    }

    public int GetRoomCost(RoomType roomType)
    {
        return _roomConfigs.ContainsKey(roomType) ? _roomConfigs[roomType].Cost : 0;
    }

    // Вспомогательный класс конфигурации комнаты
    private class RoomConfig
    {
        public List<MonsterType> MonsterTypes;
        public int MonsterHealth;
        public int Cost;
    }
}