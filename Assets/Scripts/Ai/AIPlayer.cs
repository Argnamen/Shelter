using Cysharp.Threading.Tasks;
using R3;
using System;
using UnityEngine;

public class AIPlayer
{
    public Faction Faction { get; }

    private readonly IEnemyPlayer _player;
    private readonly GridService _gridService;
    private readonly RoomFactory _roomFactory;
    private readonly SquadSpawner _spawner;
    private readonly DayCycleService _dayCycleService;
    private readonly CompositeDisposable _disposables = new();

    private bool _isSpawnHero = false;

    public AIPlayer(IEnemyPlayer player, Faction faction, GridService gridService, RoomFactory roomFactory, SquadSpawner squadSpawner, DayCycleService dayCycleService)
    {
        _player = player;
        Faction = faction;
        _gridService = gridService;
        _roomFactory = roomFactory;
        _spawner = squadSpawner;
        _dayCycleService = dayCycleService;
    }

    public void StartAITurn()
    {
        Debug.Log($"{Faction} starting AI turn");

        // ∆дем случайное врем€ перед действием (1-3 секунды)
        Observable.Timer(TimeSpan.FromSeconds(UnityEngine.Random.Range(1f, 3f)))
            .Subscribe(_ => ExecuteAIAction())
            .AddTo(_disposables);
    }

    private void ExecuteAIAction()
    {
        if (_dayCycleService.Time.Value <= 0)
            _isSpawnHero = false;

        if (!_isSpawnHero)
        {
            if (SpawnRooms())
            {
                return;
            }
        }

        if (!_dayCycleService.IsTimeStop.Value)
        {
            _isSpawnHero = true;
            SpawnSquad(_dayCycleService.Time.Value);
        }

    }

    private bool SpawnRooms()
    {
        var roomType = GetRoomType();

        var availablePositions = _gridService.GetAvailablePositions(roomType, Faction);

        if (availablePositions.Count == 0)
        {
            Debug.Log($"{Faction} has no available positions");
            return false;
        }

        var randomPosition = availablePositions[UnityEngine.Random.Range(0, availablePositions.Count)];
        var monsterType = GetMonsterType(roomType);

        if (_gridService.TryPlaceRoom(roomType, randomPosition, Faction, monsterType))
        {
            Debug.Log($"{Faction} successfully built {roomType} at {randomPosition}");
            return true;
        }
        else
        {
            Debug.LogWarning($"{Faction} failed to build at {randomPosition}");
            return false;
        }


    }

    private void SpawnSquad(float time)
    {
        _spawner.StartAutoSpawning(Faction, time);
    }

    private RoomType GetRoomType()
    {
        var roomTypes = new[] { RoomType.Combat, RoomType.Stairs, RoomType.Rest, RoomType.Treasure };

        for (int i = 0; i < roomTypes.Length; i++)
        {
            if (_gridService.GetAvailablePositions(roomTypes[i], Faction).Count > 0)
                return roomTypes[i];
        }

        return roomTypes[UnityEngine.Random.Range(0, roomTypes.Length)];
    }

    private MonsterType GetMonsterType(RoomType roomType)
    {
        if (roomType != RoomType.Combat)
            return MonsterType.None;

        var monsterTypes = new[] { MonsterType.Slime, MonsterType.Eagle, MonsterType.Skeleton };

        return monsterTypes[UnityEngine.Random.Range(0, monsterTypes.Length)];
    }

    public void Dispose()
    {
        _disposables?.Dispose();
    }
}
