using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomPointHolder : MonoBehaviour
{
    public string RoomName;
    public string roomGUID;


    public List<StageEntryPoint> entryPoints = new List<StageEntryPoint>();

    public PolygonCollider2D cameraBound;


    
}
