using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Game", menuName = "ScriptableObjects/GameData", order = 1)]
public class GameData : ScriptableObject
{
    public int GridHeight = 5;
    public int GridWidth = 5;
    public float CellSize = 2.5f;

    public Vector2Int StartHeroPosition = new Vector2Int(0, 5);
    public Vector2Int StartRoomPosition = new Vector2Int(0, 4);

    public int TimeDaySecond = 10;
    public int LengthDay = 100;

    public int MaxInteres = 1000;
    public int MaxGold = 1000;
    public int MaxGhost = 1000;
    public int MaxVlianie = 1000;
}
