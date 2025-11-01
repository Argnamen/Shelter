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
    private readonly DiContainer _diContainer;
    private readonly EnemyPlayerFactory _enemyPlayerFactory;
    private readonly SquadSpawner _spawner;
    private readonly GameModel _gameModel;

    public AIManager(GridService gridService, RoomFactory roomFactory, DayCycleService dayCycleService, DiContainer diContainer, EnemyPlayerFactory enemyPlayerFactory, SquadSpawner squadSpawner, GameModel gameModel)
    {
        _gridService = gridService;
        _roomFactory = roomFactory;
        _dayCycleService = dayCycleService;
        _diContainer = diContainer;
        _enemyPlayerFactory = enemyPlayerFactory;
        _spawner = squadSpawner;
        _gameModel = gameModel;
    }

    public void Initialize()
    {
        // Создаем AI противников
        CreateAIPlayers();

        _dayCycleService.IsTimeStop.Subscribe(StartAutoSpawning).AddTo(_disposables);
    }

    public async void StartAutoSpawning(bool isTimeStop)
    {
        while (isTimeStop)
        {
            ExecuteAITurns();

            await Task.Delay(10);
        }
    }

    private void CreateAIPlayers()
    {
        var enemyFactions = new[] { Faction.Enemy1, Faction.Enemy2, Faction.Enemy3 };
        var playerList = new[] { 
            CreatyPlayer(PlayerType.SkeletonLedi),
            CreatyPlayer(PlayerType.GigaKrish) };

        foreach (var faction in enemyFactions)
        {
            var player = playerList[UnityEngine.Random.Range(0, playerList.Length)];
            var aiPlayer = new AIPlayer(player, faction, _gridService, _roomFactory, _spawner, _dayCycleService, _gameModel);
            _aiPlayers.Add(aiPlayer);
        }

        Debug.Log($"Created {_aiPlayers.Count} AI players");
    }

    private IEnemyPlayer CreatyPlayer(PlayerType playerType)
    {
        return _enemyPlayerFactory.Create(playerType);
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
    Enemy3,
    Enemy4,
}
