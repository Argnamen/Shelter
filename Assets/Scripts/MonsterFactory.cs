using UnityEngine;

public class MonsterFactory
{
    public MonsterView Create(MonsterType type)
    {
        var prefab = Resources.Load<MonsterView>($"Monsters/{type}Monster");
        var monster = Object.Instantiate(prefab);
        monster.Initialize(type);
        return monster;
    }
}
