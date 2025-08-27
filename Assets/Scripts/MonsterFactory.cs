using UnityEngine;

public class MonsterFactory
{
    public MonsterView Create(MonsterType type)
    {
        var prefab = Resources.Load<MonsterView>($"Monsters/Slime");
        var monster = Object.Instantiate(prefab);
        monster.Initialize(type);
        return monster;
    }
}
