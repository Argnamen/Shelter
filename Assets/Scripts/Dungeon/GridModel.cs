using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class GridModel
{
    private int _width = 5;
    private int _height  = 5;
    private float _cellSize = 2.5f;

    private DungeonView _view;
    private GameModel _gameModel;

    private Vector2Int _startPosition;

    public Dictionary<Faction, RoomModel[,]> Grid { get; private set; } = new();
    public Dictionary<Faction,List<Vector2Int>> AvailablePositions { get; } = new();

    public GridModel(GameModel gameModel, DungeonView dungeonView)
    {
        _startPosition = Vector2Int.zero;

        _view = dungeonView;
        _gameModel = gameModel;

        Initialize();
    }

    public void Initialize()
    {
        NewGrid(Faction.Player);
        NewGrid(Faction.Enemy1);
        NewGrid(Faction.Enemy2);
        NewGrid(Faction.Enemy3);

        AvailablePositions.Add(Faction.Player, new());
        AvailablePositions.Add(Faction.Enemy1, new());
        AvailablePositions.Add(Faction.Enemy2, new());
        AvailablePositions.Add(Faction.Enemy3, new());

        InitializeAvailablePositions();
    }

    private void NewGrid(Faction faction)
    {
        var data = _gameModel.GetPlayer(faction).Data;

        if (Grid.ContainsKey(faction))
        {
            Grid[faction] = new RoomModel[data.GridWidth, data.GridHeight];
        }
        else
        {
            Grid.Add(faction, new RoomModel[data.GridWidth, data.GridHeight]);
        }
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

    public RoomModel GetRoomAt(Vector2Int position, Faction faction)
    {
        if (IsPositionValid(position))
        {
            return Grid[faction][position.x, position.y];
        }
        return null;
    }
}
