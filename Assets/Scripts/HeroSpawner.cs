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

    private CompositeDisposable _spawnDisposables = new();

    public HeroSpawner(
        GameModel gameModel,
        DungeonView dungeonView,
        DiContainer container,
        GridService gridService,
        GameData gameData)
    {
        _gameModel = gameModel;
        _dungeonView = dungeonView;
        _container = container;
        _gridService = gridService;
        _gameData = gameData;
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
            var heroModel = new HeroModel(HeroClass.Damager, 100);

            _gameModel.AddHero(heroModel);

            var heroView = _dungeonView.CreateHeroView();
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

    public void StartAutoSpawning()
    {
        StopAutoSpawning();

        float interval = _gameModel.GetHeroSpawnInterval();
        Observable.Interval(TimeSpan.FromSeconds(interval))
            .Subscribe(_ => SpawnHeroWave())
            .AddTo(_spawnDisposables);
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
