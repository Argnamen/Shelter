using R3;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class MonsterSpawner
{
    private readonly DungeonView _dungeonView;
    private readonly DiContainer _container;
    private readonly GameData _gameData;
    private readonly RoomModel _roomModel;
    private readonly MonsterFactory _monsterFactory;

    private CompositeDisposable _spawnDisposables = new();

    public MonsterSpawner(
        GameModel gameModel,
        DungeonView dungeonView,
        DiContainer container,
        GameData gameData,
        MonsterFactory monsterFactory)
    {
        _dungeonView = dungeonView;
        _container = container;
        _gameData = gameData;
        _monsterFactory = monsterFactory;

    }

    public void SpawnWave(MonsterType type, int roomIndex)
    {
        int monsterCount = _roomModel.Monsters.Count;

        int health = 100;

        for (int i = 0; i < monsterCount; i++)
        {
            SpawnWithDelay(type, 0.5f, health, roomIndex);
        }
    }

    private void SpawnWithDelay(MonsterType type, float delay, int health, int roomIndex)
    {
        Observable.Timer(TimeSpan.FromSeconds(delay))
            .Subscribe(_ => Spawn(type, health, roomIndex, delay))
            .AddTo(_spawnDisposables);
    }

    public MonsterModel Spawn(MonsterType type, int health, int roomIndex, float delay, RoomModel roomModel = null, Action<MonsterView> action = null)
    {
        Debug.Log("Spawning monster...");

        var model = new MonsterModel(type, health, delay, roomIndex);

        var view = _monsterFactory.Create(type);
        if (view == null)
        {
            Debug.LogError("Failed to create monster view!");
            return null;
        }

        if (action != null)
        {
            action.Invoke(view);
        }

        view.transform.localPosition = Vector3.zero;

        _container.Instantiate<MonsterPresenter>(new object[] {
           type, model, view, roomModel, _gameData
        });

        return model;
    }

    public void Remove(MonsterModel monster)
    {
        _roomModel.Monsters.Remove(monster);
    }

    public void StartAutoSpawning(float interval, int roomIndex)
    {
        StopAutoSpawning();

        //Observable.Interval(TimeSpan.FromSeconds(interval))
            //.Subscribe(_ => SpawnWave(roomIndex))
            //.AddTo(_spawnDisposables);
    }

    public void StopAutoSpawning()
    {
        _spawnDisposables.Clear();
    }

    public void Dispose()
    {
        _spawnDisposables.Dispose();
    }
}
