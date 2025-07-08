using UnityEngine;

public class DungeonView : MonoBehaviour
{
    [SerializeField] private Transform _roomsContainer;
    [SerializeField] private Transform _heroesContainer;

    public RoomView CreateRoomView(RoomType type, Vector2Int position)
    {
        var prefab = Resources.Load<RoomView>($"Rooms/{type}Room");
        var room = Instantiate(prefab, _roomsContainer);
        room.transform.position = new Vector3(position.x, position.y, 0);
        room.Initialize(position, type);
        return room;
    }

    public HeroView CreateHeroView()
    {
        var prefab = Resources.Load<HeroView>("Heroes/Hero");
        return Instantiate(prefab, _heroesContainer);
    }
}
