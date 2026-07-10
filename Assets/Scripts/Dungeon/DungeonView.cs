using UnityEngine;
using System.Collections.Generic;

public class DungeonView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _gridContainer;
    [SerializeField] private Transform _heroesContainer;
    [SerializeField] private GameObject _gridCellPrefab;

    [SerializeField] private Transform[] _roomsContainers;

    [Header("StartPoint")]
    public Transform StartPoint;
    [SerializeField] private Transform _spawnPoint;

    public GameObject[,] GridCells;
    private bool _isGridInitialized = false;

    private int _gridWidth;
    private int _gridHeight;
    private float _cellSize;

    public void InitializeGrid(GameData gameData)
    {
        _gridWidth = gameData.GridWidth;
        _gridHeight = gameData.GridHeight;
        _cellSize = gameData.CellSize;

        if (_isGridInitialized)
        {
            Debug.LogWarning("Grid already initialized!");
            return;
        }

        GridCells = new GameObject[_gridWidth, _gridHeight];

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

    public void ActivateRoomsContainer(Faction faction)
    {
        for (int i = 0; i < _roomsContainers.Length; i++)
        {
            _roomsContainers[i].gameObject.SetActive((Faction)i == faction);
        }

        ActivateGridContainer(faction == Faction.Player);
    }

    public void ActivateGridContainer(bool active)
    {
        _gridContainer.gameObject.SetActive(active);
    }

    private void CreateGridCell(int x, int y)
    {
        var cellPos = new Vector3(x * _cellSize, y * _cellSize, 0);
        var cell = Instantiate(_gridCellPrefab, cellPos, Quaternion.identity, _gridContainer);
        cell.transform.localPosition = cellPos;
        cell.name = $"GridCell_{x}_{y}";
        GridCells[x, y] = cell;

        var renderer = cell.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            var color = renderer.color;
            color.a = 0.3f;
            renderer.color = color;
        }
    }

    public Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        int x = Mathf.RoundToInt((worldPosition.x / _cellSize));
        int y = Mathf.RoundToInt((10 - worldPosition.y) / _cellSize);

        return new Vector2Int(x, y);
    }

    public void HighlightAvailablePositions(IReadOnlyList<Vector2Int> positions)
    {
        ResetGridHighlight();

        foreach (var pos in positions)
        {
            if (IsValidGridPosition(pos))
            {
                var cell = GridCells[pos.x, pos.y];
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
                var renderer = GridCells[x, y].GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.color = new Color(1, 1, 1, 0.3f);
                }
            }
        }
    }

    public RoomView CreateRoomView(Faction faction, RoomType type, Vector3 worldPosition)
    {
        var prefabPath = $"Rooms/{type}Room";
        var prefab = Resources.Load<RoomView>(prefabPath);

        if (prefab == null)
        {
            Debug.LogError($"Prefab for {faction} not found: {prefabPath}");
            return null;
        }

        var room = Instantiate(prefab, worldPosition, Quaternion.identity, _roomsContainers[(int)faction]);
        room.name = $"{type}Room_{worldPosition.x}_{worldPosition.y}";

        // Скрываем соответствующую клетку сетки
        var gridPos = WorldToGridPosition(worldPosition);
        if (IsValidGridPosition(gridPos) && faction == Faction.Player)
        {
            GridCells[gridPos.x, gridPos.y].SetActive(false);
        }

        return room;
    }

    public void RemoveRoomView(Vector2Int roomPosition, Faction faction)
    {
        var gridPos = roomPosition;

        if (IsValidGridPosition(gridPos) && faction == Faction.Player)
        {
            GridCells[gridPos.x, gridPos.y].SetActive(true);
        }
    }

    public HeroView CreateHeroView(GameObject prefab, Faction faction)
    {
        var view = Instantiate<HeroView>(prefab.GetComponent<HeroView>(), _roomsContainers[(int)faction]);
        view.transform.position = _spawnPoint.position;
        return view;
    }

    private bool IsValidGridPosition(Vector2Int position)
    {
        return position.x >= 0 && position.x < _gridWidth &&
               position.y >= 0 && position.y < _gridHeight;
    }
}