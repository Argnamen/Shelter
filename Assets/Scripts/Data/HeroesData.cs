using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Heroes", menuName = "ScriptableObjects/HeroesData", order = 4)]
public class HeroesData : ScriptableObject
{
    public List<Hero> Heroes;
}

[Serializable]
public class Hero
{
    public HeroClass Class;
    public string Name;
    public int Health;
    public int Damage;
    public GameObject Prefab;
}

public enum HeroClass { None, Damager, Tank, Healing, SuperHero }