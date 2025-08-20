using UnityEngine;

public class GridVisualizer : MonoBehaviour
{
    [SerializeField] private DungeonGrid _dungeonGrid;
    [SerializeField] private GameObject _gridCellPrefab;
    [SerializeField] private Color _availableColor = Color.green;
    [SerializeField] private Color _occupiedColor = Color.red;
    [SerializeField] private Color _restrictedColor = Color.yellow;

    private GameObject[,] _gridVisuals;

    private void Start()
    {
        InitializeGridVisuals();
        _dungeonGrid.OnCellOccupied += UpdateCellVisual;
        _dungeonGrid.OnCellFreed += UpdateCellVisual;
    }

    private void InitializeGridVisuals()
    {
        _gridVisuals = new GameObject[_dungeonGrid.GridWidth, _dungeonGrid.GridHeight];

        for (int x = 0; x < _dungeonGrid.GridWidth; x++)
        {
            for (int y = 0; y < _dungeonGrid.GridHeight; y++)
            {
                var worldPos = _dungeonGrid.GridToWorldPosition(new Vector2Int(x, y));
                var cellVisual = Instantiate(_gridCellPrefab, worldPos, Quaternion.identity, transform);
                cellVisual.name = $"GridCell_{x}_{y}";

                _gridVisuals[x, y] = cellVisual;
                UpdateCellVisual(new Vector2Int(x, y));
            }
        }
    }

    private void UpdateCellVisual(Vector2Int gridPosition)
    {
        var cell = _dungeonGrid.GetCell(gridPosition);
        var visual = _gridVisuals[gridPosition.x, gridPosition.y];

        var spriteRenderer = visual.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) return;

        if (cell.IsOccupied)
        {
            spriteRenderer.color = _occupiedColor;
        }
        else
        {
            spriteRenderer.color = _availableColor;
        }
    }

    public void HighlightAvailablePositions(RoomType roomType)
    {
        for (int x = 0; x < _dungeonGrid.GridWidth; x++)
        {
            for (int y = 0; y < _dungeonGrid.GridHeight; y++)
            {
                var visual = _gridVisuals[x, y];
                var spriteRenderer = visual.GetComponent<SpriteRenderer>();

                var canPlace = _dungeonGrid.CanPlaceRoom(new Vector2Int(x, y), roomType);
                spriteRenderer.color = canPlace ? _availableColor : _restrictedColor;
            }
        }
    }

    public void ResetGridVisuals()
    {
        for (int x = 0; x < _dungeonGrid.GridWidth; x++)
        {
            for (int y = 0; y < _dungeonGrid.GridHeight; y++)
            {
                UpdateCellVisual(new Vector2Int(x, y));
            }
        }
    }

    private void OnDestroy()
    {
        if (_dungeonGrid != null)
        {
            _dungeonGrid.OnCellOccupied -= UpdateCellVisual;
            _dungeonGrid.OnCellFreed -= UpdateCellVisual;
        }
    }
}
