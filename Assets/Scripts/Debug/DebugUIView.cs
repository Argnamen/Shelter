using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebugUIView : MonoBehaviour
{
    [SerializeField] private UIView _uiView;
    [SerializeField] private TextMeshProUGUI _debugText;

    private void Start()
    {
        if (_uiView == null)
        {
            Debug.LogError("UIView is not assigned!");
            return;
        }

        // Подписываемся на события для отладки
        _uiView.OnBuildButtonClicked.Subscribe(_ =>
        {
            Debug.Log("Build button clicked!");
        }).AddTo(this);

        _uiView.OnPlayButtonClicked.Subscribe(_ =>
        {
            Debug.Log("Play button clicked!");
        }).AddTo(this);

        _uiView.OnRoomSelected.Subscribe(room =>
        {
            Debug.Log($"Room type selected: {room.Type}");
        }).AddTo(this);
    }

    private void Update()
    {
        // Проверяем ссылки в реальном времени
        if (_uiView == null)
        {
            _debugText.text = "UIView is NULL!";
        }
    }
}
