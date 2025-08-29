using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

[CreateAssetMenu(fileName = "Rooms", menuName = "ScriptableObjects/RoomsData", order = 2)]
public class RoomsData : ScriptableObject
{
    public List<RoomData> Rooms = new List<RoomData>();
}

[Serializable]
public class RoomData 
{
    public RoomType Type;
    public MonsterType MonsterType;
    public float CellSize = 2.5f;
    public int Cost;

    public Vector2Int[] Neighbors = {
            new (1, 0),
            new (-1, 0),
        };

    public SpecialNeighbors[] SpecialNeighbors;
}

[Serializable]
public class SpecialNeighbors
{
    public RoomType NeighborType;
    public Vector2Int Neighbor;
}
