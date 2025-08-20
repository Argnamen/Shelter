using UnityEngine;
using System.Collections.Generic;

public class DungeonGrid : MonoBehaviour
{
    [SerializeField] private int _gridWidth = 5;
    [SerializeField] private int _gridHeight = 5;
    [SerializeField] private float _cellSize = 2f;

    private GridCell[,] _grid;
    private Vector3 _gridOrigin;

    public int GridWidth => _gridWidth;
    public int GridHeight => _gridHeight;
    public float CellSize => _cellSize;

    public event System.Action<Vector2Int> OnCellOccupied;
    public event System.Action<Vector2Int> OnCellFreed;

    private void Awake()
    {
        InitializeGrid();
        _gridOrigin = transform.position;
    }

    private void InitializeGrid()
    {
        _grid = new GridCell[_gridWidth, _gridHeight];

        for (int x = 0; x < _gridWidth; x++)
        {
            for (int y = 0; y < _gridHeight; y++)
            {
                // Центральная линия - для лестниц
                var allowedType = x == _gridWidth / 2 ? RoomType.Stairs : RoomType.Combat;
                _grid[x, y] = new GridCell(new Vector2Int(x, y), allowedType);
            }
        }
    }

    public bool CanPlaceRoom(Vector2Int gridPosition, RoomType roomType)
    {
        if (!IsValidGridPosition(gridPosition))
            return false;

        var cell = _grid[gridPosition.x, gridPosition.y];

        // Проверяем, можно ли разместить комнату этого типа
        if (cell.AllowedRoomType != roomType && roomType != RoomType.Combat)
            return false;

        return !cell.IsOccupied;
    }

    public bool TryPlaceRoom(Vector2Int gridPosition, RoomModel room)
    {
        if (!CanPlaceRoom(gridPosition, room.Type))
            return false;

        var cell = _grid[gridPosition.x, gridPosition.y];
        cell.IsOccupied = true;
        cell.OccupiedRoom = room;

        OnCellOccupied?.Invoke(gridPosition);
        return true;
    }

    public Vector3 GridToWorldPosition(Vector2Int gridPosition)
    {
        return _gridOrigin + new Vector3(
            gridPosition.x * _cellSize,
            gridPosition.y * _cellSize,
            0
        );
    }

    public Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        Vector3 localPos = worldPosition - _gridOrigin;
        return new Vector2Int(
            Mathf.RoundToInt(localPos.x / _cellSize),
            Mathf.RoundToInt(localPos.y / _cellSize)
        );
    }

    public GridCell GetCell(Vector2Int gridPosition)
    {
        if (IsValidGridPosition(gridPosition))
            return _grid[gridPosition.x, gridPosition.y];
        return null;
    }

    private bool IsValidGridPosition(Vector2Int position)
    {
        return position.x >= 0 && position.x < _gridWidth &&
               position.y >= 0 && position.y < _gridHeight;
    }

    public List<Vector2Int> GetAvailablePositionsForRoomType(RoomType roomType)
    {
        var availablePositions = new List<Vector2Int>();

        for (int x = 0; x < _gridWidth; x++)
        {
            for (int y = 0; y < _gridHeight; y++)
            {
                if (CanPlaceRoom(new Vector2Int(x, y), roomType))
                {
                    availablePositions.Add(new Vector2Int(x, y));
                }
            }
        }

        return availablePositions;
    }

    // Для отладки в редакторе
    private void OnDrawGizmos()
    {
        if (_grid == null) return;

        Gizmos.color = Color.white;
        for (int x = 0; x < _gridWidth; x++)
        {
            for (int y = 0; y < _gridHeight; y++)
            {
                var cell = _grid[x, y];
                var worldPos = GridToWorldPosition(new Vector2Int(x, y));

                // Рисуем ячейку
                Gizmos.DrawWireCube(worldPos, new Vector3(_cellSize, _cellSize, 0.1f));

                // Цвет в зависимости от занятости
                if (cell.IsOccupied)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawCube(worldPos, new Vector3(_cellSize * 0.9f, _cellSize * 0.9f, 0.1f));
                    Gizmos.color = Color.white;
                }
            }
        }
    }
}
