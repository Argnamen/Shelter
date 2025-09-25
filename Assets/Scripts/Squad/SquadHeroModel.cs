using R3;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SquadHeroModel
{
    public Faction Faction;
    public List<HeroModel> Heroes = new List<HeroModel>();
    public ReactiveProperty<bool> HeroesIsReady { get; } = new();

    public ReactiveProperty<int> Count { get; } = new();

    public SquadHeroModel(Faction faction, List<HeroModel> heroModels)
    {
        Faction = faction;
        Heroes = heroModels;
        Count.Value = heroModels.Count;
        Initialize();
    }

    private void Initialize()
    {
        foreach (var hero in Heroes) 
        {
            hero.HeroIsReady.Subscribe(AllHeroIsReady);

            hero.Die.Subscribe(x => RemoveHero(hero, x));

            hero.HeroIsReady.Value = true;
        }
    }

    private void AllHeroIsReady(bool isReady)
    {
        if(Heroes.FindAll(x => x.HeroIsReady.Value).Count == Heroes.Count)
        {
            Count.Value = Heroes.Count;
            HeroesIsReady.Value = true;
        }
        else
        {
            HeroesIsReady.Value = false;
        }
    }

    public HeroModel GetHero()
    {
        if (Heroes.Count == 0)
            return null;

        var hero = Heroes.Find(x => x.Class == HeroClass.Tank);

        if (hero == null)
        {
            hero = Heroes[UnityEngine.Random.Range(0, Heroes.Count)];
        }

        return hero;
    }

    public void RemoveHero(HeroModel hero, bool isDie)
    {
        if (isDie)
        {
            Heroes.Remove(hero);
            Count.Value--;
        }
    }
}
