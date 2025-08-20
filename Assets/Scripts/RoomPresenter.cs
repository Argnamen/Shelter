using R3;
using System;

public class RoomPresenter : IDisposable
{
    private readonly RoomModel _model;
    private readonly RoomView _view;

    private CompositeDisposable _disposables = new();

    public RoomPresenter(RoomModel model, RoomView view)
    {
        _model = model;
        _view = view;
    }

    // Можно добавить логику комнаты здесь, если понадобится
    // Например, обработка входа/выхода героев

    public void Dispose()
    {
        _disposables.Dispose();
    }
}