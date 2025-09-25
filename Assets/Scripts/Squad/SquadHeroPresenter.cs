using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquadHeroPresenter : IDisposable
{
    private readonly GameModel _gameModel;
    private readonly GridService _gridService;
    private readonly SquadSpawner _squadSpawner;
    private readonly GameData _gameData;
    private readonly SquadHeroModel _model;

    private RoomModel _roomModel;

    private CompositeDisposable _disposables = new();
    private bool _isDisposed = false;

    private Vector2Int _moveVector = Vector2Int.right;

    private List<Vector2Int> _cleanRooms = new();

    public SquadHeroPresenter(SquadHeroModel model,
        GameModel gameModel,
        GridService gridService,
        SquadSpawner squadSpawner,
        GameData gameData)
    {
        _model = model;
        _gameModel = gameModel;
        _gridService = gridService;
        _gameData = gameData;
        _squadSpawner = squadSpawner;

        Initialize();
    }

    private void Initialize()
    {
        InitCleanRooms();

        _model.HeroesIsReady.Subscribe((x) => OnRoomChanged(x, _model.Faction)).AddTo(_disposables);
        _model.Count.Subscribe(Die).AddTo(_disposables);
    }

    private void InitCleanRooms()
    {
        for (int i = 0; i < 4; i++)
        {
            foreach (var room in _gameModel.Rooms[(Faction)i])
            {
                if (room.IsUnlocked)
                    _cleanRooms.Add(room.Position);
            }
        }
    }

    private void OnRoomChanged(bool isReady, Faction faction)
    {
        if (_isDisposed) return;

        if (!isReady) return;

        var room = FindRoom(faction);

        if (_cleanRooms.Count == 0 || room == null)
        {
            NotifyAllHeroesOnRoomChanged(null);
            Dispose();
            return;
        }

        if (_roomModel != null)
            _roomModel.Squad = null;

        _cleanRooms.Remove(room.Position);

        _roomModel = room;

        NotifyAllHeroesOnRoomChanged(room);

        room.Squad = _model;
    }

    private void NotifyAllHeroesOnRoomChanged(RoomModel room)
    {
        foreach (var hero in _model.Heroes) 
        {
            hero.CurrentRoomModel.Value = room;
        }
    }

    private RoomModel FindRoom(Faction faction)
    {
        var room = _gameModel.Rooms[faction][0];

        if (_roomModel == null)
        {
            return room;
        }
        else
        {
            var nextRoom = _gridService.GetRoomAt(_roomModel.Position + _moveVector, faction);

            if (_roomModel.Type == RoomType.Stairs)
            {
                if (_cleanRooms.FindAll(x => x.y == _roomModel.Position.y).Count != 0)
                {
                    if (nextRoom != null)
                    {
                        return nextRoom;
                    }

                    _moveVector *= -1;
                    return _gridService.GetRoomAt(_roomModel.Position + _moveVector, faction);

                }

                if (_cleanRooms.FindAll(x => x.y == _roomModel.Position.y - 1).Count != 0)
                {
                    nextRoom = _gridService.GetRoomAt(_roomModel.Position + Vector2Int.down, faction);

                    if (nextRoom != null)
                    {
                        if (nextRoom.Type == RoomType.Stairs)
                        {
                            _moveVector = UnityEngine.Random.Range(0, 2) == 0 ? Vector2Int.right : Vector2Int.left;

                            return nextRoom;
                        }
                        else
                        {
                            return _gridService.GetRoomAt(_roomModel.Position + _moveVector, faction);
                        }
                    }
                }

                return null;
            }
            else
            {
                if (nextRoom != null)
                {
                    return nextRoom;
                }

                _moveVector *= -1;
                return _gridService.GetRoomAt(_roomModel.Position + _moveVector, faction);
            }


        }
    }

    private void Die(int count)
    {
        if(count <= 0)
        {
            Dispose();
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        _isDisposed = true;
        _disposables.Dispose();
        _squadSpawner.RemoveSquad(_model);
        _roomModel.Squad = null;
        _roomModel.Enter.Value = false;
    }
}
