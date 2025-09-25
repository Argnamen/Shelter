using Cysharp.Threading.Tasks;
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
    private readonly DayCycleService _dayCycleService;
    private readonly GameModel _gameModel;
    private readonly MonstersData _monstersData;

    private CompositeDisposable _spawnDisposables = new();

    public MonsterSpawner(
        GameModel gameModel,
        DungeonView dungeonView,
        DiContainer container,
        GameData gameData,
        MonsterFactory monsterFactory,
        DayCycleService dayCycleService,
        MonstersData monstersData)
    {
        _dungeonView = dungeonView;
        _container = container;
        _gameData = gameData;
        _monsterFactory = monsterFactory;
        _dayCycleService = dayCycleService;
        _gameModel = gameModel;
        _monstersData = monstersData;

        _dayCycleService.Time.Subscribe(StartAutoSpawning).AddTo(_spawnDisposables);
    }
    public void Spawn(MonsterType type, RoomModel roomModel)
    {
        Debug.Log("Spawning monster...");

        var monsterData = _monstersData.Monsters.Find(monster => monster.Type == type);

        var model = new MonsterModel(type, monsterData.Health, monsterData.Damage, monsterData.DamageSpeadMillisecond);

        var view = _monsterFactory.Create(type);

        if (view == null)
        {
            Debug.LogError("Failed to create monster view!");
            return;
        }

        roomModel.Monsters.Add(model);
        roomModel.AddMonsterView.Value = view;

        _container.Instantiate<MonsterPresenter>(new object[] {
           type, model, view, roomModel, _gameData
        });
    }

    public void Remove(MonsterModel monster)
    {
        _roomModel.Monsters.Remove(monster);
    }

    public async void StartAutoSpawning(float time)
    {
        if (time <= 0)
        {
            for (int i = 0; i < 4; i++)
            {
                foreach (var room in _gameModel.Rooms[(Faction)i])
                {
                    await new WaitWhile(() => room.Enter.Value);

                    if (room.Type == RoomType.Combat && room.Monsters.Count < 3)
                    {
                        Spawn(room.Monster, room);
                    }
                }
            }
        }
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
