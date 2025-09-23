using UnityEngine;

public class MonsterFactory
{
    private readonly MonstersData _monsterData;

    public MonsterFactory(MonstersData monsterData)
    {
        _monsterData = monsterData;
    }

    public MonsterView Create(MonsterType type)
    {
        if(type == MonsterType.None)
        {
            return null;
        }
        var monster = Object.Instantiate(_monsterData.Monsters.Find(x => x.Type == type).Prefab.GetComponent<MonsterView>());
        monster.Initialize(type);
        return monster;
    }
}
