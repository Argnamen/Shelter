using UnityEngine;
using System.Collections.Generic;

public class DungeonView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _roomsContainer;
    [SerializeField] private Transform _heroesContainer;
    [SerializeField] private GameObject _gridCellPrefab;

    [Header("Grid Settings")]
    [SerializeField] private int _gridWidth = 5;
    [SerializeField] private int _gridHeight = 5;
    [SerializeField] private float _cellSize = 2.5f;

    private GameObject[,] _gridCells;
    private bool _isGridInitialized = false;

    public int GridWidth => _gridWidth;
    public int GridHeight => _gridHeight;
    public float CellSize => _cellSize;

    public void InitializeGrid()
    {
        if (_isGridInitialized)
        {
            Debug.LogWarning("Grid already initialized!");
            return;
        }

        _gridCells = new GameObject[_gridWidth, _gridHeight];

        for (int x = 0; x < _gridWidth; x++)
        {
            for (int y = 0; y < _gridHeight; y++)
            {
                CreateGridCell(x, y);
            }
        }

        _isGridInitialized = true;
        Debug.Log("Grid initialized successfully");
    }

    private void CreateGridCell(int x, int y)
    {
        var cellPos = new Vector3(x * _cellSize, y * _cellSize, 0);
        var cell = Instantiate(_gridCellPrefab, cellPos, Quaternion.identity, _roomsContainer);
        cell.name = $"GridCell_{x}_{y}";
        _gridCells[x, y] = cell;

        var renderer = cell.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            var color = renderer.color;
            color.a = 0.3f;
            renderer.color = color;
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

    public void HighlightAvailablePositions(IReadOnlyList<Vector2Int> positions)
    {
        ResetGridHighlight();

        foreach (var pos in positions)
        {
            if (IsValidGridPosition(pos))
            {
                var cell = _gridCells[pos.x, pos.y];
                var renderer = cell.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.color = Color.green;
                }
            }
        }
    }

    public void ResetGridHighlight()
    {
        for (int x = 0; x < _gridWidth; x++)
        {
            for (int y = 0; y < _gridHeight; y++)
            {
                var renderer = _gridCells[x, y].GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.color = new Color(1, 1, 1, 0.3f);
                }
            }
        }
    }

    public RoomView CreateRoomView(RoomType type, Vector3 worldPosition)
    {
        var prefabPath = $"Rooms/{type}Room";
        var prefab = Resources.Load<RoomView>(prefabPath);

        if (prefab == null)
        {
            Debug.LogError($"Prefab not found: {prefabPath}");
            return null;
        }

        var room = Instantiate(prefab, worldPosition, Quaternion.identity, _roomsContainer);
        room.name = $"{type}Room_{worldPosition.x}_{worldPosition.y}";

        // Скрываем соответствующую клетку сетки
        var gridPos = WorldToGridPosition(worldPosition);
        if (IsValidGridPosition(gridPos))
        {
            _gridCells[gridPos.x, gridPos.y].SetActive(false);
        }

        return room;
    }

    public HeroView CreateHeroView()
    {
        var prefab = Resources.Load<HeroView>("Heroes/Hero_1");
        return Instantiate(prefab, _heroesContainer);
    }

    private bool IsValidGridPosition(Vector2Int position)
    {
        return position.x >= 0 && position.x < _gridWidth &&
               position.y >= 0 && position.y < _gridHeight;
    }
}