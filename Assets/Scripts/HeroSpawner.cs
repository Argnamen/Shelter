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

    private CompositeDisposable _spawnDisposables = new();

    public HeroSpawner(
        GameModel gameModel,
        DungeonView dungeonView,
        DiContainer container,
        GridService gridService)
    {
        _gameModel = gameModel;
        _dungeonView = dungeonView;
        _container = container;
        _gridService = gridService;
    }

    public void SpawnHeroWave()
    {
        int heroCount = _gameModel.GetHeroSpawnCount();

        for (int i = 0; i < heroCount; i++)
        {
            SpawnHeroWithDelay(i * 0.5f); // Задержка между спавном героев
        }
    }

    private void SpawnHeroWithDelay(float delay)
    {
        Observable.Timer(TimeSpan.FromSeconds(delay))
            .Subscribe(_ => SpawnHero())
            .AddTo(_spawnDisposables);
    }

    private void SpawnHero()
    {
        Debug.Log("Spawning hero...");

        var heroModel = new HeroModel();
        _gameModel.AddHero(heroModel);

        var heroView = _dungeonView.CreateHeroView();
        if (heroView == null)
        {
            Debug.LogError("Failed to create hero view!");
            _gameModel.RemoveHero(heroModel);
            return;
        }

        // Устанавливаем начальную позицию за пределами экрана
        heroView.transform.position = new Vector3(-5, 2, 0);

        heroModel.CurrentRoomIndex.Value = 0;

        _container.Instantiate<HeroPresenter>(new object[] {
            heroModel, heroView, _gameModel, _gridService, this
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
