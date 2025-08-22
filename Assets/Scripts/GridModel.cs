using System.Collections.Generic;
using UnityEngine;

public class GridModel
{
    public int GridWidth { get; private set; } = 5;
    public int GridHeight { get; private set; } = 5;
    public float CellSize { get; private set; } = 2.5f;

    public RoomModel[,] Grid { get; private set; }
    public List<Vector2Int> AvailablePositions { get; } = new();

    public GridModel()
    {
        InitializeGrid();
    }

    public GridModel(int width, int height, float cellSize)
    {
        GridWidth = width;
        GridHeight = height;
        CellSize = cellSize;
        InitializeGrid();
    }

    private void InitializeGrid()
    {
        Grid = new RoomModel[GridWidth, GridHeight];
        InitializeAvailablePositions();
    }

    private void InitializeAvailablePositions()
    {
        // ÷ентральна€ позици€ дл€ стартовой комнаты
        AvailablePositions.Add(new Vector2Int(2, 2));
    }

    public bool IsPositionValid(Vector2Int position)
    {
        return position.x >= 0 && position.x < GridWidth &&
               position.y >= 0 && position.y < GridHeight;
    }

    public bool IsPositionEmpty(Vector2Int position)
    {
        return IsPositionValid(position) && Grid[position.x, position.y] == null;
    }

    public bool CanPlaceRoomAt(Vector2Int position)
    {
        if (!IsPositionEmpty(position)) return false;

        // ѕровер€ем соседние позиции (только ортогональные соседи)
        Vector2Int[] neighbors = {
            new(position.x + 1, position.y),
            new(position.x - 1, position.y),
            new(position.x, position.y + 1),
            new(position.x, position.y - 1)
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
            new(1, 0), new(-1, 0), new(0, 1), new(0, -1)
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
            gridPosition.x * CellSize,
            gridPosition.y * CellSize,
            0
        );
    }

    public Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        int x = Mathf.RoundToInt(worldPosition.x / CellSize);
        int y = Mathf.RoundToInt(worldPosition.y / CellSize);
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
