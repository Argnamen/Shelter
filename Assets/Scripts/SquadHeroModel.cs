using R3;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SquadHeroModel
{
    public List<HeroModel> Heroes = new List<HeroModel>();
    public ReactiveProperty<bool> HeroesIsReady { get; } = new();

    private int _count = 0;

    public SquadHeroModel(List<HeroModel> heroModels)
    {
        Heroes = heroModels;
        Initialize();
    }

    private void Initialize()
    {
        foreach (var hero in Heroes) 
        {
            hero.HeroIsReady.Subscribe(AllHeroIsReady);

            hero.Health.Subscribe(x => RemoveHero(hero, x));

            hero.HeroIsReady.Value = true;
        }
    }

    private void AllHeroIsReady(bool isReady)
    {
        if (isReady)
        {
            _count -= 1;
        }

        if(_count <= 0)
        {
            _count = Heroes.Count;
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

    public void RemoveHero(HeroModel hero, int health)
    {
        if (health <= 0)
        {
            _count--;
            Heroes.Remove(hero);
        }
    }
}
