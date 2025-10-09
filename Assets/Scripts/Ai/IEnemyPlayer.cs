using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject.SpaceFighter;

public interface IEnemyPlayer
{
    public PlayerData PlayerData { get; }

    public void StartLocalRools();

    public bool IsBuildRoom(RoomType roomType, Vector2Int position, MonsterType monsterType = MonsterType.None);
}
