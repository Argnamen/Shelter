using R3;

public class MonsterModel
{
    public MonsterType Type { get; }
    public ReactiveProperty<int> Health { get; }
    public ReactiveProperty<int> Damage { get; }
    public ReactiveProperty<int> DamageSpeadMillisecond { get; }

    public MonsterModel(MonsterType type, int health, int damage, int damageSpead)
    {
        Type = type;
        Health = new(health);
        Damage = new(damage);
        DamageSpeadMillisecond = new(damageSpead);
    }
}