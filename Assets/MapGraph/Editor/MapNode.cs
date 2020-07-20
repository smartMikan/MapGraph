using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class MapNode : Node
{
    public string GUID;

    public string MapText;

    public bool EntryPoint = false;

    public Vector2 Postion;

    public RoomData roomType;
}
