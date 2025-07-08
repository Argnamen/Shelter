using UnityEngine;
using Zenject;

public class RoomFactory
{
    private readonly DungeonView _dungeonView;
    private readonly MonsterFactory _monsterFactory;
    private readonly DiContainer _container;

    public RoomFactory(
        DungeonView dungeonView,
        MonsterFactory monsterFactory,
        DiContainer container)
    {
        _dungeonView = dungeonView;
        _monsterFactory = monsterFactory;
        _container = container;
    }

    public RoomModel CreateRoom(RoomType type, Vector2Int position)
    {
        var roomModel = new RoomModel(type, position);

        // Add monsters based on room type
        switch (type)
        {
            case RoomType.Combat:
                AddMonstersToRoom(roomModel, MonsterType.Slime, 3);
                break;
            case RoomType.Rest:
                AddMonstersToRoom(roomModel, MonsterType.Skeleton, 1);
                break;
        }

        var roomView = _dungeonView.CreateRoomView(type, position);
        _container.Instantiate<RoomPresenter>(new object[] { roomModel, roomView });

        return roomModel;
    }

    private void AddMonstersToRoom(RoomModel room, MonsterType type, int count)
    {
        for (int i = 0; i < count; i++)
        {
            room.Monsters.Add(new MonsterModel(type, 30));
        }
    }
}
