using R3;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayCycleService
{
    public ReactiveProperty<float> Time { get; } = new(0);

    public ReactiveProperty<bool> IsTimeStop { get; } = new(true);

    private int _time;
    private float _lengthDay;

    private CompositeDisposable _cycleDisposables = new();

    public DayCycleService(GameData gameData)
    {
        _time = gameData.TimeDaySecond;
        _lengthDay = gameData.LengthDay;
    }

    public void StartDay()
    {
        IsTimeStop.Value = false;
        Observable.Interval(TimeSpan.FromSeconds(_lengthDay / _time))
            .Subscribe(_ => DayCycle())
            .AddTo(_cycleDisposables);
    }

    public void StopDay()
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
}
