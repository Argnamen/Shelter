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
    private readonly HeroSpawner _heroSpawner;

    private CompositeDisposable _disposables = new();
    private IDisposable _heroWaveSubscription;

    public GamePresenter(
        GameModel model,
        DungeonView dungeonView,
        UIView uiView,
        GridService gridService,
        HeroSpawner heroSpawner)
    {
        _model = model;
        _dungeonView = dungeonView;
        _uiView = uiView;
        _gridService = gridService;
        _heroSpawner = heroSpawner;
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
            .Subscribe(_ => StartHeroWave())
            .AddTo(_disposables);

        // Кнопка закрытия меню строительства
        _uiView.OnCloseBuildMenuClicked
            .Subscribe(_ => _uiView.ToggleBuildMenu(false))
            .AddTo(_disposables);

        // Выбор типа комнаты
        _uiView.OnRoomTypeSelected
            .Subscribe(roomType => _uiView.SetSelectedRoomText(roomType))
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
        _model.ObserveHeroesCountChanged()
            .Subscribe(count => _uiView.UpdateHeroesCount(count))
            .AddTo(_disposables);
    }

    private void InitializeGame()
    {
        // Инициализация начального состояния
        _model.Gold.Value = 500; // Стартовое золото
        _model.DungeonLevel.Value = 1;

        _dungeonView.InitializeGrid();

        // Создаем начальную комнату
        var startPosition = new Vector2Int(2, 2);
        _gridService.TryPlaceRoom(RoomType.Combat, startPosition);

        _uiView.ShowMessage("Welcome to Dungeon Shelter! Build rooms and defend against heroes!");
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