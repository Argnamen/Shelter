using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    [SerializeField] private DungeonView _dungeonView;
    [SerializeField] private UIView _uiView;
    [SerializeField] private GameObject _gridCellPrefab;

    public override void InstallBindings()
    {
        // Модели
        Container.BindInterfacesAndSelfTo<GameModel>().AsSingle();
        Container.Bind<GridModel>().AsSingle();

        // Вью
        Container.Bind<DungeonView>().FromInstance(_dungeonView).AsSingle();
        Container.Bind<UIView>().FromInstance(_uiView).AsSingle();

        // Сервисы
        Container.Bind<GridService>().AsSingle();
        Container.Bind<RoomFactory>().AsSingle();
        Container.Bind<HeroSpawner>().AsSingle();
        Container.Bind<MonsterFactory>().AsSingle();

        // Презентеры
        Container.BindInterfacesAndSelfTo<GamePresenter>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<BuildPresenter>().AsSingle().NonLazy();
    }
}