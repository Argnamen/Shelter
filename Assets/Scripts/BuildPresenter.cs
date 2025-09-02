using DG.Tweening;
using R3;
using System;
using UnityEngine;
using Zenject;

public class BuildPresenter : IInitializable, IDisposable
{
    private readonly UIView _uiView;
    private readonly GridService _gridService;
    private readonly DungeonView _dungeonView;
    private readonly GameData _gameData;
    private readonly CameraController _cameraController;

    private CompositeDisposable _disposables = new();
    private RoomType _selectedRoomType = RoomType.Combat;
    private MonsterType _monsterType = MonsterType.None;
    private Vector2Int _destroyRoomPosition;
    private bool _isBuildMode = false;

    public BuildPresenter(
        UIView uiView,
        GridService gridService,
        DungeonView dungeonView,
        GameModel gameModel,
        GameData gameData,
        CameraController cameraController)
    {
        _uiView = uiView;
        _gridService = gridService;
        _dungeonView = dungeonView;
        _gameData = gameData;
        _cameraController = cameraController;
    }

    public void Initialize()
    {
        Debug.Log("BuildPresenter initialized");

        SetupUISubscriptions();
        SetupGridInput();
    }

    private void SetupUISubscriptions()
    {
        _uiView.OnBuildButtonClicked
            .Subscribe(_ => ToggleBuildMode())
            .AddTo(_disposables);

        _uiView.OnCloseBuildMenuClicked
            .Subscribe(_ => ToggleBuildMode())
            .AddTo(_disposables);

        _uiView.OnRoomSelected
            .Subscribe(room =>
            {
                _selectedRoomType = room.Type;
                _monsterType = room.MonsterType;
                _uiView.SetSelectedRoomText(_selectedRoomType);
                if (_isBuildMode) HighlightAvailablePositions();
            })
            .AddTo(_disposables);

        _uiView.OnDeleteRoomButtonClicked.Subscribe(_=> RemoveRoom()).AddTo(_disposables);
    }

    private void SetupGridInput()
    {
        // Подписка на клики мыши для строительства
        Observable.EveryUpdate()
            .Where(_ => Input.GetMouseButtonDown(0))
            .Subscribe(_ => HandleGridClick())
            .AddTo(_disposables);
    }

    private void HandleGridClick()
    {
        var mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var gridPosition = new Vector2Int(
            Mathf.RoundToInt(mouseWorldPos.x / _gameData.CellSize), // Соответствует CellSize
            Mathf.RoundToInt(mouseWorldPos.y / _gameData.CellSize)
        );

        if (_gridService.GetRoomAt(gridPosition) != null)
        {
            _cameraController.FocusOnRoom(_gridService.GetWorldPosition(gridPosition));

            if (_isBuildMode)
            {
                _destroyRoomPosition = gridPosition;
            }
        }

        if (_isBuildMode)
        {
            Debug.Log($"Clicked grid position: {gridPosition}");

            if (_gridService.CanPlaceRoomAt(gridPosition, _selectedRoomType))
            {
                TryBuildRoom(_selectedRoomType, gridPosition, _monsterType);
            }
            else
            {
                Debug.LogWarning($"Cannot build at position {gridPosition}");
                _uiView.ShowMessage("Cannot build here!");
            }
        }
    }

    private void ToggleBuildMode()
    {
        _isBuildMode = !_isBuildMode;
        _uiView.ToggleBuildMenu(_isBuildMode);

        if (_isBuildMode)
        {
            EnterBuildMode();
        }
        else
        {
            ExitBuildMode();
        }
    }

    private void EnterBuildMode()
    {
        Debug.Log("Entering build mode");
        HighlightAvailablePositions();
        _uiView.SetSelectedRoomText(_selectedRoomType);
        _uiView.ShowMessage("Build mode: Click on green cells to build");
    }

    private void ExitBuildMode()
    {
        Debug.Log("Exiting build mode");
        _dungeonView.ResetGridHighlight();
        _uiView.ShowMessage("Build mode deactivated");
    }

    private void HighlightAvailablePositions()
    {
        var availablePositions = _gridService.GetAvailablePositions(_selectedRoomType);
        _dungeonView.HighlightAvailablePositions(availablePositions);
    }

    public bool TryBuildRoom(RoomType roomType, Vector2Int position, MonsterType monsterType = MonsterType.None)
    {
        if (_gridService.TryPlaceRoom(roomType, position, monsterType))
        {
            _uiView.ShowMessage($"Built {roomType} room!");
            HighlightAvailablePositions(); // Обновляем подсветку
            return true;
        }

        _uiView.ShowMessage("Build failed!");
        return false;
    }

    public void RemoveRoom()
    {
        if (_destroyRoomPosition != null)
        {
            _gridService.RemoveRoom(_destroyRoomPosition);
            HighlightAvailablePositions();
        }
    }

    public void Dispose()
    {
        _disposables?.Dispose();
    }
}