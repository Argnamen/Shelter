using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterPresenter : IDisposable
{
    public MonsterType Type { get => _type; }

    private readonly MonsterType _type;
    private readonly MonsterView _view;
    private readonly MonsterModel _model;
    private readonly GameData _gameData;
    private readonly RoomModel _roomModel;

    private CompositeDisposable _disposables = new();
    private bool _isDisposed = false;
    public MonsterPresenter(MonsterType type, MonsterView view, MonsterModel model, RoomModel roomModel, GameData gameData, MonsterFactory monsterFactory)
    {
        _type = type;
        _view = view;
        _model = model;
        _roomModel = roomModel;
        _gameData = gameData;

        Initialize();
    }

    private void Initialize()
    {
        _roomModel.Enter.Subscribe(OnRoomEnter).AddTo(_disposables);
        _model.Health.Subscribe(OnHealthChanged).AddTo(_disposables);
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
    }

    private async UniTask FightHeroes()
    {
        if (_isDisposed) return;

        _view.SetFighting();
        await UniTask.Delay(500);

        foreach (var squad in _roomModel.Squad.ToArray()) // Используем ToArray чтобы избежать модификации коллекции
        {
            var hero = squad.GetHero();

            while (hero.Health.Value > 0)
            {
                if (_isDisposed) break;

                hero.Health.Value -= _model.Damage.Value;

                await UniTask.Delay(_model.DamageSpeadMillisecond.Value);
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

        _roomModel.Monsters.Remove(_model);

        _isDisposed = true;
        _disposables.Dispose();

        if (_view != null)
        {
            _view.Die();
        }
    }
}
