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
    public ReactiveProperty<int> TotalSquadsDefeated { get; } = new(0);
    public ReactiveProperty<int> TotalRoomsBuilt { get; } = new(0);

    public List<SquadHeroModel> ActiveSquads { get; } = new();
    public List<RoomModel> Rooms { get; } = new();

    private readonly GameData _gameData;

    // События для уведомлений
    public Subject<Unit> OnSquadSpawned { get; } = new();
    public Subject<Unit> OnSquadDefeated { get; } = new();
    public Subject<RoomModel> OnRoomBuilt { get; } = new();
    public Subject<int> OnGoldChanged { get; } = new();

    public GameModel(GameData gameData)
    {
        _gameData = gameData;

        Initialize();
    }

    private void Initialize()
    {
        // Начальные значения
        Gold.Value = _gameData.StartGold;
        DungeonLevel.Value = 1;
    }

    public bool TrySpendGold(int amount)
    {
        if (Gold.Value >= amount)
        {
            Gold.Value -= amount;
            OnGoldChanged.OnNext(Gold.Value);
            return true;
        }
        return false;
    }

    public void AddGold(int amount)
    {
        Gold.Value += amount;
        OnGoldChanged.OnNext(Gold.Value);
    }

    public void AddRoom(RoomModel room)
    {
        var findRoom = Rooms.Find(x => x.Position == room.Position);

        if (findRoom != null && findRoom.Type == RoomType.None)
        {
            Rooms.Remove(findRoom);
        }
        else
        {
            TotalRoomsBuilt.Value++;
            OnRoomBuilt.OnNext(room);
        }

        Rooms.Add(room);

        RoomsIsUnlock(room);

        if(room.Type == RoomType.Stairs)
        {
            UpdateStairsRoom(room, true);
        }
    }

    public void RemoveRoom(RoomModel room)
    {
        Rooms.Remove(room);
        TotalRoomsBuilt.Value--;
        room.Destroy.Value = true;

        RoomsIsUnlock(room);

        if (room.Type == RoomType.Stairs)
        {
            UpdateStairsRoom(room, false);
        }

        foreach (var lockRoom in Rooms)
        {
            Debug.Log(lockRoom.Type + " " + lockRoom.Position + " " + lockRoom.IsUnlocked);
        }
    }

    private void UpdateStairsRoom(RoomModel room, bool isSpawn)
    {
        var allStairRoom = Rooms.FindAll(x => x.Position.x == room.Position.x && x.Position.y < room.Position.y && x.Type == room.Type);

        foreach (var lockRoom in allStairRoom)
        {
            lockRoom.IsUnlocked = isSpawn;
            RoomsIsUnlock(lockRoom);
        }
    }
    private void RoomsIsUnlock(RoomModel room, List<RoomModel> roomModels = null)
    {
        var rooms = new List<RoomModel>();

        if(roomModels != null)
        {
            rooms = roomModels;
        }
        else
        {
            rooms = Rooms.FindAll(x => x.Position.y == room.Position.y);
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
                    Rooms.Find(x => x == rooms[j]).IsUnlocked = isStair;
                }

                rooms.RemoveRange(0, i + 1);

                RoomsIsUnlock(room, rooms);

                return;
            }

        }
    }

    public RoomModel GetRoomAtPosition(Vector2Int position)
    {
        return Rooms.FirstOrDefault(room => room.Position == position);
    }

    public bool HasRoomAtPosition(Vector2Int position)
    {
        return Rooms.Any(room => room.Position == position);
    }

    public void AddSquad(SquadHeroModel squad)
    {
        ActiveSquads.Add(squad);
        OnSquadSpawned.OnNext(Unit.Default);

        // Подписываемся на смерть отряда
        squad.Count
            .Where(count => count <= 0)
            .Take(1)
            .Subscribe(_ => RemoveSquad(squad)); // Добавляем подписку к отряду для автоматической отписки
    }

    public void RemoveSquad(SquadHeroModel squad)
    {
        if (ActiveSquads.Remove(squad))
        {
            TotalSquadsDefeated.Value++;
            OnSquadDefeated.OnNext(Unit.Default);
        }
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
