using R3;
using System;
using UnityEngine;

public class AIPlayer
{
    public Faction Faction { get; }
    private readonly GridService _gridService;
    private readonly RoomFactory _roomFactory;
    private readonly CompositeDisposable _disposables = new();

    public AIPlayer(Faction faction, GridService gridService, RoomFactory roomFactory)
    {
        Faction = faction;
        _gridService = gridService;
        _roomFactory = roomFactory;
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
        // ¬ыбираем случайный тип комнаты
        var roomType = GetRoomType();

        var availablePositions = _gridService.GetAvailablePositions(roomType ,Faction);

        if (availablePositions.Count == 0)
        {
            Debug.Log($"{Faction} has no available positions");
            return;
        }

        var randomPosition = availablePositions[UnityEngine.Random.Range(0, availablePositions.Count)];
        var monsterType = GetMonsterType(roomType);

        if (_gridService.TryPlaceRoom(roomType, randomPosition, Faction, monsterType))
        {
            Debug.Log($"{Faction} successfully built {roomType} at {randomPosition}");
        }
        else
        {
            Debug.LogWarning($"{Faction} failed to build at {randomPosition}");
        }
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
