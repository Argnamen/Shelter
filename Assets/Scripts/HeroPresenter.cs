using Cysharp.Threading.Tasks;
using R3;
using System;
using UnityEngine;

public class HeroPresenter : IDisposable
{
    private readonly HeroModel _model;
    private readonly HeroView _view;
    private readonly GameModel _gameModel;
    private readonly GridService _gridService;
    private readonly HeroSpawner _heroSpawner;
    private readonly GridModel _gridModel;

    private CompositeDisposable _disposables = new();
    private bool _isDisposed = false;

    public HeroPresenter(
        HeroModel model,
        HeroView view,
        GameModel gameModel,
        GridService gridService,
        HeroSpawner heroSpawner,
        GridModel gridModel)
    {
        _model = model;
        _view = view;
        _gameModel = gameModel;
        _gridService = gridService;
        _heroSpawner = heroSpawner;
        _gridModel = gridModel;

        Initialize();
    }

    private void Initialize()
    {
        _model.CurrentRoomIndex.Subscribe(OnRoomChanged).AddTo(_disposables);
        _model.Health.Subscribe(OnHealthChanged).AddTo(_disposables);
    }

    private async void OnRoomChanged(int roomIndex)
    {
        if (_isDisposed) return;

        if (roomIndex < 0 || roomIndex >= _gameModel.Rooms.Count)
        {
            await MoveToExit();
            Dispose();
            return;
        }

        var room = _gameModel.Rooms[roomIndex];
        await MoveToRoom(room);

        switch (room.Type)
        {
            case RoomType.Combat:
                await FightMonsters(room);
                break;
            case RoomType.Rest:
                await RestInRoom(room);
                break;
            case RoomType.Treasure:
                await LootTreasure(room);
                break;
        }

        // Переходим к следующей комнате
        _model.CurrentRoomIndex.Value++;
    }

    private void OnHealthChanged(int health)
    {
        if (health <= 0 && !_isDisposed)
        {
            Die();
        }
    }

    private async UniTask MoveToRoom(RoomModel room)
    {
        if (_isDisposed) return;

        _view.SetMoving(true);

        var roomPosition = _gridService.GetRoomPosition(room);
        if (roomPosition.HasValue)
        {
            var worldPosition = GetWorldPosition(roomPosition.Value);
            await MoveToPosition(worldPosition);
        }

        _view.SetMoving(false);
    }

    private async UniTask MoveToExit()
    {
        if (_isDisposed) return;

        _view.SetMoving(true);
        await MoveToPosition(new Vector3(-5, 2, 0));
        _view.SetMoving(false);
    }

    private async UniTask MoveToPosition(Vector3 targetPosition)
    {
        if (_isDisposed) return;

        float duration = 1f;
        float elapsed = 0f;
        Vector3 startPosition = _view.transform.position;

        while (elapsed < duration && !_isDisposed)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            _view.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            await UniTask.Yield();
        }

        if (!_isDisposed)
        {
            _view.transform.position = targetPosition;
        }
    }

    private async UniTask FightMonsters(RoomModel room)
    {
        if (_isDisposed || room.Monsters.Count == 0) return;

        _view.SetFighting();
        await UniTask.Delay(500);

        foreach (var monster in room.Monsters.ToArray()) // Используем ToArray чтобы избежать модификации коллекции
        {
            if (_isDisposed) break;

            monster.Health.Value -= 10;
            _model.Health.Value -= 5;
            await UniTask.Delay(300);
        }

        _view.SetFighting(false);
    }

    private async UniTask RestInRoom(RoomModel room)
    {
        if (_isDisposed) return;

        await UniTask.Delay(2000);

        if (!_isDisposed)
        {
            _model.Health.Value = Mathf.Min(100, _model.Health.Value + 30);
        }
    }

    private async UniTask LootTreasure(RoomModel room)
    {
        if (_isDisposed) return;

        await UniTask.Delay(1000);

        if (!_isDisposed)
        {
            // Награда за сундук
            int goldReward = UnityEngine.Random.Range(10, 30);
            _gameModel.Gold.Value += goldReward;
        }
    }

    private void Die()
    {
        if (_isDisposed) return;

        _view.Die();
        Dispose();
    }

    private Vector3 GetWorldPosition(Vector2Int gridPosition)
    {
        return new Vector3(
            gridPosition.x * _gridModel.CellSize,
            gridPosition.y * _gridModel.CellSize,
            0
        );
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        _isDisposed = true;
        _disposables.Dispose();
        _heroSpawner.RemoveHero(_model);

        if (_view != null)
        {
            UnityEngine.Object.Destroy(_view.gameObject);
        }
    }
}