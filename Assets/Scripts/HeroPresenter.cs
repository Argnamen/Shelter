using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Collections.Generic;
using UnityEngine;

public class HeroPresenter : IDisposable
{
    private readonly HeroModel _model;
    private readonly HeroView _view;
    private readonly GameModel _gameModel;
    private readonly GridService _gridService;
    private readonly HeroSpawner _heroSpawner;
    private readonly GameData _gameData;

    private RoomModel _roomModel;

    private CompositeDisposable _disposables = new();
    private bool _isDisposed = false;

    private Vector2Int _moveVector = Vector2Int.right;

    private List<Vector2Int> _cleanRooms = new();

    public HeroPresenter(
        HeroModel model,
        HeroView view,
        GameModel gameModel,
        GridService gridService,
        HeroSpawner heroSpawner,
        GameData gameData)
    {
        _model = model;
        _view = view;
        _gameModel = gameModel;
        _gridService = gridService;
        _heroSpawner = heroSpawner;
        _gameData = gameData;

        Initialize();
    }

    private void Initialize()
    {
        InitCleanRooms();
        _model.CurrentRoomIndex.Subscribe(OnRoomChanged).AddTo(_disposables);
        _model.Health.Subscribe(OnHealthChanged).AddTo(_disposables);
    }

    private void InitCleanRooms()
    {
        foreach (var room in _gameModel.Rooms)
        {
            if(room.IsUnlocked)
                _cleanRooms.Add(room.Position);
        }
    }

    private async void OnRoomChanged(int roomIndex)
    {
        if (_isDisposed) return;

        var room = FindRoom();

        if (_cleanRooms.Count == 0 || room == null)
        {
            await MoveToExit();
            Dispose();
            return;
        }

        if (_roomModel != null)
            _roomModel.Heroes.Remove(_model);

        if (_cleanRooms == null || _cleanRooms.Count == 0)
        {
            await MoveToExit();
            Dispose();
            return;
        }

        _cleanRooms.Remove(room.Position);

        _roomModel = room;

        await MoveToRoom(room);

        switch (room.Type)
        {
            case RoomType.Combat:
                await FightMonsters(room);
                break;
            case RoomType.Rest:
                await RestInRoom(room);
                break;
            case RoomType.Treasure:
                await LootTreasure(room);
                break;
        }

        // Переходим к следующей комнате
        _model.CurrentRoomIndex.Value++;
    }

    private RoomModel FindRoom()
    {
        var room = _gameModel.Rooms[0];

        if (_roomModel == null)
        {
            return room;
        }
        else
        {
            var nextRoom = _gridService.GetRoomAt(_roomModel.Position + _moveVector);

            if (_roomModel.Type == RoomType.Stairs)
            {
                if (_cleanRooms.FindAll(x => x.y == _roomModel.Position.y).Count != 0)
                {
                    if (nextRoom != null)
                    {
                        return nextRoom;
                    }

                    _moveVector *= -1;
                    return _gridService.GetRoomAt(_roomModel.Position + _moveVector);

                }

                if (_cleanRooms.FindAll(x => x.y == _roomModel.Position.y - 1).Count != 0)
                {
                    nextRoom = _gridService.GetRoomAt(_roomModel.Position + Vector2Int.down);

                    if (nextRoom != null)
                    {
                        if (nextRoom.Type == RoomType.Stairs)
                        {
                            _moveVector = UnityEngine.Random.Range(0, 2) == 0 ? Vector2Int.right : Vector2Int.left;

                            return nextRoom;
                        }
                        else
                        {
                            return _gridService.GetRoomAt(_roomModel.Position + _moveVector);
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
                return _gridService.GetRoomAt(_roomModel.Position + _moveVector);
            }


        }
    }

    private void OnHealthChanged(int health)
    {
        if (health <= 0 && !_isDisposed)
        {
            Die();
        }
    }

    private async UniTask MoveToRoom(RoomModel room)
    {
        if (_isDisposed) return;

        _roomModel.Enter.Value = false;

        _view.SetMoving(true);

        var roomPosition = _gridService.GetRoomPosition(room);

        room.Heroes.Add(_model);

        if (roomPosition.HasValue)
        {
            var worldPosition = GetWorldPosition(roomPosition.Value);
            await MoveToPosition(worldPosition);
        }

        _view.SetMoving(false);

        _roomModel.Enter.Value = true;
    }

    private async UniTask MoveToExit()
    {
        if (_isDisposed) return;

        _roomModel.Heroes.Remove(_model);

        _view.SetMoving(true);
        await MoveToPosition(new Vector3(_gameData.StartHeroPosition.x, _gameData.StartHeroPosition.y, 0));
        _view.SetMoving(false);
    }

    private async UniTask MoveToPosition(Vector3 targetPosition)
    {
        if (_isDisposed) return;

        float duration = 1f;
        float elapsed = 0f;
        Vector3 startPosition = _view.transform.position;

        while (elapsed < duration && !_isDisposed)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            _view.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            await UniTask.Yield();
        }

        if (!_isDisposed)
        {
            _view.transform.position = targetPosition;
        }
    }

    private async UniTask FightMonsters(RoomModel room)
    {
        if (_isDisposed || room.Monsters.Count == 0) return;

        _view.SetFighting();
        await UniTask.Delay(500);

        foreach (var monster in room.Monsters.ToArray()) // Используем ToArray чтобы избежать модификации коллекции
        {
            while (monster.Health.Value > 0)
            {
                if (_isDisposed) break;

                monster.Health.Value -= 10;
                await UniTask.Delay(300);
            }

            if (_isDisposed) break;
        }

        _view.SetFighting(false);
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

    private async UniTask LootTreasure(RoomModel room)
    {
        if (_isDisposed) return;

        await UniTask.Delay(1000);

        if (!_isDisposed)
        {
            // Награда за сундук
            int goldReward = UnityEngine.Random.Range(10, 30);
            _gameModel.Gold.Value += goldReward;
        }
    }

    private void Die()
    {
        if (_isDisposed) return;

        _view.Die();
        Dispose();
    }

    private Vector3 GetWorldPosition(Vector2Int gridPosition)
    {
        return new Vector3(
            gridPosition.x * _gameData.CellSize,
            gridPosition.y * _gameData.CellSize,
            0
        );
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        _isDisposed = true;
        _disposables.Dispose();
        _heroSpawner.RemoveHero(_model);

        if (_view != null)
        {
            _view.Die();
        }
    }
}