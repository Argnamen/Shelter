using UnityEngine;
using Cinemachine;
using R3;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _zoomSpeed = 2f;
    [SerializeField] private float _minZoom = 5f;
    [SerializeField] private float _maxZoom = 20f;

    [Header("Camera Bounds")]
    [SerializeField] private Vector2 _cameraBoundsMin = new Vector2(-10, -10);
    [SerializeField] private Vector2 _cameraBoundsMax = new Vector2(10, 10);

    private CompositeDisposable _disposables = new();
    private Vector3 _targetPosition;
    private CinemachineVirtualCamera _virtualCamera;
    private CinemachineBasicMultiChannelPerlin _noiseComponent;

    private void Awake()
    {
        _virtualCamera = GetComponentInChildren<CinemachineVirtualCamera>();
        _targetPosition = transform.position;

        if (_virtualCamera != null)
        {
            _noiseComponent = _virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }
    }

    private void Start()
    {
        SetupCameraControl();

        // Подписка на обновление камеры
        Observable.EveryUpdate()
            .Subscribe(_ => UpdateCamera())
            .AddTo(_disposables);
    }

    private void SetupCameraControl()
    {
        if (_virtualCamera != null)
        {
            _virtualCamera.Follow = null;
            _virtualCamera.LookAt = null;
        }
    }

    private void UpdateCamera()
    {
        HandleMovement();
        HandleZoom();
    }

    private void HandleMovement()
    {
        transform.position = Vector3.Lerp(transform.position, _targetPosition, _moveSpeed * Time.deltaTime);

        var clampedPosition = new Vector3(
            Mathf.Clamp(transform.position.x, _cameraBoundsMin.x, _cameraBoundsMax.x),
            Mathf.Clamp(transform.position.y, _cameraBoundsMin.y, _cameraBoundsMax.y),
            transform.position.z
        );

        transform.position = clampedPosition;
    }

    private void HandleZoom()
    {
        if (_virtualCamera == null || Input.mouseScrollDelta.y == 0) return;

        float zoomDelta = -Input.mouseScrollDelta.y * _zoomSpeed;
        float newZoom = Mathf.Clamp(_virtualCamera.m_Lens.OrthographicSize + zoomDelta, _minZoom, _maxZoom);
        _virtualCamera.m_Lens.OrthographicSize = newZoom;
    }

    public void MoveToPosition(Vector3 worldPosition)
    {
        _targetPosition = new Vector3(worldPosition.x, worldPosition.y, _targetPosition.z);
    }

    public void FocusOnRoom(Vector2Int roomGridPosition)
    {
        Vector3 roomWorldPosition = new Vector3(roomGridPosition.x * 2f, roomGridPosition.y * 2f, 0);
        MoveToPosition(roomWorldPosition);
    }

    public void ShakeCamera(float intensity = 1f, float duration = 0.3f)
    {
        if (_noiseComponent == null) return;

        _noiseComponent.m_AmplitudeGain = intensity;
        Observable.TimerFrame((int)(duration * 60))
            .Subscribe(_ => _noiseComponent.m_AmplitudeGain = 0f)
            .AddTo(_disposables);
    }

    // В CameraController добавьте:
    public void SetCameraBoundsBasedOnGrid(int gridWidth, int gridHeight, float padding = 2f)
    {
        _cameraBoundsMin = new Vector2(-padding, -padding);
        _cameraBoundsMax = new Vector2(gridWidth * 2f + padding, gridHeight * 2f + padding);
    }

    private void OnDestroy()
    {
        _disposables.Dispose();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(
            new Vector3((_cameraBoundsMin.x + _cameraBoundsMax.x) * 0.5f,
                       (_cameraBoundsMin.y + _cameraBoundsMax.y) * 0.5f, 0),
            new Vector3(_cameraBoundsMax.x - _cameraBoundsMin.x,
                       _cameraBoundsMax.y - _cameraBoundsMin.y, 1)
        );
    }
#endif
}