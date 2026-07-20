using Cysharp.Threading.Tasks;
using DG.Tweening;
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
        DayCycleService dayCycleService,
        HeroesData heroesData,
        WinSystem winSystem)
    {
        _gameModel = gameModel;
        _dungeonView = dungeonView;
        _container = container;
        _gridService = gridService;
        _dayCycleService = dayCycleService;
        _heroesData = heroesData;
        _winSystem = winSystem;

        _dayCycleService.Time.Subscribe((x) => StartAutoSpawning(Faction.Player, x)).AddTo(_spawnDisposables);
    }

    public async UniTask SpawnHeroWave(Faction faction)
    {
        SpawnHeroWithDelay(faction, 0.5f);
        await UniTask.WaitForSeconds(0.5f);
    }

    private void SpawnHeroWithDelay(Faction faction, float delay)
    {
        Observable.Timer(TimeSpan.FromSeconds(delay))
            .Subscribe(_ => SpawnSquad(faction))
            .AddTo(_spawnDisposables);
    }

    private async void SpawnSquad(Faction faction)
    {
        Debug.Log("Spawning squad...");

        int heroCount = _gameModel.GetSquadSpawnCount();
        var heroes = new List<HeroModel>();
        var views = new List<HeroView>();
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
            //heroView.transform.position = ((Vector3Int)_gameData.StartHeroPosition);

            heroes.Add(heroModel);
            views.Add(heroView);

            _container.Instantiate<HeroPresenter>(new object[] {
            heroModel, heroView, _gameModel, _winSystem
        });
        }
        squad = new SquadHeroModel(faction, heroes, views);

        _gameModel.AddSquad(squad);

        _container.Instantiate<SquadHeroPresenter>(new object[] {
            squad, _gameModel, _gridService, this, _dungeonView.StartPoint.position
        });
    }

    public void RemoveSquad(SquadHeroModel squad)
    {
        _gameModel.RemoveSquad(squad);
    }

    public async void StartAutoSpawning(Faction faction, float time)
    {
        var data = _gameModel.GetPlayer(Faction.Player).Data;

        if (time <= 0)
        {
            _spawnTime[(int)faction] = 0;
            return;
        }

        if (time >= _spawnTime[(int)faction] && _spawnTime[(int)faction] <= data.TimeDaySecond)
        {
            float interval = _gameModel.GetSquadSpawnInterval();

            _spawnTime[(int)faction] += interval;

            if (_spawnTime[(int)faction] <= data.TimeDaySecond)
            {
                await SpawnHeroWave(faction);
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
