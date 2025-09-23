using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MonsterButtonView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _textMeshProUGUI;

    public Button BayButton;

    public void UpdateText(string ButtonName, int cost)
    {
        _textMeshProUGUI.text = $"{ButtonName}\n{cost}";
    }
}
