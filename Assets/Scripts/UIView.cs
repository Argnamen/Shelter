using R3;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIView : MonoBehaviour
{
    [Header("Main UI")]
    [SerializeField] private Button _buildButton;
    [SerializeField] private Button _playButton;
    [SerializeField] private TextMeshProUGUI _goldText;
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private TextMeshProUGUI _heroesCountText;

    [Header("Build Menu")]
    [SerializeField] private GameObject _buildMenu;
    [SerializeField] private Button _combatRoomButton;
    [SerializeField] private Button _restRoomButton;
    [SerializeField] private Button _treasureRoomButton;
    [SerializeField] private Button _stairsRoomButton;
    [SerializeField] private Button _closeBuildMenuButton;

    [Header("Room Selection")]
    [SerializeField] private GameObject _roomSelectionPanel;
    [SerializeField] private TextMeshProUGUI _selectedRoomText;

    [Header("Message System")]
    [SerializeField] private TextMeshProUGUI _messageText;
    [SerializeField] private float _messageDuration = 2f;

    private IDisposable _messageDisposable;

    public Observable<Unit> OnBuildButtonClicked => _buildButton.OnClickAsObservable();
    public Observable<Unit> OnPlayButtonClicked => _playButton.OnClickAsObservable();

    public Observable<RoomType> OnRoomTypeSelected { get; private set; }
    public Observable<Unit> OnCloseBuildMenuClicked => _closeBuildMenuButton.OnClickAsObservable();

    private void Awake()
    {
        InitializeRoomSelection();
    }

    private void InitializeRoomSelection()
    {
        // Создаем Subject для выбора типа комнаты
        var roomSelectionSubject = new Subject<RoomType>();
        OnRoomTypeSelected = roomSelectionSubject;

        // Подписываем кнопки комнат
        _combatRoomButton.OnClickAsObservable()
            .Subscribe(_ => roomSelectionSubject.OnNext(RoomType.Combat))
            .AddTo(this);

        _restRoomButton.OnClickAsObservable()
            .Subscribe(_ => roomSelectionSubject.OnNext(RoomType.Rest))
            .AddTo(this);

        _treasureRoomButton.OnClickAsObservable()
            .Subscribe(_ => roomSelectionSubject.OnNext(RoomType.Treasure))
            .AddTo(this);

        _stairsRoomButton.OnClickAsObservable()
            .Subscribe(_ => roomSelectionSubject.OnNext(RoomType.Stairs))
            .AddTo(this);
    }

    public void UpdateGold(int amount)
    {
        _goldText.text = $"Gold: {amount}";
    }

    public void UpdateLevel(int level)
    {
        _levelText.text = $"Level: {level}";
    }

    public void UpdateHeroesCount(int count)
    {
        _heroesCountText.text = $"Heroes: {count}";
    }

    public void ToggleBuildMenu(bool show)
    {
        _buildMenu.SetActive(show);
        _roomSelectionPanel.SetActive(show);
    }

    public bool IsBuildMenuActive()
    {
        return _buildMenu.activeSelf;
    }

    public void SetSelectedRoomText(RoomType roomType)
    {
        _selectedRoomText.text = $"Selected: {roomType}";
        _selectedRoomText.color = GetRoomColor(roomType);
    }

    private Color GetRoomColor(RoomType roomType)
    {
        return roomType switch
        {
            RoomType.Combat => Color.red,
            RoomType.Rest => Color.blue,
            RoomType.Treasure => Color.yellow,
            RoomType.Stairs => Color.green,
            _ => Color.white
        };
    }

    public void ShowMessage(string message, float duration = -1)
    {
        if (_messageText == null) return;

        _messageDisposable?.Dispose();
        _messageText.text = message;
        _messageText.gameObject.SetActive(true);

        if (duration > 0)
        {
            _messageDisposable = Observable.Timer(TimeSpan.FromSeconds(duration))
                .Subscribe(_ => _messageText.gameObject.SetActive(false));
        }
    }

    public void ShowTemporaryMessage(string message, float duration = 2f)
    {
        ShowMessage(message, duration);
    }
}