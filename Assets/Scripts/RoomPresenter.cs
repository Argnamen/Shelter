using R3;
using System;
using UnityEngine;

public class RoomPresenter : IDisposable
{
    private readonly RoomModel _model;
    private readonly RoomView _view;

    private CompositeDisposable _disposables = new();

    public RoomPresenter(RoomModel model, RoomView view)
    {
        _model = model;
        _view = view;

        Spawn();
    }

    private void Spawn()
    {
        _model.Destroy.Subscribe(Destroy).AddTo(_disposables);
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