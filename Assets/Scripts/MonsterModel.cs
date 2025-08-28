using R3;

public class MonsterModel
{
    public MonsterType Type { get; }
    public ReactiveProperty<int> Health { get; }
    public int RoomIndex { get; }

    public float TimeSpawn { get; }

    public MonsterModel(MonsterType type, int health, float timeSpawn, int roomIndex)
    {
        Type = type;
        Health = new ReactiveProperty<int>(health);
        TimeSpawn = timeSpawn;
        RoomIndex = roomIndex;
    }
}

public enum MonsterType { None, Slime, Skeleton, Eagle }