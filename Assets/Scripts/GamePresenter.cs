using R3;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class GamePresenter : IInitializable, IDisposable
{
    private readonly GameModel _model;
    private readonly DungeonView _dungeonView;
    private readonly UIView _uiView;
    private readonly GridService _gridService;
    private readonly SquadSpawner _heroSpawner;
    private readonly CameraController _cameraController;
    private readonly GameData _gameData;
    private readonly DayCycleService _dayCycleService;

    private CompositeDisposable _disposables = new();
    private IDisposable _heroWaveSubscription;

    private bool _isPlay = false;

    public GamePresenter(
        GameModel model,
        DungeonView dungeonView,
        UIView uiView,
        GridService gridService,
        SquadSpawner heroSpawner,
        CameraController cameraController,
        GameData gameData,
        DayCycleService dayCycleService)
    {
        _model = model;
        _dungeonView = dungeonView;
        _uiView = uiView;
        _gridService = gridService;
        _heroSpawner = heroSpawner;
        _cameraController = cameraController;
        _gameData = gameData;
        _dayCycleService = dayCycleService;
    }

    public void Initialize()
    {
        SetupUISubscriptions();
        SetupModelSubscriptions();
        InitializeGame();
    }

    private void SetupUISubscriptions()
    {
        // Кнопка Play - запуск волны героев
        _uiView.OnPlayButtonClicked
            .Subscribe(_ => _uiView.OnPlay())
            .AddTo(_disposables);
        _uiView.OnPlayButtonClicked
            .Subscribe(_ => StartGameCycle())
            .AddTo(_disposables);

        // Кнопка закрытия меню строительства
        _uiView.OnCloseBuildMenuClicked
            .Subscribe(_ => _uiView.ToggleBuildMenu(false))
            .AddTo(_disposables);

        // Выбор типа комнаты
        _uiView.OnRoomSelected
            .Subscribe(room => _uiView.SetSelectedRoomText(room.Type))
            .AddTo(_disposables);
    }

    private void SetupModelSubscriptions()
    {
        // Подписки на изменения модели
        _model.Gold
            .Subscribe(gold => _uiView.UpdateGold(gold))
            .AddTo(_disposables);

        _model.DungeonLevel
            .Subscribe(level => _uiView.UpdateLevel(level))
            .AddTo(_disposables);

        // Подписка на изменение количества героев
        _model.ObserveSquadsCountChanged()
            .Subscribe(count => _uiView.UpdateHeroesCount(count))
            .AddTo(_disposables);
    }

    private void InitializeGame()
    {
        // Инициализация начального состояния
        _model.Gold.Value = _gameData.StartGold; // Стартовое золото
        _model.DungeonLevel.Value = 1;

        _dungeonView.InitializeGrid();

        // Создаем начальную комнату
        var startPosition = _gameData.StartRoomPosition;
        _gridService.TryPlaceRoom(RoomType.Rest, startPosition);

        _cameraController.FocusOnRoom(_gridService.GetWorldPosition(startPosition));

        _uiView.ShowMessage("Welcome to Dungeon Shelter! Build rooms and defend against heroes!");
    }

    private void StartGameCycle()
    {
        _isPlay = !_isPlay;

        if (_isPlay)
        {
            _dayCycleService.StartDay();
        }
        else
        {
            _dayCycleService.StopDay();
        }
    }

    private void StartHeroWave()
    {
        if (_heroWaveSubscription != null)
        {
            _heroWaveSubscription.Dispose();
        }

        _uiView.ShowMessage("Hero wave incoming!");

        // Запускаем волну героев с задержкой
        _heroWaveSubscription = Observable.Timer(TimeSpan.FromSeconds(1))
            .Subscribe(_ =>
            {
                _heroSpawner.SpawnHeroWave();
                ScheduleNextWave();
            })
            .AddTo(_disposables);
    }

    private void ScheduleNextWave()
    {
        // Планируем следующую волну через 30 секунд
        Observable.Timer(TimeSpan.FromSeconds(30))
            .Subscribe(_ => _uiView.ShowMessage("Heroes are preparing for next attack!"))
            .AddTo(_disposables);
    }

    public void AddGold(int amount)
    {
        _model.Gold.Value += amount;
        _uiView.ShowMessage($"+{amount} gold!", 1.5f);
    }

    public void LevelUpDungeon()
    {
        _model.DungeonLevel.Value++;
        _uiView.ShowMessage($"Dungeon Level Up! Now level {_model.DungeonLevel.Value}");
    }

    public void Dispose()
    {
        return;
        _disposables.Dispose();
        _heroWaveSubscription?.Dispose();
    }
}