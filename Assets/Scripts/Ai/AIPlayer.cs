using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Collections.Generic;
using UnityEngine;

public class AIPlayer
{
    public Faction Faction { get; }

    private readonly IEnemyPlayer _player;
    private readonly GridService _gridService;
    private readonly RoomFactory _roomFactory;
    private readonly SquadSpawner _spawner;
    private readonly DayCycleService _dayCycleService;
    private readonly GameModel _gameModel;
    private readonly CompositeDisposable _disposables = new();

    private bool _isSpawnHero = false;

    public AIPlayer(IEnemyPlayer player, Faction faction, GridService gridService, RoomFactory roomFactory, SquadSpawner squadSpawner, DayCycleService dayCycleService, GameModel gameModel)
    {
        _player = player;
        Faction = faction;
        _gridService = gridService;
        _roomFactory = roomFactory;
        _spawner = squadSpawner;
        _dayCycleService = dayCycleService;
        _gameModel = gameModel;

        _dayCycleService.Time.Subscribe(NewDay).AddTo(_disposables);
        _dayCycleService.IsTimeStop.Subscribe(TimeStop).AddTo(_disposables);
    }

    public void StartAITurn()
    {
        Debug.Log($"{Faction} starting AI turn");

        // ∆дем случайное врем€ перед действием (1-3 секунды)
        Observable.Timer(TimeSpan.FromSeconds(UnityEngine.Random.Range(1f, 3f)))
            .Subscribe(_ => ExecuteAIAction())
            .AddTo(_disposables);
    }

    private void NewDay(float time)
    {
        if (time <= 0)
        {
            _gameModel.AddGold(100, Faction);

            Debug.Log("AAAAAA " + Faction + " AAAA " + _gameModel.GetGold(Faction));
        }
    }

    private void TimeStop(bool isTimeStop)
    {
        if (!isTimeStop)
        {
            _isSpawnHero = true;
        }
        else
        {
            _isSpawnHero = false;
        }
    }

    private void ExecuteAIAction()
    {
        if (!_isSpawnHero)
        {
            SpawnRooms();
        }
        else
        {
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
        var roomTypes = new[] { 
            RoomType.Stairs,
            RoomType.Combat,
            RoomType.Rest,
            RoomType.Treasure
        };

        float[] installRoomChance = new float[roomTypes.Length];

        int indexReturn = 0;

        for (int i = 0; i < installRoomChance.Length; i++)
        {
            installRoomChance[i] = (_gridService.GetAvailablePositions(roomTypes[i], Faction).Count / 100f);
        }

        for (int i = 0; i < installRoomChance.Length; i++)
        {
            if (installRoomChance[indexReturn] == installRoomChance[i] && roomTypes[indexReturn] != RoomType.Stairs)
            {
                if(UnityEngine.Random.Range(0, 2) > 0)
                    indexReturn = i;
            }

            if (installRoomChance[indexReturn] < installRoomChance[i])
            {
                indexReturn = i;

                
            }
        }

        return roomTypes[indexReturn];
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
