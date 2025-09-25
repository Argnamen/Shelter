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
        var roomTypes = new[] { RoomType.Combat, RoomType.Rest, RoomType.Treasure };
        var randomRoomType = roomTypes[UnityEngine.Random.Range(0, roomTypes.Length)];

        var availablePositions = _gridService.GetAvailablePositions(randomRoomType ,Faction);

        if (availablePositions.Count == 0)
        {
            Debug.Log($"{Faction} has no available positions");
            return;
        }

        var randomPosition = availablePositions[UnityEngine.Random.Range(0, availablePositions.Count)];

        if (_gridService.TryPlaceRoom(randomRoomType, randomPosition, Faction))
        {
            Debug.Log($"{Faction} successfully built {randomRoomType} at {randomPosition}");
        }
        else
        {
            Debug.LogWarning($"{Faction} failed to build at {randomPosition}");
        }
    }

    public void Dispose()
    {
        _disposables?.Dispose();
    }
}
