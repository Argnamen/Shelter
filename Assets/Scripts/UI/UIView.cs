using R3;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UIView : MonoBehaviour
{
    [Header("Main UI")]
    [SerializeField] private Button _buildButton;
    [SerializeField] private Button _playButton;
    [SerializeField] private TextMeshProUGUI _playButtonText;
    [SerializeField] private TextMeshProUGUI _goldText;
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private TextMeshProUGUI _heroesCountText;
    [SerializeField] private Slider _daySlider;
    [SerializeField] private WinUICollection _winUICollection;

    [Header("Build Menu")]
    [SerializeField] private GameObject _buildMenu;
    [SerializeField] private Button _closeBuildMenuButton;
    [SerializeField] private Button _deleteRoom;

    [SerializeField] private List<MonsterButtonView> _peaceRooms;
    [SerializeField] private List<MonsterButtonView> _monsterRooms;

    [Header("Room Selection")]
    [SerializeField] private GameObject _roomSelectionPanel;
    [SerializeField] private TextMeshProUGUI _selectedRoomText;

    [Header("Message System")]
    [SerializeField] private TextMeshProUGUI _messageText;
    [SerializeField] private float _messageDuration = 2f;

    [Header("Characters")]
    [SerializeField] private Button _playerCharacter;
    [SerializeField] private Button _enemy1Character;
    [SerializeField] private Button _enemy2Character;
    [SerializeField] private Button _enemy3Character;

    [Inject] private RoomsData _roomsData;

    private IDisposable _messageDisposable;

    private bool _isPlay = false;

    public Observable<Unit> OnBuildButtonClicked => _buildButton.OnClickAsObservable();
    public Observable<Unit> OnPlayButtonClicked => _playButton.OnClickAsObservable();
    public Observable<Unit> OnDeleteRoomButtonClicked => _deleteRoom.OnClickAsObservable();

    public Observable<Unit> OnSwitchToPlayer => _playerCharacter.OnClickAsObservable();
    public Observable<Unit> OnSwitchToEnemy1 => _enemy1Character.OnClickAsObservable();
    public Observable<Unit> OnSwitchToEnemy2 => _enemy2Character.OnClickAsObservable();
    public Observable<Unit> OnSwitchToEnemy3 => _enemy3Character.OnClickAsObservable();

    public float DayValue { get { return _daySlider.value; } set { _daySlider.value = value; } }

    public float WinGhost { get => _winUICollection.GhostSlider.value; set { _winUICollection.GhostSlider.value = value; } }
    public float WinInteres { get => _winUICollection.InteresSlider.value; set { _winUICollection.InteresSlider.value = value; } }
    public float WinGold { get => _winUICollection.GoldSlider.value; set { _winUICollection.GoldSlider.value = value; } }
    public float WinVlianie{ get => _winUICollection.VlianieSlider.value; set { _winUICollection.VlianieSlider.value = value; } }

    public Observable<RoomData> OnRoomSelected { get; private set; }
    public Observable<Unit> OnCloseBuildMenuClicked => _closeBuildMenuButton.OnClickAsObservable();

    private void Awake()
    {
        InitializeRoomSelection();
        ToggleBuildMenu(false);
    }

    private void InitializeRoomSelection()
    {
        // Создаем Subject для выбора типа комнаты
        var roomSelectionSubject = new Subject<RoomData>();
        var room = _roomsData.Rooms.Find(x => x.Type == RoomType.Combat && x.MonsterType == MonsterType.Slime);

        OnRoomSelected = roomSelectionSubject;

        SetupRoom(_monsterRooms[0], roomSelectionSubject, RoomType.Combat, MonsterType.Slime);

        SetupRoom(_monsterRooms[1], roomSelectionSubject, RoomType.Combat, MonsterType.Skeleton);

        SetupRoom(_monsterRooms[2], roomSelectionSubject, RoomType.Combat, MonsterType.Eagle);

        SetupRoom(_peaceRooms[0], roomSelectionSubject, RoomType.Rest);

        SetupRoom(_peaceRooms[1], roomSelectionSubject, RoomType.Treasure);

        SetupRoom(_peaceRooms[2], roomSelectionSubject, RoomType.Stairs);
    }

    private void SetupRoom(MonsterButtonView monsterButtonView, Subject<RoomData> roomSelectionSubject, RoomType roomType, MonsterType monsterType = MonsterType.None)
    {
        var room = _roomsData.Rooms.Find(x => x.Type == roomType && x.MonsterType == monsterType);

        monsterButtonView.BayButton.OnClickAsObservable().Subscribe(_ =>
        {
            roomSelectionSubject.OnNext(room);
        }).AddTo(this);

        if(monsterType != MonsterType.None)
            monsterButtonView.UpdateText(room.MonsterType.ToString(), room.Cost);
        else
            monsterButtonView.UpdateText(room.Type.ToString(), room.Cost);
    }

    public void OnPlay()
    {
        _isPlay = !_isPlay;

        if (_isPlay)
        {
            _playButtonText.text = "Stop";
        }
        else
        {
            _playButtonText.text = "Play";
        }
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
        _deleteRoom.gameObject.SetActive(show);
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