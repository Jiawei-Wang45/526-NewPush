using System;
using TMPro;
using UnityEngine;

public class SurvivalTime : MonoBehaviour
{
    [NonSerialized] private TextMeshProUGUI survivalTime;
    [SerializeField] private int fontSize;
    private void Awake()
    {
        survivalTime = GetComponent<TextMeshProUGUI>();
    }
    public void UpdateSurvivalTime(float time)
    {
        survivalTime.text= $"<size={fontSize}><color=#FF0000>Time Survived: </color>{time:F1}s</size>";
    }
}
