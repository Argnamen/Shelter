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

    public Observable<Unit> OnBuildButtonClicked => _buildButton.OnClickAsObservable();
    public Observable<Unit> OnPlayButtonClicked => _playButton.OnClickAsObservable();

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
