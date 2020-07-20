using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "MapData")]
[Serializable]
public class MapContainer : ScriptableObject
{
    public List<MapNodeData> nodedata = new List<MapNodeData>();
    public List<NodeLinkedData> linkdata = new List<NodeLinkedData>();
    //public List<ExposedProperty> ExposedProperties = new List<ExposedProperty>();
}
