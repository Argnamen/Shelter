using R3;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class HeroSpawner
{
    private readonly GameModel _gameModel;
    private readonly DungeonView _dungeonView;
    private readonly DiContainer _container;
    private readonly GridService _gridService;
    private readonly GameData _gameData;
    private readonly DayCycleService _dayCycleService;
    private readonly HeroesData _heroesData;

    private float _spawnTime = 0;

    private CompositeDisposable _spawnDisposables = new();

    public HeroSpawner(
        GameModel gameModel,
        DungeonView dungeonView,
        DiContainer container,
        GridService gridService,
        GameData gameData,
        DayCycleService dayCycleService,
        HeroesData heroesData)
    {
        _gameModel = gameModel;
        _dungeonView = dungeonView;
        _container = container;
        _gridService = gridService;
        _gameData = gameData;
        _dayCycleService = dayCycleService;
        _heroesData = heroesData;

        _dayCycleService.Time.Subscribe(StartAutoSpawning).AddTo(_spawnDisposables);
    }

    public void SpawnHeroWave()
    {
        SpawnHeroWithDelay(0.5f);
    }

    private void SpawnHeroWithDelay(float delay)
    {
        Observable.Timer(TimeSpan.FromSeconds(delay))
            .Subscribe(_ => SpawnSquad())
            .AddTo(_spawnDisposables);
    }

    private void SpawnSquad()
    {
        Debug.Log("Spawning squad...");

        int heroCount = _gameModel.GetSquadSpawnCount();
        var heroes = new List<HeroModel>();

        for (int i = 0; i < heroCount; i++)
        {
            var heroData = _heroesData.Heroes[UnityEngine.Random.Range(0, _heroesData.Heroes.Count)];
            var heroModel = new HeroModel(heroData.Class, heroData.Health, heroData.Damage, heroData.DamageSpeadMillisecond);

            _gameModel.AddHero(heroModel);

            var heroView = _dungeonView.CreateHeroView(heroData.Prefab);

            if (heroView == null)
            {
                Debug.LogError("Failed to create hero view!");
                _gameModel.RemoveHero(heroModel);
                return;
            }

            // Устанавливаем начальную позицию за пределами сетки
            heroView.transform.position = ((Vector3Int)_gameData.StartHeroPosition);

            heroes.Add(heroModel);

            _container.Instantiate<HeroPresenter>(new object[] {
            heroModel, heroView, _gameModel, _gridService, this
        });
        }

        _container.Instantiate<SquadHeroPresenter>(new object[] {
            new SquadHeroModel(heroes), _gameModel, _gridService, this
        });
    }

    public void RemoveHero(HeroModel hero)
    {
        _gameModel.RemoveHero(hero);
    }

    public void StartAutoSpawning(float time)
    {
        if (time <= 0)
        {
            _spawnTime = 0;
            return;
        }

        float interval = _gameModel.GetHeroSpawnInterval();

        _spawnTime += interval;

        if (_spawnTime <= _gameData.TimeDaySecond)
        {
            Observable.Timer(TimeSpan.FromSeconds(_spawnTime))
                .Subscribe(_ => SpawnHeroWave())
                .AddTo(_spawnDisposables);
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
