using R3;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class SquadHeroModel
{
    public Faction Faction;
    public List<HeroModel> Heroes = new List<HeroModel>();
    public List<HeroView> ViewHeroes = new List<HeroView>();
    public ReactiveProperty<bool> HeroesIsReady { get; } = new();

    public ReactiveProperty<int> Count { get; } = new();

    private readonly object LockObj = new object();

    public SquadHeroModel(Faction faction, List<HeroModel> heroModels, List<HeroView> viewHeroes)
    {
        Faction = faction;
        Heroes = heroModels;
        ViewHeroes = viewHeroes;
        Count.Value = heroModels.Count;
        Initialize();
    }

    public void Initialize()
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
        if (!isReady)
            return;

        lock (LockObj)
        {
            if (Heroes.Count == Heroes.FindAll(x => x.HeroIsReady.Value).Count)
            {
                Count.Value = Heroes.Count;
                HeroesIsReady.OnNext(true);
            }
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

    public void UpdateRoom(RoomModel room)
    {
        var heroes = Heroes.ToArray();

        foreach (var hero in heroes)
        {
            if (hero.Die.Value == false)
                if (room != null)
                {
                    hero.CurrentRoomModel.Value = room;
                }
                else
                {
                    hero.MoveToExit.Value = true;
                }
        }
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
