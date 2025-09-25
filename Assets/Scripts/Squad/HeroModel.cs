using R3;

public class HeroModel
{
    public ReactiveProperty<int> Health { get; } = new(100);
    public ReactiveProperty<int> Damage { get; } = new(100);
    public ReactiveProperty<int> DamageSpead { get; } = new(100);
    public ReactiveProperty<RoomModel> CurrentRoomModel { get; } = new();

    public ReactiveProperty<bool> HeroIsReady = new ReactiveProperty<bool>();

    public ReactiveProperty<bool> Die { get; } = new(false);
    public Faction Faction { get; }
    public HeroState State { get; set; } = HeroState.Entering;
    public HeroClass Class { get; } = HeroClass.None;

    public int Gold;

    public int MaxHealth;

    public HeroModel(Faction faction, HeroClass heroClass, int health, int damage, int damageSpead, int gold)
    {
        Faction = faction;
        Class = heroClass;
        Health = new(health);
        Damage = new(damage);
        DamageSpead = new(damageSpead);
        Gold = gold;

        MaxHealth = health;
    }
}

public enum HeroState { Entering, Fighting, Resting, Leaving, Dead }
