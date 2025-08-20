using R3;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIView : MonoBehaviour
{
    [SerializeField] private Button _buildButton;
    [SerializeField] private Button _playButton;
    [SerializeField] private TextMeshProUGUI _goldText;
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private GameObject _buildMenu;

    [SerializeField] private Button _combatRoomButton;
    [SerializeField] private Button _restRoomButton;
    [SerializeField] private Button _treasureRoomButton;
    [SerializeField] private Button _stairsRoomButton;

    public Observable<Unit> OnBuildButtonClicked => _buildButton.OnClickAsObservable();
    public Observable<Unit> OnPlayButtonClicked => _playButton.OnClickAsObservable();
    public Observable<Unit> OnCombatRoomSelected => _combatRoomButton.OnClickAsObservable();
    public Observable<Unit> OnRestRoomSelected => _restRoomButton.OnClickAsObservable();
    public Observable<Unit> OnTreasureRoomSelected => _treasureRoomButton.OnClickAsObservable();
    public Observable<Unit> OnStairsRoomSelected => _stairsRoomButton.OnClickAsObservable();

    public void HideBuildMenu()
    {
        _buildMenu.SetActive(false);
    }

    public void UpdateGold(int amount)
    {
        _goldText.text = $"Gold: {amount}";
    }

    public void UpdateLevel(int level)
    {
        _levelText.text = $"Level: {level}";
    }

    public void ToggleBuildMenu(bool show)
    {
        _buildMenu.SetActive(show);
    }
}
