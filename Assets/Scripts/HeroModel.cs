using R3;

public class HeroModel
{
    public ReactiveProperty<int> Health { get; } = new(100);
    public ReactiveProperty<int> CurrentRoomIndex { get; } = new(-1);
    public HeroState State { get; set; } = HeroState.Entering;
}

public enum HeroState { Entering, Fighting, Resting, Leaving }
