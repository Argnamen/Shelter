using System.Collections.Generic;
using UnityEngine;

public enum RoomType { None, Combat, Rest, Treasure, Stairs }

public class RoomModel
{
    public RoomType Type { get; }
    public Vector2Int Position { get; set; }
    public List<MonsterModel> Monsters { get; } = new();
    public bool IsUnlocked { get; set; } = true;

    public RoomModel(RoomType type, Vector2Int position)
    {
        Type = type;
        Position = position;
    }
}
