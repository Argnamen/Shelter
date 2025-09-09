using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Monsters", menuName = "ScriptableObjects/MonstersData", order = 3)]
public class MonstersData : ScriptableObject
{
    public List<Monster> Monsters;
}

[Serializable]
public class Monster
{
    public MonsterType Type;
    public string Name;
    public int Health;
    public int Damage;
    public int DamageSpeadMillisecond;
    public GameObject Prefab;
}

public enum MonsterType { None, Slime, Skeleton, Eagle }
