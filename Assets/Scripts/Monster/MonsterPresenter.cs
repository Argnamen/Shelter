using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class MonsterPresenter : IDisposable
{
    public MonsterType Type { get => _type; }

    private readonly MonsterType _type;
    private readonly MonsterView _view;
    private readonly MonsterModel _model;
    private readonly RoomModel _roomModel;
    private readonly DayCycleService _dayCycleService;
    private bool _isTimeStop;

    private CompositeDisposable _disposables = new();
    private bool _isDisposed = false;
    public MonsterPresenter(MonsterType type, MonsterView view, MonsterModel model, RoomModel roomModel, MonsterFactory monsterFactory, DayCycleService dayCycleService)
    {
        _type = type;
        _view = view;
        _model = model;
        _roomModel = roomModel;
        _dayCycleService = dayCycleService;
        Initialize();
    }

    public void Initialize()
    {
        _roomModel.Enter.Subscribe(OnRoomEnter).AddTo(_disposables);
        _model.Health.Subscribe(OnHealthChanged).AddTo(_disposables);
        _dayCycleService.IsTimeStop.Subscribe(OnTimeStop).AddTo(_disposables);
    }

    private void OnTimeStop(bool isTimeStop)
    {
        if (_isDisposed) return;

        _isTimeStop = isTimeStop;

        _view.SetPause(!isTimeStop);
    }

    private async void OnRoomEnter(bool isEnter)
    {
        if (isEnter)
        {
            Debug.Log("Fight!!!!!");
            await FightHeroes();
        }
    }

    private void OnHealthChanged(int health)
    {
        if (health <= 0 && !_isDisposed)
        {
            Die();
        }
        else if (_isDisposed)
        {
            Rest();
        }
    }

    private async UniTask FightHeroes()
    {
        if (_isDisposed) return;

        _view.Fight();
        while (_roomModel.Squad != null)
        {
            var hero = _roomModel.Squad.GetHero();

            if (hero == null)
                break;

            if (hero.Health.Value > 0)
            {
                if (_isDisposed && hero.Die.Value) break;

                hero.Health.Value -= _model.Damage.Value;

                await UniTask.Delay(_model.DamageSpeadMillisecond.Value);
            }

            if (_isDisposed) break;

            await UniTask.WaitUntil(() => !_isTimeStop);
        }

        _view.Idle();
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

    private void Rest()
    {
        _view.Respawn();
        _isDisposed = false;
    }

    private void Die()
    {
        _view.Die();

        _isDisposed = true;
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        _isDisposed = true;
        _disposables.Dispose();

        if (_view != null)
        {
            _view.Die();
        }
    }
}
