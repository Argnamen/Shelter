using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GigaCkhrush : IEnemyPlayer
{
    public PlayerData PlayerData => throw new System.NotImplementedException();

    private readonly RoomsData _roomsData;

    public GigaCkhrush(RoomsData roomData)
    {
        _roomsData = roomData;
    }

    public void StartLocalRools()
    {
        SpawnRoom();
    }

    public bool IsBuildRoom(RoomType roomType, Vector2Int position, MonsterType monsterType = MonsterType.None)
    {
        RoomData roomData = _roomsData.Rooms.Find(x => x.Type == roomType && x.MonsterType == monsterType);


        return true;
    }

    private void SpawnRoom()
    {

    }
}
