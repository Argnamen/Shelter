using UnityEngine;

public class MonsterFactory
{
    public MonsterView Create(MonsterType type)
    {
        if(type == MonsterType.None)
        {
            return null;
        }
        var prefab = Resources.Load<MonsterView>($"Monsters/{type}");
        var monster = Object.Instantiate(prefab);
        monster.Initialize(type);
        return monster;
    }
}
