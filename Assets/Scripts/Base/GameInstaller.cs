using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    [SerializeField] private DungeonView _dungeonView;
    [SerializeField] private UIView _uiView;
    [SerializeField] private GameObject _gridCellPrefab;
    [SerializeField] private CameraController _cameraController;
    [SerializeField] private GameData _gameData;
    [SerializeField] private HeroesData _heroesData;
    [SerializeField] private PlayersData _playersData;
    [SerializeField] private MonstersData _monstersData;

    public override void InstallBindings()
    {
        PlayerData playerData = _playersData.Players[1];

        //Данные
        Container.Bind<GameData>().FromInstance(_gameData).AsSingle();
        Container.Bind<HeroesData>().FromInstance(_heroesData).AsSingle();

        Container.Bind<PlayerData>().FromInstance(playerData).AsSingle();
        Container.Bind<RoomsData>().FromInstance(playerData.Rooms).AsSingle();
        Container.Bind<MonstersData>().FromInstance(_monstersData).AsSingle();

        // Модели
        Container.BindInterfacesAndSelfTo<GameModel>().AsSingle();
        Container.Bind<GridModel>().FromInstance(new GridModel(_gameData.GridWidth, _gameData.GridHeight, _gameData.CellSize, _gameData.StartHeroPosition)).AsSingle();

        // Вью
        Container.Bind<DungeonView>().FromInstance(_dungeonView).AsSingle();
        Container.Bind<UIView>().FromInstance(_uiView).AsSingle();

        // Сервисы
        Container.Bind<GridService>().AsSingle();
        Container.Bind<RoomFactory>().AsSingle();
        Container.Bind<SquadSpawner>().AsSingle();
        Container.Bind<MonsterSpawner>().AsSingle();
        Container.Bind<MonsterFactory>().AsSingle();
        Container.Bind<DayCycleService>().AsSingle();
        Container.Bind<WinSystem>().AsSingle();

        // Презентеры
        Container.BindInterfacesAndSelfTo<GamePresenter>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<BuildPresenter>().AsSingle().NonLazy();

        //Камера
        Container.Bind<CameraController>().FromInstance(_cameraController).AsSingle();

        //AI
        Container.BindInterfacesAndSelfTo<AIManager>().AsSingle().NonLazy();
    }
}