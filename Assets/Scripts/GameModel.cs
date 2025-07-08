using R3;
using System.Collections.Generic;

public class GameModel
{
    public ReactiveProperty<int> Gold { get; } = new(0);
    public ReactiveProperty<int> DungeonLevel { get; } = new(1);
    public List<RoomModel> Rooms { get; } = new();
    public List<HeroModel> ActiveHeroes { get; } = new();
}
