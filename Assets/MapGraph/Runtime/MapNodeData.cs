using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MapNodeData
{
    public RoomData roomType;
    public string NodeGUID;
    public string MapText;
    public Vector2 Position;
    public List<PortData> inputPorts = new List<PortData>();
    public List<PortData> outputPorts = new List<PortData>();

}


