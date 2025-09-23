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

    public bool TryPlaceRoom(RoomType roomType, Vector2Int position, MonsterType monsterType = MonsterType.None)
    {
        RoomData roomData = _roomsData.Rooms.Find(x => x.Type == roomType && x.MonsterType == monsterType);

        if (position != _gameData.StartRoomPosition && !GetAvailablePositions(roomType).ContainsItem(position))
        {
            Debug.LogWarning($"Cannot place room at position {position}");
            return false;
        }

        if (!_gridModel.CanPlaceRoomAt(position, roomData))
        {
            Debug.LogWarning($"Cannot place room at position {position}");
            return false;
        }

        int roomCost = roomData.Cost;
        if (!_gameModel.TrySpendGold(roomCost))
        {
            Debug.LogWarning($"Not enough gold to build {roomType} room. Need {roomCost} gold.");
            return false;
        }

        var room = _roomFactory.CreateRoom(roomType, position, monsterType);
        if (room == null)
        {
            Debug.LogError("Failed to create room!");
            return false;
        }

        _gridModel.AddRoom(roomData, room, position);
        _gameModel.AddRoom(room);

        Debug.Log($"Room placed at {position}. Type: {roomType}. Cost: {roomCost} gold.");
        return true;
    }

    public void RemoveRoom(Vector2Int position)
    {
        if (_gameModel.Rooms[0].Position == position)
            return;

        var roomModel = _gridModel.GetRoomAt(position);
        var roomData = _roomsData.Rooms.Find(x => x.Type == roomModel.Type && x.MonsterType == roomModel.Monster);

        _gameModel.RemoveRoom(roomModel);
        _gameModel.AddGold(roomData.Cost);
        _gridModel.RemoveRoom(position);
        _dungeonView.RemoveRoomView(position);
    }

    public IReadOnlyList<Vector2Int> GetAvailablePositions(RoomType roomType)
    {
        RoomData roomData = _roomsData.Rooms.Find(x => x.Type == roomType);
        List<Vector2Int> newReturn = new List<Vector2Int>();

        foreach(var pos in _gridModel.AvailablePositions)
        {
            var FindRoom = GetRoomAt(pos);

            if(roomType == RoomType.Stairs && FindRoom != null)
            {
                
            }

            if (FindRoom != null && !FindRoom.IsUnlocked)
                continue;

            if (roomData.SpecialNeighbors != null && roomData.SpecialNeighbors.Length > 0 && _gameModel.Rooms.Find(x => x.Type == roomType) != null) 
            {
                foreach (var specialNeighbors in roomData.SpecialNeighbors)
                {
                    if (FindRoom != null &&
                        FindRoom.Type == specialNeighbors.NeighborType)
                    {
                        if (GetRoomAt(pos + specialNeighbors.Neighbor) == null)
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
                        if (GetRoomAt(pos + posNewRoom) == null)
                            newReturn.Add(pos + posNewRoom);
                    }
                } 
            }
        }

        return newReturn;
    }

    public RoomModel GetRoomAt(Vector2Int position)
    {
        return _gridModel.GetRoomAt(position);
    }

    public Vector2Int? GetRoomPosition(RoomModel room)
    {
        for (int x = 0; x < _gameData.GridWidth; x++)
        {
            for (int y = 0; y < _gameData.GridHeight; y++)
            {
                if (_gridModel.Grid[x, y] == room)
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

    public bool IsPositionEmpty(Vector2Int position)
    {
        return _gridModel.IsPositionEmpty(position);
    }

    public bool CanPlaceRoomAt(Vector2Int position, RoomType roomType)
    {
        return _gridModel.CanPlaceRoomAt(position, _roomsData.Rooms.Find(x => x.Type == roomType));
    }
}