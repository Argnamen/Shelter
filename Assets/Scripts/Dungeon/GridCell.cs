using UnityEngine;

[System.Serializable]
public class GridCell
{
    public Vector2Int Position;
    public bool IsOccupied;
    public RoomType AllowedRoomType;
    public RoomModel OccupiedRoom;

    public GridCell(Vector2Int position, RoomType allowedType = RoomType.Combat)
    {
        Position = position;
        IsOccupied = false;
        AllowedRoomType = allowedType;
        OccupiedRoom = null;
    }
}
