using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class DayCycleService: IDisposable
{
    public ReactiveProperty<float> Time { get; } = new(0);

    public ReactiveProperty<bool> IsTimeStop { get; } = new(true);

    private int _time;
    private float _lengthDay;

    private CompositeDisposable _cycleDisposables = new();

    private GameModel _gameModel;

    public DayCycleService(GameModel gameModel)
    {
        _gameModel = gameModel;
        Initialize();
    }

    public void Initialize()
    {
        var data = _gameModel.GetPlayer(Faction.Player).Data;
        _time = data.TimeDaySecond;
        _lengthDay = data.LengthDay;
    }

    public void StartDay()
    {
        IsTimeStop.Value = false;
        Observable.Interval(TimeSpan.FromSeconds(_lengthDay / _time))
            .Subscribe(_ => DayCycle())
            .AddTo(_cycleDisposables);
    }

    public async UniTask StopDay()
    {
        IsTimeStop.Value = true;
        _cycleDisposables.Clear();
    }

    private void DayCycle()
    {
        if (Time.Value >= 1)
        {
            Time.Value = 0;

            return;
        }

        Time.Value += 1 / _lengthDay;
    }

    public void Dispose()
    {
        Time.Dispose();
        IsTimeStop.Dispose();
    }
}
