using R3;

public class MonsterModel
{
    public MonsterType Type { get; }
    public ReactiveProperty<int> Health { get; }

    public MonsterModel(MonsterType type, int health)
    {
        Type = type;
        Health = new ReactiveProperty<int>(health);
    }
}

public enum MonsterType { Slime, Skeleton, Goblin }