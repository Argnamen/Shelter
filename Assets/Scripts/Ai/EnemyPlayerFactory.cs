using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class EnemyPlayerFactory
{
    private readonly DiContainer _container;
    public EnemyPlayerFactory(DiContainer container)
    {
        _container = container;
    }
    public IEnemyPlayer Create(PlayerType playerType)
    {
        switch (playerType) 
        {
            case PlayerType.SkeletonLedi:
                return _container.Instantiate<LedySkeleton>();
            case PlayerType.GigaKrish:
                return _container.Instantiate<GigaCkhrush>();
            case PlayerType.None:
                return null;
        }

        return null;
    }
}
