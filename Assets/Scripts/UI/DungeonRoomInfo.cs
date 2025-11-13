using System;
using TMPro;
using UnityEditor;
using UnityEngine;

public class DungeonRoomInfo : MonoBehaviour
{
    [NonSerialized] private TextMeshProUGUI RoomInfo;
    private void Awake()
    {
        RoomInfo = GetComponent<TextMeshProUGUI>();
    }
    public void UpdateRoomInfo(int clearedCount, int length)
    {
        RoomInfo.text= $"<color=#FF0000>Rooms</color>: {clearedCount}/{length}";
    }

}
