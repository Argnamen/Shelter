using System.Collections.Generic;
using UnityEngine;

public class GridService
{
    private readonly GridModel _gridModel;
    private readonly RoomFactory _roomFactory;
    private readonly GameModel _gameModel;

    public GridService(GridModel gridModel, RoomFactory roomFactory, GameModel gameModel)
    {
        _gridModel = gridModel;
        _roomFactory = roomFactory;
        _gameModel = gameModel;
    }

    public bool TryPlaceRoom(RoomType roomType, Vector2Int position)
    {
        if (!_gridModel.CanPlaceRoomAt(position))
        {
            Debug.LogWarning($"Cannot place room at position {position}");
            return false;
        }

        int roomCost = _gameModel.GetRoomCost(roomType);
        if (!_gameModel.TrySpendGold(roomCost))
        {
            Debug.LogWarning($"Not enough gold to build {roomType} room. Need {roomCost} gold.");
            return false;
        }

        var room = _roomFactory.CreateRoom(roomType, position);
        if (room == null)
        {
            Debug.LogError("Failed to create room!");
            return false;
        }

        _gridModel.AddRoom(room, position);
        _gameModel.AddRoom(room);

        Debug.Log($"Room placed at {position}. Type: {roomType}. Cost: {roomCost} gold.");
        return true;
    }

    public IReadOnlyList<Vector2Int> GetAvailablePositions()
    {
        return _gridModel.AvailablePositions;
    }

    public RoomModel GetRoomAt(Vector2Int position)
    {
        return _gridModel.GetRoomAt(position);
    }

    public Vector2Int? GetRoomPosition(RoomModel room)
    {
        for (int x = 0; x < _gridModel.GridWidth; x++)
        {
            for (int y = 0; y < _gridModel.GridHeight; y++)
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

    public bool CanPlaceRoomAt(Vector2Int position)
    {
        return _gridModel.CanPlaceRoomAt(position);
    }
}