using R3;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerModel
{
    private readonly DayCycleService _dayCycleService;
    private readonly GameModel _gameModel;

    public int StartGold;
    public RoomData RoomData;
    public MonstersData MonstersData;

    private CompositeDisposable _spawnDisposables = new();

    public PlayerModel(DayCycleService dayCycleService, GameModel gameModel)
    {
        _dayCycleService = dayCycleService;
        _gameModel = gameModel;

        _dayCycleService.Time.Subscribe(NewDay).AddTo(_spawnDisposables);
    }

    private void NewDay(float time)
    {
        if(time <= 0f)
        {
            _gameModel.AddGold(100, Faction.Player);
        }
    }

    public void Dispose()
    {
        _spawnDisposables.Dispose();
    }
}
