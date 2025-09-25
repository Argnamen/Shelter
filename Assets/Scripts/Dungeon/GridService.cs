using ModestTree;
using System.Collections.Generic;
using UnityEngine;

public class GridService
{
    private readonly GridModel _gridModel;
    private readonly RoomFactory _roomFactory;
    private readonly GameModel _gameModel;
    private readonly GameData _gameData;
    private readonly RoomsData _roomsData;
    private readonly DungeonView _dungeonView;

    public GridService(GridModel gridModel, RoomFactory roomFactory, GameModel gameModel, GameData gameData, RoomsData roomData, DungeonView dungeonView)
    {
        _gridModel = gridModel;
        _roomFactory = roomFactory;
        _gameModel = gameModel;
        _gameData = gameData;
        _roomsData = roomData;
        _dungeonView = dungeonView;
    }

    public bool TryPlaceRoom(RoomType roomType, Vector2Int position, Faction faction, MonsterType monsterType = MonsterType.None)
    {
        RoomData roomData = _roomsData.Rooms.Find(x => x.Type == roomType && x.MonsterType == monsterType);

        if (position != _gameData.StartRoomPosition && !GetAvailablePositions(roomType, faction).ContainsItem(position))
        {
            Debug.LogWarning($"Cannot place room at position {position}");
            return false;
        }

        if (!_gridModel.CanPlaceRoomAt(position, roomData, faction))
        {
            Debug.LogWarning($"Cannot place room at position {position}");
            return false;
        }

        int roomCost = roomData.Cost;
        if (position != _gameData.StartRoomPosition && !_gameModel.TrySpendGold(roomCost))
        {
            Debug.LogWarning($"Not enough gold to build {roomType} room. Need {roomCost} gold.");
            return false;
        }

        var room = _roomFactory.CreateRoom(roomType, position, faction, monsterType);
        if (room == null)
        {
            Debug.LogError("Failed to create room!");
            return false;
        }

        _gridModel.AddRoom(faction, roomData, room, position);
        _gameModel.AddRoom(room, faction);

        Debug.Log($"Room placed at {position}. Type: {roomType}. Cost: {roomCost} gold.");
        return true;
    }

    public void RemoveRoom(Vector2Int position, Faction faction)
    {
        if (_gameModel.Rooms[faction][0].Position == position)
            return;

        var roomModel = _gridModel.GetRoomAt(position, faction);
        var roomData = _roomsData.Rooms.Find(x => x.Type == roomModel.Type && x.MonsterType == roomModel.Monster);

        _gameModel.RemoveRoom(roomModel, faction);
        _gameModel.AddGold(roomData.Cost);
        _gridModel.RemoveRoom(position, faction);
        _dungeonView.RemoveRoomView(position, faction);
    }

    public IReadOnlyList<Vector2Int> GetAvailablePositions(RoomType roomType, Faction faction)
    {
        RoomData roomData = _roomsData.Rooms.Find(x => x.Type == roomType);
        List<Vector2Int> newReturn = new List<Vector2Int>();

        foreach(var pos in _gridModel.AvailablePositions[faction])
        {
            var FindRoom = GetRoomAt(pos, faction);

            if(roomType == RoomType.Stairs && FindRoom != null)
            {
                
            }

            if (FindRoom != null && !FindRoom.IsUnlocked)
                continue;

            if (roomData.SpecialNeighbors != null && roomData.SpecialNeighbors.Length > 0 && _gameModel.Rooms[faction].Find(x => x.Type == roomType) != null) 
            {
                foreach (var specialNeighbors in roomData.SpecialNeighbors)
                {
                    if (FindRoom != null &&
                        FindRoom.Type == specialNeighbors.NeighborType)
                    {
                        if (GetRoomAt(pos + specialNeighbors.Neighbor, faction) == null)
                            newReturn.Add(pos + specialNeighbors.Neighbor);
                    }
                }
            }
            else 
            {
                foreach (var posNewRoom in roomData.Neighbors)
                {
                    if (FindRoom != null)
                    {
                        if (GetRoomAt(pos + posNewRoom, faction) == null)
                            newReturn.Add(pos + posNewRoom);
                    }
                } 
            }
        }

        return newReturn;
    }

    public RoomModel GetRoomAt(Vector2Int position, Faction faction)
    {
        return _gridModel.GetRoomAt(position, faction);
    }

    public Vector2Int? GetRoomPosition(RoomModel room, Faction faction)
    {
        for (int x = 0; x < _gameData.GridWidth; x++)
        {
            for (int y = 0; y < _gameData.GridHeight; y++)
            {
                if (_gridModel.Grid[faction][x, y] == room)
                {
                    return new Vector2Int(x, y);
                }
            }
        }
        return null;
    }

    public Vector3 GetWorldPosition(Vector2Int gridPosition)
    {
        return _gridModel.GridToWorldPosition(gridPosition);
    }

    public Vector2Int GetGridPosition(Vector3 worldPosition)
    {
        return _gridModel.WorldToGridPosition(worldPosition);
    }

    public bool IsPositionValid(Vector2Int position)
    {
        return _gridModel.IsPositionValid(position);
    }

    public bool IsPositionEmpty(Vector2Int position, Faction faction)
    {
        return _gridModel.IsPositionEmpty(position, faction);
    }

    public bool CanPlaceRoomAt(Vector2Int position, RoomType roomType, Faction faction)
    {
        return _gridModel.CanPlaceRoomAt(position, _roomsData.Rooms.Find(x => x.Type == roomType), faction);
    }
}