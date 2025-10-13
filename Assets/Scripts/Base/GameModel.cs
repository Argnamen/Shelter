using Cysharp.Threading.Tasks;
using R3;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameModel
{
    public ReactiveProperty<int> Gold { get; } = new(0);
    public ReactiveProperty<int> DungeonLevel { get; } = new(1);
    public ReactiveProperty<int> TotalRoomsBuilt { get; } = new(0);

    public List<SquadHeroModel> ActiveSquads { get; } = new();
    public Dictionary<Faction, List<RoomModel>> Rooms { get; } = new();

    private readonly GameData _gameData;
    private readonly WinSystem _winSystem;
    private readonly PlayerData _playerData;

    private Dictionary<Faction, int> _goldEnemys = new();

    // События для уведомлений
    public Subject<RoomModel> OnRoomBuilt { get; } = new();
    public Subject<int> OnGoldChanged { get; } = new();

    public GameModel(GameData gameData, WinSystem winSystem, PlayerData playerData)
    {
        _gameData = gameData;
        _winSystem = winSystem;
        _playerData = playerData;

        Initialize();
    }

    private void Initialize()
    {
        // Начальные значения
        Gold.Value = _playerData.StartGold;
        _goldEnemys.Add(Faction.Enemy1, _playerData.StartGold);
        _goldEnemys.Add(Faction.Enemy2, _playerData.StartGold);
        _goldEnemys.Add(Faction.Enemy3, _playerData.StartGold);
        DungeonLevel.Value = 1;

        Rooms.Add(Faction.Player, new List<RoomModel>());
        Rooms.Add(Faction.Enemy1, new List<RoomModel>());
        Rooms.Add(Faction.Enemy2, new List<RoomModel>());
        Rooms.Add(Faction.Enemy3, new List<RoomModel>());
    }

    public bool TrySpendGold(int amount, Faction faction)
    {
        if(faction != Faction.Player)
        {
            if (_goldEnemys[faction] >= amount) 
            {
                AddGold(-amount, faction);
                return true;
            }

            return false;
        }
        else if (Gold.Value >= amount)
        {
            AddGold(-amount, faction);
            return true;
        }
        return false;
    }

    public void AddGold(int amount, Faction faction)
    {
        if (faction == Faction.Player)
        {
            Gold.Value += amount;

            _winSystem.AddWinPoint(WinPoint.Gold, amount);

            OnGoldChanged.OnNext(Gold.Value);
        }
        else
        {
            _goldEnemys[faction] += amount;
        }
    }

    public void AddRoom(RoomModel room, Faction faction)
    {
        var findRoom = Rooms[faction].Find(x => x.Position == room.Position);

        if (findRoom != null && findRoom.Type == RoomType.None)
        {
            Rooms[faction].Remove(findRoom);
        }
        else
        {
            TotalRoomsBuilt.Value++;
            OnRoomBuilt.OnNext(room);
        }

        Rooms[faction].Add(room);

        RoomsIsUnlock(faction, room);

        if(room.Type == RoomType.Stairs)
        {
            UpdateStairsRoom(faction, room, true);
        }
    }

    public void RemoveRoom(RoomModel room, Faction faction)
    {
        Rooms[faction].Remove(room);
        TotalRoomsBuilt.Value--;
        room.Destroy.Value = true;

        RoomsIsUnlock(faction, room);

        if (room.Type == RoomType.Stairs)
        {
            UpdateStairsRoom(faction, room, false);
        }

        foreach (var lockRoom in Rooms[faction])
        {
            Debug.Log(lockRoom.Type + " " + lockRoom.Position + " " + lockRoom.IsUnlocked);
        }
    }

    private void UpdateStairsRoom(Faction faction, RoomModel room, bool isSpawn)
    {
        var allStairRoom = Rooms[faction].FindAll(x => x.Position.x == room.Position.x && x.Position.y < room.Position.y && x.Type == room.Type);

        foreach (var lockRoom in allStairRoom)
        {
            lockRoom.IsUnlocked = isSpawn;
            RoomsIsUnlock(faction, lockRoom);
        }
    }
    private void RoomsIsUnlock(Faction faction, RoomModel room, List<RoomModel> roomModels = null)
    {
        var rooms = new List<RoomModel>();

        if(roomModels != null)
        {
            rooms = roomModels;
        }
        else
        {
            rooms = Rooms[faction].FindAll(x => x.Position.y == room.Position.y);
        }

        rooms = rooms.OrderBy(x => x.Position.x).ToList();

        bool isStair = false;

        for (int i = 0; i < rooms.Count; i++)
        {
            if ((rooms[i].Type == RoomType.Stairs && rooms[i].IsUnlocked) || 
                (rooms[i].Position == _gameData.StartRoomPosition))
            {
                isStair = true;
            }

            if (i + 1 >= rooms.Count || rooms[i + 1].Position.x - rooms[i].Position.x > 1)
            {
                for (int j = 0; j <= i; j++)
                {
                    rooms[j].IsUnlocked = isStair;
                    Rooms[faction].Find(x => x == rooms[j]).IsUnlocked = isStair;
                }

                rooms.RemoveRange(0, i + 1);

                RoomsIsUnlock(faction, room, rooms);

                return;
            }

        }
    }

    public RoomModel GetRoomAtPosition(Faction faction, Vector2Int position)
    {
        return Rooms[faction].FirstOrDefault(room => room.Position == position);
    }

    public bool HasRoomAtPosition(Faction faction, Vector2Int position)
    {
        return Rooms[faction].Any(room => room.Position == position);
    }

    public void AddSquad(SquadHeroModel squad)
    {
        ActiveSquads.Add(squad);

        // Подписываемся на смерть отряда
        squad.Count
            .Where(count => count <= 0)
            .Take(1)
            .Subscribe(_ => RemoveSquad(squad)); // Добавляем подписку к отряду для автоматической отписки
    }

    public void RemoveSquad(SquadHeroModel squad)
    {
        ActiveSquads.Remove(squad);
    }

    public void LevelUpDungeon()
    {
        DungeonLevel.Value++;
    }

    public int GetSquadSpawnCount()
    {
        // Количество героев увеличивается с уровнем подземелья
        return UnityEngine.Random.Range(1, 4);
    }

    public float GetSquadSpawnInterval()
    {
        // Интервал между волнами уменьшается с уровнем
        return _gameData.LengthDay / _gameData.TimeDaySecond * UnityEngine.Random.Range(1, _gameData.TimeDaySecond);
    }

    // Метод для наблюдения за изменением количества героев
    public Observable<int> ObserveSquadsCountChanged()
    {
        return Observable.EveryValueChanged(this, x => x.ActiveSquads.Count);
    }

    // Метод для наблюдения за изменением количества комнат
    public Observable<int> ObserveRoomsCountChanged()
    {
        return Observable.EveryValueChanged(this, x => x.Rooms.Count);
    }
}
