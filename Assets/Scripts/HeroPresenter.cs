using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Collections.Generic;
using UnityEngine;

public class HeroPresenter : IDisposable
{
    private readonly HeroModel _model;
    private readonly HeroView _view;
    private readonly GameModel _gameModel;
    private readonly GridService _gridService;
    private readonly HeroSpawner _heroSpawner;
    private readonly GameData _gameData;

    private RoomModel _roomModel;

    private CompositeDisposable _disposables = new();
    private bool _isDisposed = false;

    public bool isDie = false;

    public HeroPresenter(
        HeroModel model,
        HeroView view,
        GameModel gameModel,
        GridService gridService,
        HeroSpawner heroSpawner,
        GameData gameData)
    {
        _model = model;
        _view = view;
        _gameModel = gameModel;
        _gridService = gridService;
        _heroSpawner = heroSpawner;
        _gameData = gameData;

        Initialize();
    }

    private void Initialize()
    {
        _model.CurrentRoomModel.Subscribe(OnRoomChanged).AddTo(_disposables);
        _model.Health.Subscribe(OnHealthChanged).AddTo(_disposables);
    }

    private void OnHealthChanged(int health)
    {
        if (health <= 0 && !_isDisposed)
        {
            isDie = true;
            Die();
        }
    }

    private async void OnRoomChanged(RoomModel room)
    {
        if (_roomModel == null && room == null)
            return;

        _model.HeroIsReady.Value = false;

        _roomModel = room;

        if (_isDisposed) return;

        if(room == null)
        {
            await MoveToExit();
            Dispose();
            return;
        }
        else
        {
            room.AddHeroView.Value = _view;

            await MoveToRoom(room);
        }

        _model.HeroIsReady.Value = true;
    }
    private async UniTask MoveToRoom(RoomModel room)
    {
        if (_isDisposed) return;

        _roomModel.Enter.Value = false;

        _view.SetMoving(true);

        await MoveToPosition();

        _view.SetMoving(false);

        _roomModel.Enter.Value = true;

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
    }

    private async UniTask MoveToExit()
    {
        if (_isDisposed) return;

        _view.SetMoving(true);
        await MoveToPosition();
        _view.SetMoving(false);
    }

    private async UniTask MoveToPosition()
    {
        if (_isDisposed) return;

        float duration = 1f;
        float elapsed = 0f;
        Vector3 startPosition = _view.transform.localPosition;

        while (elapsed < duration)
        {
            if (_isDisposed) return;

            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            _view.transform.localPosition = Vector3.Lerp(startPosition, Vector3.zero, t);
            await UniTask.Yield();
        }

        if (!_isDisposed)
        {
            _view.transform.localPosition = Vector3.zero;
        }
    }

    public async UniTask FightMonsters(RoomModel room)
    {
        if (_isDisposed || room.Monsters.Count == 0) return;

        _view.SetFighting();
        await UniTask.Delay(500);

        foreach (var monster in room.Monsters.ToArray()) // Используем ToArray чтобы избежать модификации коллекции
        {
            while (monster.Health.Value > 0)
            {
                if (_isDisposed) break;

                monster.Health.Value -= _model.Damage.Value;
                await UniTask.Delay(_model.DamageSpead.Value);
            }

            if (_isDisposed) break;
        }

        _view.SetFighting(false);
    }

    public async UniTask RestInRoom(RoomModel room)
    {
        if (_isDisposed) return;

        await UniTask.Delay(2000);

        if (!_isDisposed)
        {
            _model.Health.Value = Mathf.Min(100, _model.Health.Value + 30);
        }
    }

    public async UniTask LootTreasure(RoomModel room)
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
            gridPosition.x * _gameData.CellSize,
            gridPosition.y * _gameData.CellSize,
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
            _view.Die();
        }
    }
}