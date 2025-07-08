using R3;
using System;
using Zenject;

public class GamePresenter : IInitializable, IDisposable
{
    private readonly GameModel _model;
    private readonly DungeonView _dungeonView;
    private readonly UIView _uiView;
    private readonly RoomFactory _roomFactory;
    private readonly HeroSpawner _heroSpawner;

    private CompositeDisposable _disposables = new();

    public GamePresenter(
        GameModel model,
        DungeonView dungeonView,
        UIView uiView,
        RoomFactory roomFactory,
        HeroSpawner heroSpawner)
    {
        _model = model;
        _dungeonView = dungeonView;
        _uiView = uiView;
        _roomFactory = roomFactory;
        _heroSpawner = heroSpawner;
    }

    public void Initialize()
    {
        _uiView.OnBuildButtonClicked.Subscribe(_ => _uiView.ToggleBuildMenu(true)).AddTo(_disposables);
        _uiView.OnPlayButtonClicked.Subscribe(_ => StartHeroWave()).AddTo(_disposables);

        _model.Gold.Subscribe(gold => _uiView.UpdateGold(gold)).AddTo(_disposables);
        _model.DungeonLevel.Subscribe(level => _uiView.UpdateLevel(level)).AddTo(_disposables);
    }

    private void StartHeroWave()
    {
        _heroSpawner.SpawnHeroWave();
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}
