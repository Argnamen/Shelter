using System.ComponentModel;
using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    [SerializeField] private DungeonView _dungeonView;
    [SerializeField] private UIView _uiView;
    [SerializeField] private CameraController _cameraController;

    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<GameModel>().AsSingle();
        Container.Bind<DungeonView>().FromInstance(_dungeonView).AsSingle();
        Container.Bind<UIView>().FromInstance(_uiView).AsSingle();

        Container.Bind<RoomFactory>().AsSingle();
        Container.Bind<HeroSpawner>().AsSingle();
        Container.Bind<MonsterFactory>().AsSingle();

        Container.BindInterfacesAndSelfTo<GamePresenter>().AsSingle().NonLazy();

        Container.Bind<CameraController>().FromInstance(_cameraController).AsSingle();
    }
}