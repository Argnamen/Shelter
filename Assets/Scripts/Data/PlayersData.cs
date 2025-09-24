using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Players", menuName = "ScriptableObjects/PlayersData", order = 5)]
public class PlayersData : ScriptableObject
{
    public List<PlayerData> Players = new List<PlayerData>();
}

[Serializable]
public class PlayerData
{
    public PlayerType playerType;
    public Sprite Image;

    public int StartGold;
    public RoomsData Rooms;
}

public enum PlayerType { None, SkeletonLedi, GigaKrish }
