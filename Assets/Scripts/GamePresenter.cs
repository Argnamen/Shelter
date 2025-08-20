using R3;
using System;
using UnityEngine;
using Zenject;

public class GamePresenter : IInitializable, IDisposable
{
    private readonly GameModel _model;
    private readonly UIView _uiView;
    private readonly RoomFactory _roomFactory;
    private readonly HeroSpawner _heroSpawner;

    private CompositeDisposable _disposables = new();
    private RoomType _selectedRoomType = RoomType.None;

    public GamePresenter(
        GameModel model,
        UIView uiView,
        RoomFactory roomFactory,
        HeroSpawner heroSpawner)
    {
        _model = model;
        _uiView = uiView;
        _roomFactory = roomFactory;
        _heroSpawner = heroSpawner;
    }

    public void Initialize()
    {
        // Подписка на кнопки UI
        _uiView.OnBuildButtonClicked.Subscribe(_ => ShowBuildMenu()).AddTo(_disposables);
        _uiView.OnPlayButtonClicked.Subscribe(_ => StartHeroWave()).AddTo(_disposables);

        // Подписка на выбор комнаты в build menu
        _uiView.OnCombatRoomSelected.Subscribe(_ => SelectRoomType(RoomType.Combat)).AddTo(_disposables);
        _uiView.OnRestRoomSelected.Subscribe(_ => SelectRoomType(RoomType.Rest)).AddTo(_disposables);
        _uiView.OnTreasureRoomSelected.Subscribe(_ => SelectRoomType(RoomType.Treasure)).AddTo(_disposables);
        _uiView.OnStairsRoomSelected.Subscribe(_ => SelectRoomType(RoomType.Stairs)).AddTo(_disposables);

        // Подписка на клик по сетке (если есть система сетки)
        // GridEvents.OnCellClicked.Subscribe(OnGridCellClicked).AddTo(_disposables);

        _model.Gold.Subscribe(gold => _uiView.UpdateGold(gold)).AddTo(_disposables);
        _model.DungeonLevel.Subscribe(level => _uiView.UpdateLevel(level)).AddTo(_disposables);
    }

    private void ShowBuildMenu()
    {
        _uiView.ToggleBuildMenu(true);
        _selectedRoomType = RoomType.None;
    }

    private void SelectRoomType(RoomType roomType)
    {
        _selectedRoomType = roomType;
        _uiView.HideBuildMenu();

        // Здесь можно показать подсветку доступных ячеек
        Debug.Log($"Selected room type: {roomType}");
    }

    private void OnGridCellClicked(Vector2Int gridPosition)
    {
        if (_selectedRoomType == RoomType.None) return;

        var room = _roomFactory.BuildRoom(_selectedRoomType, gridPosition);
        if (room != null)
        {
            // Успешно построили комнату
            _selectedRoomType = RoomType.None;
        }
    }

    private void StartHeroWave()
    {
        _heroSpawner.SpawnHeroWave();
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}