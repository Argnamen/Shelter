using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Linq;
using UnityEngine;

public class HeroPresenter : IDisposable
{
    private readonly HeroModel _model;
    private readonly HeroView _view;
    private readonly GameModel _gameModel;

    private CompositeDisposable _disposables = new();

    public HeroPresenter(
        HeroModel model,
        HeroView view,
        GameModel gameModel)
    {
        _model = model;
        _view = view;
        _gameModel = gameModel;

        Initialize();
    }

    private void Initialize()
    {
        _model.CurrentRoomIndex.Subscribe(OnRoomChanged).AddTo(_disposables);
        _model.Health.Subscribe(health =>
        {
            if (health <= 0)
            {
                Dispose();
            }
        }).AddTo(_disposables);
    }

    private async void OnRoomChanged(int roomIndex)
    {
        if (roomIndex < 0 || roomIndex >= _gameModel.Rooms.Count)
        {
            _view.SetMoving(true);
            await MoveToExit();
            Dispose();
            return;
        }

        var room = _gameModel.Rooms[roomIndex];
        await MoveToRoom(room);

        switch (room.Type)
        {
            case RoomType.Combat:
                await FightMonsters(room);
                break;
            case RoomType.Rest:
                await RestInRoom(room);
                break;
        }

        _model.CurrentRoomIndex.Value++;
    }

    private async UniTask MoveToRoom(RoomModel room)
    {
        _view.SetMoving(true);
        // Implement movement logic
        await UniTask.Delay(1000);
        _view.SetMoving(false);
    }

    private async UniTask FightMonsters(RoomModel room)
    {
        if (room.Monsters.Count == 0) return;

        _view.SetFighting();
        await UniTask.Delay(500);

        foreach (var monster in room.Monsters.ToList())
        {
            monster.Health.Value -= 10;
            _model.Health.Value -= 5;
            await UniTask.Delay(300);
        }
    }

    private async UniTask RestInRoom(RoomModel room)
    {
        await UniTask.Delay(2000);
        _model.Health.Value = Mathf.Min(100, _model.Health.Value + 30);
    }

    private async UniTask MoveToExit()
    {
        // Implement exit movement logic
        await UniTask.Delay(1000);
        _gameModel.ActiveHeroes.Remove(_model);
    }

    public void Dispose()
    {
        _disposables.Dispose();
        UnityEngine.GameObject.Destroy(_view.gameObject);
    }
}
