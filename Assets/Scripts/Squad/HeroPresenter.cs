using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class HeroPresenter : IDisposable
{
    private readonly HeroModel _model;
    private readonly HeroView _view;
    private readonly GameModel _gameModel;
    private readonly WinSystem _winSystem;
    private readonly DayCycleService _dayCycleService;

    private RoomModel _roomModel;
    private List<RoomModel> _roomsToExit = new List<RoomModel>();
    private Sequence _tweener;
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

    public void Initialize()
    {
        _view.SetHealth(_model.Health.Value);
        _model.CurrentRoomModel.Subscribe(OnRoomChanged).AddTo(_disposables);
        _model.Health.Subscribe(OnHealthChanged).AddTo(_disposables);
        _model.Health.Subscribe(_view.UpdateHealth).AddTo(_disposables);

        _model.MoveToExit.Subscribe(MoveToExit).AddTo(_disposables);

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

        if(room != null)
            _roomsToExit.Add(room);

        _model.HeroIsReady.Value = false;

        if (_roomModel != null)
        {
            switch (_roomModel.Type)
            {
                case RoomType.Treasure:
                    _winSystem.AddWinPoint(EventType.OpenChest);
                    break;
                case RoomType.Combat:
                    if (_isBattle)
                        _winSystem.AddWinPoint(EventType.WinBattle);
                    break;
            }
        }

        if (_isDisposed) return;

        if (_roomModel != null)
        {
            _roomModel.AddHeroView.Value = null;
        }
        room.AddHeroView.Value = _view;

        await MoveToRoom(room);


        _model.HeroIsReady.Value = true;
    }
    private async UniTask MoveToRoom(RoomModel room)
    {
        if (_isDisposed) return;

        _model.State.Value = HeroState.Entering;

        _roomModel = room;

        _view.SetMoving(true);

        await MoveToPosition();

        _view.SetMoving(false);

        switch (room.Type)
        {
            case RoomType.Combat:
                _model.State.Value = HeroState.Fighting;
                await FightMonsters(room);
                break;
            case RoomType.Rest:
                _model.State.Value = HeroState.Resting;
                await RestInRoom(room);
                break;
            case RoomType.Treasure:
                _model.State.Value = HeroState.Resting;
                await LootTreasure(room);
                break;
        }
    }

    private async void MoveToExit(bool isExit)
    {
        float duration = 1f;

        if (_isDisposed || _roomsToExit.Count == 0 || !isExit) return;

        _view.SetMoving(true);

        ClearRoomsToExit();

        for (int i = _roomsToExit.Count - 1; i > 0; i--)
        {
            _roomsToExit[i - 1].AddHeroView.Value = _view;

            if (i > 0)
            {
                duration = Mathf.Abs(_roomsToExit[i].Position.x - _roomsToExit[i - 1].Position.x);

                if(duration == 0)
                    duration = Mathf.Abs(_roomsToExit[i].Position.y - _roomsToExit[i - 1].Position.y);
            }

            await MoveToPosition(duration);
        }
        _view.SetMoving(false);

        if (_model.Faction == Faction.Player)
            _winSystem.AddWinPoint(EventType.CleanDangeon);

        _model.State.Value = HeroState.Leaving;
    }

    private void ClearRoomsToExit()
    {
        var stairs = _roomsToExit.FindLast(x => x.Type == RoomType.Stairs);
        var lastRoom = _roomsToExit[_roomsToExit.Count - 1];
        List<RoomModel> retRoooms = new List<RoomModel>();
        try
        {
            retRoooms = _roomsToExit.FindAll(x => (x.Position.y == lastRoom.Position.y || x.Position.y == 0) && x.Position.x <= stairs.Position.x);
        }
        catch
        {
            retRoooms = _roomsToExit;
        }

        _roomsToExit = retRoooms;
    }

    private async UniTask MoveToPosition(float duration = 1f)
    {
        if (_isDisposed) return;

        RotateHero();

        _tweener = DOTween.Sequence();

        _tweener.Append( _view.transform.DOLocalMove(Vector3.zero, duration)
            .SetEase(Ease.Flash));

        if (_view.GerSpriteRender().color.a <= 0)
        {
            _tweener.Append(_view.GerSpriteRender().DOFade(1, 0.2f)).SetEase(Ease.Flash);
            _tweener.AppendCallback(() => _view.HealthVisible(true));
        }

        await _tweener.AsyncWaitForCompletion();

        if (!_isDisposed)
        {
            _view.transform.localPosition = Vector3.zero;
        }

        RotateHero();
    }

    private void RotateHero()
    {
        var rotate = _view.transform.localRotation;
        if (_view.transform.localPosition.x <= 0 && _view.transform.rotation.eulerAngles.y > 0)
        {
            rotate.eulerAngles = Vector3.zero;
        }
        else if(_view.transform.localPosition.x > 0 && _view.transform.rotation.eulerAngles.y <= 0)
        {
            rotate.eulerAngles = Vector3.up * 180;
        }

        _view.transform.localRotation = rotate;
    }

    private async UniTask FightMonsters(RoomModel room)
    {
        if (_isDisposed) return;

        if(room.Monsters.FindAll(x => x.Health.CurrentValue > 0).Count > 0)
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