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
    private readonly DayCycleService _dayCycleService;
    private readonly WinSystem _winSystem;

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
        CameraController cameraController,
        DayCycleService dayCycleService,
        WinSystem winSystem)
    {
        _uiView = uiView;
        _gridService = gridService;
        _dungeonView = dungeonView;
        _gameData = gameData;
        _cameraController = cameraController;
        _dayCycleService = dayCycleService;
        _winSystem = winSystem;
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
                if (_isBuildMode) HighlightAvailablePositions(Faction.Player);
            })
            .AddTo(_disposables);

        _uiView.OnDeleteRoomButtonClicked.Subscribe(_=> RemoveRoom(Faction.Player)).AddTo(_disposables);

        _uiView.OnSwitchToPlayer.Subscribe(_ => SwitchCharacterVisible(Faction.Player)).AddTo(_disposables);
        _uiView.OnSwitchToEnemys[0].Subscribe(_ => SwitchCharacterVisible(Faction.Enemy1)).AddTo(_disposables);
        _uiView.OnSwitchToEnemys[1].Subscribe(_ => SwitchCharacterVisible(Faction.Enemy2)).AddTo(_disposables);
        _uiView.OnSwitchToEnemys[2].Subscribe(_ => SwitchCharacterVisible(Faction.Enemy3)).AddTo(_disposables);

        _dayCycleService.Time.Subscribe(x => _uiView.DayValue = x).AddTo(_disposables);

        _winSystem.Points[WinPoint.Interes].Subscribe(x => _uiView.WinInteres = 1 - (float)x / _gameData.MaxInteres).AddTo(_disposables);
        _winSystem.Points[WinPoint.Gold].Subscribe(x => _uiView.WinGold = 1 - (float)x / _gameData.MaxGold).AddTo(_disposables);
        _winSystem.Points[WinPoint.Vlianie].Subscribe(x => _uiView.WinVlianie = 1 - (float)x / _gameData.MaxVlianie).AddTo(_disposables);
        _winSystem.Points[WinPoint.Ghost].Subscribe(x => _uiView.WinGhost = 1 - (float)x / _gameData.MaxGhost).AddTo(_disposables);
    }

    private void SetupGridInput()
    {
        // Подписка на клики мыши для строительства
        Observable.EveryUpdate()
            .Where(_ => Input.GetMouseButtonDown(0))
            .Subscribe(_ => HandleGridClick())
            .AddTo(_disposables);
    }

    private void SwitchCharacterVisible(Faction faction)
    {
        _dungeonView.ActivateRoomsContainer(faction);
    }

    private void HandleGridClick()
    {
        var mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var gridPosition = _dungeonView.WorldToGridPosition(mouseWorldPos);
        

        if (_gridService.GetRoomAt(gridPosition, Faction.Player) != null)
        {
            _cameraController.FocusOnRoom(_dungeonView.GridCells[gridPosition.x, gridPosition.y].transform.position);

            if (_isBuildMode)
            {
                _destroyRoomPosition = gridPosition;
            }
        }

        if (_isBuildMode)
        {
            Debug.Log($"Clicked grid position: {gridPosition}");

            if (_gridService.CanPlaceRoomAt(gridPosition, _selectedRoomType, Faction.Player))
            {
                TryBuildRoom(_selectedRoomType, gridPosition, Faction.Player, _monsterType);
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
        HighlightAvailablePositions(Faction.Player);
        _uiView.SetSelectedRoomText(_selectedRoomType);
        _uiView.ShowMessage("Build mode: Click on green cells to build");
    }

    private void ExitBuildMode()
    {
        Debug.Log("Exiting build mode");
        _dungeonView.ResetGridHighlight();
        _uiView.ShowMessage("Build mode deactivated");
    }

    private void HighlightAvailablePositions(Faction faction)
    {
        var availablePositions = _gridService.GetAvailablePositions(_selectedRoomType, faction);
        _dungeonView.HighlightAvailablePositions(availablePositions);
    }

    public bool TryBuildRoom(RoomType roomType, Vector2Int position, Faction faction, MonsterType monsterType = MonsterType.None)
    {
        if (_gridService.TryPlaceRoom(roomType, position, faction, monsterType))
        {
            _uiView.ShowMessage($"Built {roomType} room!");
            HighlightAvailablePositions(faction); // Обновляем подсветку
            return true;
        }

        _uiView.ShowMessage("Build failed!");
        return false;
    }

    public void RemoveRoom(Faction faction)
    {
        if (_destroyRoomPosition != null)
        {
            _gridService.RemoveRoom(_destroyRoomPosition, faction);
            HighlightAvailablePositions(faction);
        }
    }

    public void Dispose()
    {
        _disposables?.Dispose();
    }
}