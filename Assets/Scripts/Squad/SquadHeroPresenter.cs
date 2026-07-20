using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

public class SquadHeroPresenter : IDisposable
{
    private readonly GameModel _gameModel;
    private readonly GridService _gridService;
    private readonly SquadSpawner _squadSpawner;
    private readonly SquadHeroModel _model;
    private readonly Vector3 _startDungeonPos;

    private RoomModel _roomModel;

    private CompositeDisposable _disposables = new();
    private bool _isDisposed = false;
    private bool _isStart = true;

    private Vector2Int _moveVector = Vector2Int.right;

    private List<Vector2Int> _cleanRooms = new();

    public SquadHeroPresenter(SquadHeroModel model,
        GameModel gameModel,
        GridService gridService,
        SquadSpawner squadSpawner,
        Vector3 StartDungeonPos)
    {
        _model = model;
        _gameModel = gameModel;
        _gridService = gridService;
        _squadSpawner = squadSpawner;
        _startDungeonPos = StartDungeonPos;
        Initialize();
    }

    public void Initialize()
    {
        InitCleanRooms();

        _model.HeroesIsReady.Subscribe((x) => OnRoomChanged(x, _model.Faction)).AddTo(_disposables);
        _model.Count.Subscribe(Die).AddTo(_disposables);
    }

    private void InitCleanRooms()
    {
        foreach (var room in _gameModel.Rooms[_model.Faction])
        {
            if (room.IsUnlocked)
                _cleanRooms.Add(room.Position);
        }
    }

    private async void OnRoomChanged(bool isReady, Faction faction)
    {
        if (_isDisposed) return;

        if (!isReady) return;

        if (_isStart)
        {
            await GoToEnterTheDungeon();
        }

        var room = FindRoom(faction);

        if (_cleanRooms.Count == 0 || room == null)
        {
            NotifyAllHeroesOnRoomChanged(null);
            Debug.Log($"_cleanRooms.Count = {_cleanRooms.Count} / room == {room == null}");
            return;
        }

        if (_roomModel != null)
            _roomModel.Squad = null;

        _cleanRooms.Remove(room.Position);

        _roomModel = room;

        NotifyAllHeroesOnRoomChanged(room);

        room.Squad = _model;
    }

    private async UniTask GoToEnterTheDungeon()
    {
        Sequence sequence;

        foreach (var heroView in _model.ViewHeroes)
        {
            sequence = DOTween.Sequence();

            heroView.SetMoving(true);

            sequence.AppendCallback(() => heroView.HealthVisible(false));

            sequence.Append(heroView.transform.DOMove(_startDungeonPos, 2.8f).SetEase(Ease.Flash));

            heroView.SetMoving(false);

            sequence.Append(heroView.GerSpriteRender().DOFade(0,0.2f)).SetEase(Ease.Flash);

            sequence.AppendInterval(1f);
            sequence.Append(heroView.GerSpriteRender().DOFade(1, 0.2f)).SetEase(Ease.Flash);
            sequence.AppendCallback(() => heroView.HealthVisible(true));
        }

        await Task.Delay(3 * 1000);

        _isStart = false;
    }

    private void NotifyAllHeroesOnRoomChanged(RoomModel room)
    {
        foreach (var hero in _model.Heroes) 
        {
            hero.CurrentRoomModel.Value = room;
        }

        if(room == null)
        {
            Dispose();
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

                //Лестница ведёт вниз, но её объект контейнер перевернут (на 180 относительно Y)
                if (_cleanRooms.FindAll(x => x.y == _roomModel.Position.y + 1).Count != 0) //По этому мы ищем объекты выше неё
                {
                    nextRoom = _gridService.GetRoomAt(_roomModel.Position + Vector2Int.up, faction); // Потому мы возвращаем комнату стоящую выше

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
