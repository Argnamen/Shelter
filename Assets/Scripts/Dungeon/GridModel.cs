using System.Collections.Generic;
using UnityEngine;

public class GridModel
{
    private int _width = 5;
    private int _height  = 5;
    private float _cellSize = 2.5f;

    private Vector2Int _startPosition;

    public Dictionary<Faction, RoomModel[,]> Grid { get; private set; } = new();
    public Dictionary<Faction,List<Vector2Int>> AvailablePositions { get; } = new();

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
        Grid.Add(Faction.Player, new RoomModel[_width, _height]);
        Grid.Add(Faction.Enemy1, new RoomModel[_width, _height]);
        Grid.Add(Faction.Enemy2, new RoomModel[_width, _height]);
        Grid.Add(Faction.Enemy3, new RoomModel[_width, _height]);

        AvailablePositions.Add(Faction.Player, new());
        AvailablePositions.Add(Faction.Enemy1, new());
        AvailablePositions.Add(Faction.Enemy2, new());
        AvailablePositions.Add(Faction.Enemy3, new());

        InitializeAvailablePositions();
    }

    private void InitializeAvailablePositions()
    {
        // Центральная позиция для стартовой комнаты
        AvailablePositions[Faction.Player].Add(_startPosition);
        AvailablePositions[Faction.Enemy1].Add(_startPosition);
        AvailablePositions[Faction.Enemy2].Add(_startPosition);
        AvailablePositions[Faction.Enemy3].Add(_startPosition);
    }

    private bool IsGridNull(Faction faction)
    {
        foreach (var item in Grid[faction])
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

    public bool IsPositionEmpty(Vector2Int position, Faction faction)
    {
        return IsPositionValid(position) && Grid[faction][position.x, position.y] == null;
    }

    public bool CanPlaceRoomAt(Vector2Int position, RoomData data, Faction faction)
    {
        if (IsGridNull(faction))
        {
            return true;
        }

        if (!IsPositionEmpty(position, faction)) 
        { 
            return false; 
        }

        if (data != null)
        {
            Vector2Int newNeighbor = new Vector2Int();

            foreach (var neighbor in data.Neighbors)
            {
                newNeighbor = neighbor + position;

                if (IsPositionValid(newNeighbor) && Grid[faction][newNeighbor.x, newNeighbor.y] != null)
                {
                    return true;
                }
            }

            foreach (var neighbor in data.SpecialNeighbors)
            {
                newNeighbor = neighbor.Neighbor + position;

                if (IsPositionValid(newNeighbor) && Grid[faction][newNeighbor.x, newNeighbor.y] != null)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void AddRoom(Faction faction, RoomData data, RoomModel room, Vector2Int position)
    {
        if (!IsPositionEmpty(position, faction)) return;

        Grid[faction][position.x, position.y] = room;
        room.Position = position;

        // Обновляем доступные позиции
        //UpdateAvailablePositions(data, position);

        AvailablePositions[faction].Add(position);
    }

    public void RemoveRoom(Vector2Int position, Faction faction)
    {
        if (IsPositionEmpty(position, faction)) return;

        Grid[faction][position.x, position.y] = null;
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

    public RoomModel GetRoomAt(Vector2Int position, Faction faction)
    {
        if (IsPositionValid(position))
        {
            return Grid[faction][position.x, position.y];
        }
        return null;
    }
}
