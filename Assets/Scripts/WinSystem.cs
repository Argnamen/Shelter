using R3;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinSystem
{
    public Dictionary<WinPoint, ReactiveProperty<int>> Points = new Dictionary<WinPoint, ReactiveProperty<int>>();
    public WinSystem(GameData data)
    {
        Points[WinPoint.Interes] = new ReactiveProperty<int>(data.MaxInteres);
        Points[WinPoint.Gold] = new ReactiveProperty<int>(data.MaxGold);
        Points[WinPoint.Ghost] = new ReactiveProperty<int>(data.MaxGhost);
        Points[WinPoint.Vlianie] = new ReactiveProperty<int>(data.MaxVlianie);
    }


    private void AddWinPoint(WinPoint winPoint, int count)
    {
        Points[winPoint].Value = Mathf.Max(0, Points[winPoint].Value - count);

        if (Points[winPoint].Value <= 0)
        {
            Win(winPoint);
        }
        
    }

    private void Win(WinPoint winPoint)
    {
        switch (winPoint)
        {
            case WinPoint.Interes:
                break;
            case WinPoint.Gold:
                break;
            case WinPoint.Ghost:
                break;
            case WinPoint.Vlianie:
                break;
        }
    }

    public void AddWinPoint(EventType type)
    {
        switch (type)
        {
            case EventType.WinBattle:
                AddWinPoint(WinPoint.Interes, 10);
                break;
            case EventType.DieHero:
                AddWinPoint(WinPoint.Interes, -1);
                AddWinPoint(WinPoint.Ghost, 1);
                break;
            case EventType.OpenChest:
                AddWinPoint(WinPoint.Interes, 3);
                break;
            case EventType.CleanDangeon:
                AddWinPoint(WinPoint.Interes, 5);
                break;
            case EventType.DieBoss:
                AddWinPoint(WinPoint.Interes, 10);
                AddWinPoint(WinPoint.Ghost, 10);
                break;
        }
    }
}

public enum WinPoint
{
    Interes, Gold, Ghost, Vlianie
}

public enum EventType 
{
    WinBattle, DieHero, OpenChest, CleanDangeon, DieBoss
}
