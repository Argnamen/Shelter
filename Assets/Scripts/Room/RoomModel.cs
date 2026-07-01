using R3;
using System.Collections.Generic;
using UnityEngine;

///<summary>RoomType 
///Combat - Монстр
///Rest - Восстановление героя
///Treasure - Сокровище для героя
///Stairs - Лестница
///</summary>
public enum RoomType { None, Combat, Rest, Treasure, Stairs }

public class RoomModel
{    public RoomType Type { get; }
    public MonsterType Monster { get; }
    public Vector2Int Position { get; set; }
    public List<MonsterModel> Monsters { get; } = new();
    public ReactiveProperty<bool> Enter { get; } = new(false);
    public ReactiveProperty<bool> Destroy { get; } = new();
    public ReactiveProperty<MonsterView> AddMonsterView { get; } = new();
    public ReactiveProperty<HeroView> AddHeroView { get; } = new();
    public bool IsUnlocked { get; set; } = true;

    public SquadHeroModel Squad;
    public RoomModel(RoomType type, MonsterType monster, Vector2Int position)
    {
        Type = type;
        Position = position;
        Monster = monster;

        Enter.Subscribe(isEnter);
    }

    public RoomModel(RoomType type, Vector2Int position)
    {
        Type = type;
        Position = position;
        Monster = MonsterType.None;

        Enter.Subscribe(isEnter);
    }

    private void isEnter(bool value)
    {
        if (Monsters.Count <= 0 && value)
            Enter.Value = false;
    }
}
