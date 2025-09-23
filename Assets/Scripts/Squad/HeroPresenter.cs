using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using System;
using System.Collections.Generic;
using UnityEngine;

public class HeroPresenter : IDisposable
{
    private readonly HeroModel _model;
    private readonly HeroView _view;
    private readonly GameModel _gameModel;
    private readonly WinSystem _winSystem;

    private RoomModel _roomModel;

    private CompositeDisposable _disposables = new();
    private bool _isDisposed = false;

    public bool isDie = false;

    private bool _isBattle = false;

    public HeroPresenter(
        HeroModel model,
        HeroView view,
        GameModel gameModel,
        WinSystem winSystem)
    {
        _model = model;
        _view = view;
        _gameModel = gameModel;
        _winSystem = winSystem;

        Initialize();
    }

    private void Initialize()
    {
        _view.SetHealth(_model.Health.Value);
        _model.CurrentRoomModel.Subscribe(OnRoomChanged).AddTo(_disposables);
        _model.Health.Subscribe(OnHealthChanged).AddTo(_disposables);
        _model.Health.Subscribe(_view.UpdateHealth).AddTo(_disposables);
    }

    private void OnHealthChanged(int health)
    {
        if (health <= 0 && !_isDisposed)
        {
            _winSystem.AddWinPoint(EventType.DieHero);
            isDie = true;
            _gameModel.AddGold(_model.Gold);
            Dispose();
        }
    }

    private async void OnRoomChanged(RoomModel room)
    {
        if (_roomModel == null && room == null)
            return;

        _model.HeroIsReady.Value = false;

        if (_roomModel != null)
        {
            _roomModel.Enter.Value = false;

            switch (_roomModel.Type)
            {
                case RoomType.Treasure:
                    _winSystem.AddWinPoint(EventType.OpenChest);
                    break;
                case RoomType.Combat:
                    if(_isBattle)
                        _winSystem.AddWinPoint(EventType.WinBattle);
                    break;
            }
        }

        _roomModel = room;

        if (_isDisposed) return;

        if(room == null)
        {
            await MoveToExit();

            _winSystem.AddWinPoint(EventType.CleanDangeon);

            Dispose();
            return;
        }
        else
        {
            await new WaitWhile(() => room.Enter.Value);

            room.AddHeroView.Value = _view;

            await MoveToRoom(room);
        }

        _model.HeroIsReady.Value = true;
    }
    private async UniTask MoveToRoom(RoomModel room)
    {
        if (_isDisposed) return;

        _view.SetMoving(true);

        await MoveToPosition();

        _view.SetMoving(false);

        room.Enter.Value = true;

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
        Vector3 startPosition = _view.transform.localPosition;

        if (_isDisposed) return;

        await _view.transform.DOLocalMove(Vector3.zero, duration)
            .SetEase(Ease.Flash)
            .AsyncWaitForCompletion();

        if (!_isDisposed)
        {
            _view.transform.localPosition = Vector3.zero;
        }
    }

    private async UniTask FightMonsters(RoomModel room)
    {
        if (_isDisposed) return;

        _view.SetFighting();
        await UniTask.Delay(500);

        while (room.Monsters.Count > 0)
        {
            if (room.Monsters[0].Health.Value > 0)
            {
                if (_isDisposed) break;

                room.Monsters[0].Health.Value -= _model.Damage.Value;

                _isBattle = true;
            }

            if (_isDisposed) break;

            await UniTask.Delay(_model.DamageSpead.Value);
        }

        _view.SetFighting(false);
    }

    public async UniTask RestInRoom(RoomModel room)
    {
        if (_isDisposed) return;


        while (_model.Health.Value != _model.MaxHealth)
        {
            await UniTask.Delay(1000);

            if (_isDisposed) return;

            _model.Health.Value = Mathf.Min(_model.MaxHealth, _model.Health.Value + 30);
        }

        await UniTask.Delay(100);
    }

    public async UniTask LootTreasure(RoomModel room)
    {
        if (_isDisposed) return;

        await UniTask.Delay(1000);

        if (!_isDisposed)
        {
            switch (UnityEngine.Random.Range(0, 5))
            {
                case 0:
                    break;
                case 1:
                    _model.MaxHealth += 5;
                    _model.Health.Value = _model.Health.Value;
                    break;
                case 2:
                    _model.Damage.Value += 5;
                    break;
                case 3:
                    _model.Health.Value += 20;
                    break;
                case 4:
                    _model.DamageSpead.Value += 10;
                    break;
            }
        }

        await UniTask.Delay(100);
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        _isDisposed = true;
        _disposables.Dispose();
        _model.Die.Value = true;

        if (_view != null)
        {
            _view.Die();
        }
    }
}