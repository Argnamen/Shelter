using R3;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class MainPlayerManager : IInitializable, IDisposable
{ 
    private readonly GameModel _gameModel;
    private readonly DayCycleService _dayCycleService;
    private readonly DiContainer _container;
    private readonly CompositeDisposable _disposables = new();

    private PlayerModel _model;
    public MainPlayerManager(DiContainer container, DayCycleService dayCycleService, GameModel gameModel)
    {
        _dayCycleService = dayCycleService;
        _gameModel = gameModel;
        _container = container;
    }
    public void Initialize()
    {
        CreateMainPlayer();
    }

    private void CreateMainPlayer()
    {
        _model = new PlayerModel(_dayCycleService, _gameModel);
    }

    public void Dispose()
    {
        _model.Dispose();
        _disposables?.Dispose();
    }
}
