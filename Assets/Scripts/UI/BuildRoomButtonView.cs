using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildRoomButtonView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _textMeshProUGUI;
    [SerializeField] private Image _image;

    public Button BayButton;

    public void UpdateText(string ButtonName, int cost)
    {
        _textMeshProUGUI.text = $"{ButtonName}\n{cost}";
    }

    public void UpdateIcon(Sprite sprite)
    {
        _image.overrideSprite = sprite;
    }
}
