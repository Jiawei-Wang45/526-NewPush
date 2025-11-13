using System;
using TMPro;
using UnityEngine;

public class SurvivalTime : MonoBehaviour
{
    [NonSerialized] private TextMeshProUGUI survivalTime;
    private void Awake()
    {
        survivalTime = GetComponent<TextMeshProUGUI>();
    }
    public void UpdateSurvivalTime(float time)
    {
        survivalTime.text= $"<size=20><color=#FF0000>Time Survived: </color>{time:F1}s</size>";
    }
}
