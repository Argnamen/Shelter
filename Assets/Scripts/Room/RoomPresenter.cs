using R3;
using System;
using UnityEngine;

public class RoomPresenter : IDisposable
{
    private readonly RoomModel _model;
    private readonly RoomView _view;
    private readonly DayCycleService _dayCycleService;

    private CompositeDisposable _disposables = new();

    public RoomPresenter(RoomModel model, RoomView view, DayCycleService dayCycleService)
    {
        _model = model;
        _view = view;
        _dayCycleService = dayCycleService;

        Spawn();
    }

    private void Spawn()
    {
        _model.Destroy.Subscribe(Destroy).AddTo(_disposables);

        _model.AddMonsterView.Subscribe(_view.AddMonsterView).AddTo(_disposables);
        _model.AddHeroView.Subscribe(_view.AddHeroView).AddTo(_disposables);

        _model.RoomStateMove.Subscribe(_view.RandomMovingInRoom).AddTo(_disposables);
        _model.RoomStateFight.Subscribe(_view.ResetMoving).AddTo(_disposables);

        _dayCycleService.IsTimeStop.Subscribe(_view.TimeStop).AddTo(_disposables);
    }
    private void Destroy(bool destroyed)
    {
        if (destroyed)
        {
            _view.DestroyRoom();
            Dispose();
        }
    }

    private void Enter()
    {

    }

    public void Remove()
    {
        GameObject.Destroy(_view.gameObject);
        Dispose();
    }

    // Можно добавить логику комнаты здесь, если понадобится
    // Например, обработка входа/выхода героев

    public void Dispose()
    {
        _disposables.Dispose();
    }
}