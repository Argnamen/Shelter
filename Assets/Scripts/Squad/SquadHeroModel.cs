using R3;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SquadHeroModel
{
    private int HeroIsReadyCount = 0;

    public Faction Faction;
    public List<HeroModel> Heroes = new List<HeroModel>();
    public List<HeroView> ViewHeroes = new List<HeroView>();
    public ReactiveProperty<bool> HeroesIsReady { get; } = new();

    public ReactiveProperty<int> Count { get; } = new();

    public SquadHeroModel(Faction faction, List<HeroModel> heroModels, List<HeroView> viewHeroes)
    {
        Faction = faction;
        Heroes = heroModels;
        ViewHeroes = viewHeroes;
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
        if(isReady)
            HeroIsReadyCount++;

        if(HeroIsReadyCount == Heroes.Count)
        {
            HeroIsReadyCount = 0;

            Count.Value = Heroes.Count;
            HeroesIsReady.OnNext(true);
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
