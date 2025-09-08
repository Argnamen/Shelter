using R3;

public class HeroModel
{
    public ReactiveProperty<int> Health { get; } = new(100);
    public ReactiveProperty<RoomModel> CurrentRoomModel { get; } = new();

    public ReactiveProperty<bool> HeroIsReady = new ReactiveProperty<bool>();
    public HeroState State { get; set; } = HeroState.Entering;
    public HeroClass Class { get; } = HeroClass.None;

    public HeroModel(HeroClass heroClass, int health)
    {
        Class = heroClass;
        Health = new(health);
    }
}

public enum HeroState { Entering, Fighting, Resting, Leaving, Dead }
