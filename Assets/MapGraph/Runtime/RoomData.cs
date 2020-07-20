using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MapDatas/RoomData")]
[System.Serializable]
public class RoomData :ScriptableObject
{
    public GameObject RoomPrefab;

    public string RoomName;
    [HideInInspector]
    public string RoomGUID;

    public List<EntryPointData> EntryPoints = new List<EntryPointData>();

}