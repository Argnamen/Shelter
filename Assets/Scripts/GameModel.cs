using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameModel
{
    public ReactiveProperty<int> Gold { get; } = new(0);
    public ReactiveProperty<int> DungeonLevel { get; } = new(1);
    public ReactiveProperty<int> TotalHeroesDefeated { get; } = new(0);
    public ReactiveProperty<int> TotalRoomsBuilt { get; } = new(0);

    public List<HeroModel> ActiveHeroes { get; } = new();
    public List<RoomModel> Rooms { get; } = new();

    private readonly GameData _gameData;

    // События для уведомлений
    public Subject<Unit> OnHeroSpawned { get; } = new();
    public Subject<Unit> OnHeroDefeated { get; } = new();
    public Subject<RoomModel> OnRoomBuilt { get; } = new();
    public Subject<int> OnGoldChanged { get; } = new();

    public GameModel(GameData gameData)
    {
        _gameData = gameData;

        Initialize();
    }

    private void Initialize()
    {
        // Начальные значения
        Gold.Value = _gameData.StartGold;
        DungeonLevel.Value = 1;
    }

    public bool TrySpendGold(int amount)
    {
        if (Gold.Value >= amount)
        {
            Gold.Value -= amount;
            OnGoldChanged.OnNext(Gold.Value);
            return true;
        }
        return false;
    }

    public void AddGold(int amount)
    {
        Gold.Value += amount;
        OnGoldChanged.OnNext(Gold.Value);
    }

    public void AddRoom(RoomModel room)
    {
        Rooms.Add(room);
        TotalRoomsBuilt.Value++;
        OnRoomBuilt.OnNext(room);
    }

    public void RemoveRoom(RoomModel room)
    {
        Rooms.Remove(room);
        TotalRoomsBuilt.Value--;
        room.Destroy.Value = true;
    }

    public RoomModel GetRoomAtPosition(Vector2Int position)
    {
        return Rooms.FirstOrDefault(room => room.Position == position);
    }

    public bool HasRoomAtPosition(Vector2Int position)
    {
        return Rooms.Any(room => room.Position == position);
    }

    public void AddHero(HeroModel hero)
    {
        ActiveHeroes.Add(hero);
        OnHeroSpawned.OnNext(Unit.Default);

        // Подписываемся на смерть героя
        hero.Health
            .Where(health => health <= 0)
            .Take(1)
            .Subscribe(_ => RemoveHero(hero))
            .AddTo(hero); // Добавляем подписку к герою для автоматической отписки
    }

    public void RemoveHero(HeroModel hero)
    {
        if (ActiveHeroes.Remove(hero))
        {
            TotalHeroesDefeated.Value++;
            OnHeroDefeated.OnNext(Unit.Default);
        }
    }

    public void LevelUpDungeon()
    {
        DungeonLevel.Value++;
    }

    public int GetHeroSpawnCount()
    {
        // Количество героев увеличивается с уровнем подземелья
        return UnityEngine.Random.Range(1, 3 + DungeonLevel.Value);
    }

    public float GetHeroSpawnInterval()
    {
        // Интервал между волнами уменьшается с уровнем
        return Mathf.Max(10f, 30f - DungeonLevel.Value * 2f);
    }

    // Метод для наблюдения за изменением количества героев
    public Observable<int> ObserveHeroesCountChanged()
    {
        return Observable.EveryValueChanged(this, x => x.ActiveHeroes.Count);
    }

    // Метод для наблюдения за изменением количества комнат
    public Observable<int> ObserveRoomsCountChanged()
    {
        return Observable.EveryValueChanged(this, x => x.Rooms.Count);
    }
}

// Расширение для удобной работы с подписками
public static class HeroModelExtensions
{
    public static void AddTo(this IDisposable disposable, HeroModel hero)
    {
        // Можно хранить подписки в HeroModel если нужно
        // В данном случае просто добавляем возможность привязки
    }
}
