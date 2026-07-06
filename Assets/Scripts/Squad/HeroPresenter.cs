using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class HeroPresenter : IDisposable
{
    private readonly HeroModel _model;
    private readonly HeroView _view;
    private readonly GameModel _gameModel;
    private readonly WinSystem _winSystem;
    private readonly DayCycleService _dayCycleService;

    private RoomModel _roomModel;
    private List<RoomModel> _roomsToExit = new List<RoomModel>();
    private Tweener _tweener;
    private bool _isTimeStop = false;

    private CompositeDisposable _disposables = new();
    private bool _isDisposed = false;

    public bool isDie = false;

    private bool _isBattle = false;

    public HeroPresenter(
        HeroModel model,
        HeroView view,
        GameModel gameModel,
        WinSystem winSystem,
        DayCycleService dayCycleService)
    {
        _model = model;
        _view = view;
        _gameModel = gameModel;
        _winSystem = winSystem;
        _dayCycleService = dayCycleService;

        Initialize();
    }

    private void Initialize()
    {
        _view.SetHealth(_model.Health.Value);
        _model.CurrentRoomModel.Subscribe(OnRoomChanged).AddTo(_disposables);
        _model.Health.Subscribe(OnHealthChanged).AddTo(_disposables);
        _model.Health.Subscribe(_view.UpdateHealth).AddTo(_disposables);

        _dayCycleService.IsTimeStop.Subscribe(TimeStop).AddTo(_disposables);
    }

    private void TimeStop(bool isTimeStop)
    {
        if (_isDisposed)
            return;

        _isTimeStop = isTimeStop;

        _view.SetPause(!isTimeStop);

        if (isTimeStop)
        {
            _tweener.Pause();
            
        }
        else
        {
            _tweener.Play();
        }
    }

    private void OnHealthChanged(int health)
    {
        if (health <= 0 && !_isDisposed)
        {
            _winSystem.AddWinPoint(EventType.DieHero);
            isDie = true;
            Dispose();
        }
    }

    private async void OnRoomChanged(RoomModel room)
    {
        if (_roomModel == null && room == null)
            return;

        if (room != null)
        {
            _roomsToExit.Add(room);
        }

        _model.HeroIsReady.Value = false;

        if (_roomModel != null)
        {
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

        if (_isDisposed) return;

        if(room == null)
        {
            await MoveToExit();

            if (_model.Faction == Faction.Player)
                _winSystem.AddWinPoint(EventType.CleanDangeon);
            else
            {

            }

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

        if (_roomModel != null)
            _roomModel.Enter.Value = false;

        _roomModel = room;

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

        room.Enter.Value = false;
    }

    private async UniTask MoveToExit()
    {
        float duration = 1f;

        if (_isDisposed) return;

        _view.SetMoving(true);
        for(int i = _roomsToExit.Count - 1; i > 0; i--)
        {
            _roomsToExit[i - 1].AddHeroView.Value = _view;

            if(i > 0)
            {
                duration = Mathf.Abs(_roomsToExit[i].Position.x - _roomsToExit[i - 1].Position.x);

                if(duration == 0)
                    duration = Mathf.Abs(_roomsToExit[i].Position.y - _roomsToExit[i - 1].Position.y);
            }

            await MoveToPosition(duration);
        }
        _view.SetMoving(false);
    }

    private async UniTask MoveToPosition(float duration = 1f)
    {
        if (_isDisposed) return;

        RotateHero();

        _tweener = _view.transform.DOLocalMove(Vector3.zero, duration)
            .SetEase(Ease.Flash);

        await _tweener.AsyncWaitForCompletion();

        if (!_isDisposed)
        {
            _view.transform.localPosition = Vector3.zero;
        }

        RotateHero();
    }

    private void RotateHero()
    {
        if (_view.transform.localPosition.x <= 0 && _view.transform.rotation.eulerAngles.y > 0)
        {
            _view.transform.DORotate(Vector3.zero, 0.2f);
        }
        else if(_view.transform.localPosition.x > 0 && _view.transform.rotation.eulerAngles.y <= 0)
        {
            _view.transform.DORotate(Vector3.up * 180, 0.2f);
        }
    }

    private async UniTask FightMonsters(RoomModel room)
    {
        if (_isDisposed) return;

        _view.SetFighting();

        foreach (var monster in room.Monsters.FindAll(x => x.Health.CurrentValue > 0))
        {
            while (monster.Health.Value > 0)
            {
                if (_isDisposed) break;

                monster.Health.Value -= _model.Damage.Value;

                _isBattle = true;

                await UniTask.Delay(_model.DamageSpead.Value);

                if (_isDisposed) break;

                await UniTask.WaitUntil(() => !_isTimeStop);
            }
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