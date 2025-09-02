using System.Collections.Generic;
using UnityEngine;

public class GridModel
{
    private int _width = 5;
    private int _height  = 5;
    private float _cellSize = 2.5f;

    private Vector2Int _startPosition;

    public RoomModel[,] Grid { get; private set; }
    public List<Vector2Int> AvailablePositions { get; } = new();

    public GridModel(int width, int height, float cellSize, Vector2Int startPos)
    {
        _width = width;
        _height = height;
        _cellSize = cellSize;
        _startPosition = startPos;
        InitializeGrid();
    }

    private void InitializeGrid()
    {
        Grid = new RoomModel[_width, _height];
        InitializeAvailablePositions();
    }

    private void InitializeAvailablePositions()
    {
        // Центральная позиция для стартовой комнаты
        AvailablePositions.Add(_startPosition);
    }

    private bool IsGridNull()
    {
        foreach (var item in Grid)
        {
            if (item != null)
            {
                return false;
            }
        }

        return true;
    }

    public bool IsPositionValid(Vector2Int position)
    {
        return position.x >= 0 && position.x < _width &&
               position.y >= 0 && position.y < _height;
    }

    public bool IsPositionEmpty(Vector2Int position)
    {
        return IsPositionValid(position) && Grid[position.x, position.y] == null;
    }

    public bool CanPlaceRoomAt(Vector2Int position, RoomData data)
    {
        if (IsGridNull())
        {
            return true;
        }

        if (!IsPositionEmpty(position)) 
        { 
            return false; 
        }

        if (data != null)
        {
            Vector2Int newNeighbor = new Vector2Int();

            foreach (var neighbor in data.Neighbors)
            {
                newNeighbor = neighbor + position;

                if (IsPositionValid(newNeighbor) && Grid[newNeighbor.x, newNeighbor.y] != null)
                {
                    return true;
                }
            }

            foreach (var neighbor in data.SpecialNeighbors)
            {
                newNeighbor = neighbor.Neighbor + position;

                if (IsPositionValid(newNeighbor) && Grid[newNeighbor.x, newNeighbor.y] != null)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void AddRoom(RoomData data, RoomModel room, Vector2Int position)
    {
        if (!IsPositionEmpty(position)) return;

        Grid[position.x, position.y] = room;
        room.Position = position;

        // Обновляем доступные позиции
        //UpdateAvailablePositions(data, position);

        AvailablePositions.Add(position);
    }

    public void RemoveRoom(Vector2Int position)
    {
        if (IsPositionEmpty(position)) return;

        Grid[position.x, position.y] = null;
    }

    private void UpdateAvailablePositions(RoomData data, Vector2Int placedPosition)
    {
        AvailablePositions.Remove(placedPosition);

        foreach (var direction in data.Neighbors)
        {
            var newPos = placedPosition + direction;
            if (IsPositionEmpty(newPos) && CanPlaceRoomAt(newPos, data) && !AvailablePositions.Contains(newPos))
            {
                AvailablePositions.Add(newPos);
            }
        }

        foreach (var direction in data.SpecialNeighbors)
        {
            var newPos = placedPosition + direction.Neighbor;
            if (IsPositionEmpty(newPos) && CanPlaceRoomAt(newPos, data) && !AvailablePositions.Contains(newPos))
            {
                AvailablePositions.Add(newPos);
            }
        }
    }

    public Vector3 GridToWorldPosition(Vector2Int gridPosition)
    {
        return new Vector3(
            gridPosition.x * _cellSize,
            gridPosition.y * _cellSize,
            0
        );
    }

    public Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        int x = Mathf.RoundToInt(worldPosition.x / _cellSize);
        int y = Mathf.RoundToInt(worldPosition.y / _cellSize);
        return new Vector2Int(x, y);
    }

    public RoomModel GetRoomAt(Vector2Int position)
    {
        if (IsPositionValid(position))
        {
            return Grid[position.x, position.y];
        }
        return null;
    }
}
