using R3;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class AIManager : IInitializable, IDisposable
{
    private readonly List<AIPlayer> _aiPlayers = new();
    private readonly GridService _gridService;
    private readonly RoomFactory _roomFactory;
    private readonly DayCycleService _dayCycleService;
    private readonly CompositeDisposable _disposables = new();

    public AIManager(GridService gridService, RoomFactory roomFactory, DayCycleService dayCycleService)
    {
        _gridService = gridService;
        _roomFactory = roomFactory;
        _dayCycleService = dayCycleService;
    }

    public void Initialize()
    {
        // Создаем AI противников
        CreateAIPlayers();

        _dayCycleService.IsTimeStop.Subscribe(StartAutoSpawning).AddTo(_disposables);
    }

    public async void StartAutoSpawning(bool isTimeStop)
    {
        while (!isTimeStop)
        {
            ExecuteAITurns();

            await Task.Delay(100);
        }
    }

    private void CreateAIPlayers()
    {
        var enemyFactions = new[] { Faction.Enemy1, Faction.Enemy2, Faction.Enemy3 };

        foreach (var faction in enemyFactions)
        {
            var aiPlayer = new AIPlayer(faction, _gridService, _roomFactory);
            _aiPlayers.Add(aiPlayer);
        }

        Debug.Log($"Created {_aiPlayers.Count} AI players");
    }

    private void ExecuteAITurns()
    {
        foreach (var aiPlayer in _aiPlayers)
        {
            aiPlayer.StartAITurn();
        }
    }

    public void Dispose()
    {
        foreach (var aiPlayer in _aiPlayers)
        {
            aiPlayer.Dispose();
        }
        _disposables?.Dispose();
    }
}

public enum Faction
{
    Player,
    Enemy1,
    Enemy2,
    Enemy3
}
