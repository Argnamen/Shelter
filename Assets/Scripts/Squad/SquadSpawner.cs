using R3;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class SquadSpawner
{
    private readonly GameModel _gameModel;
    private readonly DungeonView _dungeonView;
    private readonly DiContainer _container;
    private readonly GridService _gridService;
    private readonly GameData _gameData;
    private readonly DayCycleService _dayCycleService;
    private readonly HeroesData _heroesData;
    private readonly WinSystem _winSystem;

    private float[] _spawnTime = new float[4];

    private CompositeDisposable _spawnDisposables = new();

    public SquadSpawner(
        GameModel gameModel,
        DungeonView dungeonView,
        DiContainer container,
        GridService gridService,
        GameData gameData,
        DayCycleService dayCycleService,
        HeroesData heroesData,
        WinSystem winSystem)
    {
        _gameModel = gameModel;
        _dungeonView = dungeonView;
        _container = container;
        _gridService = gridService;
        _gameData = gameData;
        _dayCycleService = dayCycleService;
        _heroesData = heroesData;
        _winSystem = winSystem;

        _dayCycleService.Time.Subscribe(StartAutoSpawning).AddTo(_spawnDisposables);
    }

    public void SpawnHeroWave(Faction faction)
    {
        SpawnHeroWithDelay(faction, 0.5f);
    }

    private void SpawnHeroWithDelay(Faction faction, float delay)
    {
        Observable.Timer(TimeSpan.FromSeconds(delay))
            .Subscribe(_ => SpawnSquad(faction))
            .AddTo(_spawnDisposables);
    }

    private void SpawnSquad(Faction faction)
    {
        Debug.Log("Spawning squad...");

        int heroCount = _gameModel.GetSquadSpawnCount();
        var heroes = new List<HeroModel>();
        SquadHeroModel squad;

        for (int i = 0; i < heroCount; i++)
        {
            var heroData = _heroesData.Heroes[UnityEngine.Random.Range(0, _heroesData.Heroes.Count)];
            var goldWithYou = UnityEngine.Random.Range(0, 100 + 1);
            var heroModel = new HeroModel(faction, heroData.Class, heroData.Health, heroData.Damage, heroData.DamageSpeadMillisecond, goldWithYou);

            var heroView = _dungeonView.CreateHeroView(heroData.Prefab, faction);

            if (heroView == null)
            {
                Debug.LogError("Failed to create hero view!");
                return;
            }

            // Устанавливаем начальную позицию за пределами сетки
            heroView.transform.position = ((Vector3Int)_gameData.StartHeroPosition);

            heroes.Add(heroModel);

            _container.Instantiate<HeroPresenter>(new object[] {
            heroModel, heroView, _gameModel, _winSystem
        });
        }
        squad = new SquadHeroModel(faction, heroes);

        _gameModel.AddSquad(squad);

        _container.Instantiate<SquadHeroPresenter>(new object[] {
            squad, _gameModel, _gridService, this
        });
    }

    public void RemoveSquad(SquadHeroModel squad)
    {
        _gameModel.RemoveSquad(squad);
    }

    public void StartAutoSpawning(float time)
    {
        for (int i = 0; i < 4; i++)
        {
            if (time <= 0)
            {
                _spawnTime[i] = 0;
                return;
            }

            if (time >= _spawnTime[i] && _spawnTime[i] <= _gameData.TimeDaySecond)
            {
                float interval = _gameModel.GetSquadSpawnInterval();

                _spawnTime[i] += interval;

                if (_spawnTime[i] <= _gameData.TimeDaySecond)
                {
                    SpawnHeroWave((Faction)i);
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
