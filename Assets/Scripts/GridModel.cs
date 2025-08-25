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
        // ÷ентральна€ позици€ дл€ стартовой комнаты
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

    public bool CanPlaceRoomAt(Vector2Int position)
    {
        if (IsGridNull())
        {
            return true;
        }

        if (!IsPositionEmpty(position)) 
        { 
            return false; 
        }

        // ѕровер€ем соседние позиции (только ортогональные соседи)
        Vector2Int[] neighbors = {
            new(position.x + 1, position.y),
            new(position.x - 1, position.y),
            //new(position.x, position.y + 1),
            //new(position.x, position.y - 1)
        };

        foreach (var neighbor in neighbors)
        {
            if (IsPositionValid(neighbor) && Grid[neighbor.x, neighbor.y] != null)
            {
                return true;
            }
        }

        return false;
    }

    public void AddRoom(RoomModel room, Vector2Int position)
    {
        if (!IsPositionEmpty(position)) return;

        Grid[position.x, position.y] = room;
        room.Position = position;

        // ќбновл€ем доступные позиции
        UpdateAvailablePositions(position);
    }

    private void UpdateAvailablePositions(Vector2Int placedPosition)
    {
        AvailablePositions.Remove(placedPosition);

        // ƒобавл€ем новые возможные позиции вокруг установленной комнаты
        Vector2Int[] directions = {
            new(1, 0), new(-1, 0),
            //new(0, 1), new(0, -1)
        };

        foreach (var direction in directions)
        {
            var newPos = placedPosition + direction;
            if (IsPositionEmpty(newPos) && CanPlaceRoomAt(newPos) && !AvailablePositions.Contains(newPos))
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
