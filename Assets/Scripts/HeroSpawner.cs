using Zenject;

public class HeroSpawner
{
    private readonly GameModel _gameModel;
    private readonly DungeonView _dungeonView;
    private readonly DiContainer _container;

    public HeroSpawner(
        GameModel gameModel,
        DungeonView dungeonView,
        DiContainer container)
    {
        _gameModel = gameModel;
        _dungeonView = dungeonView;
        _container = container;
    }

    public void SpawnHeroWave()
    {
        int heroCount = UnityEngine.Random.Range(1, 5);

        for (int i = 0; i < heroCount; i++)
        {
            var heroModel = new HeroModel();
            _gameModel.ActiveHeroes.Add(heroModel);

            var heroView = _dungeonView.CreateHeroView(heroModel);
            _container.Instantiate<HeroPresenter>(new object[] { heroModel, heroView, _gameModel });

            heroModel.CurrentRoomIndex.Value = 0;
        }
    }
}
