using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.TerrainAPI;

public class SceneLoader : MonoBehaviour
{
    public MapContainer container;

    List<RoomPointHolder> rooms = new List<RoomPointHolder>();

    

   

   

    public void InstatiateRoom()
    {
        rooms = new List<RoomPointHolder>();

        foreach (var roomNode in container.nodedata)
        {
            var roomObj = Instantiate(roomNode.roomType.RoomPrefab,new Vector3(roomNode.Position.x*0.5f, -roomNode.Position.y*0.5f,0),Quaternion.identity);
            var holder = roomObj.GetComponent<RoomPointHolder>();
            holder.roomGUID = roomNode.NodeGUID;
            rooms.Add(holder);
        }
    }


    public void LinkRooms()
    {

        for (int i = 0; i < rooms.Count; i++)
        {
            //pick link data of this node
            var connections = container.linkdata.Where(x => x.FromNodeGuid == rooms[i].roomGUID).ToList();
            //loop for each conetion
            for (int j = 0; j < connections.Count; j++)
            {

                var fromPoint = rooms[i].entryPoints.First(x => x.Myname == connections[j].outPortName);

                var targetRoomGuid = connections[j].TargetNodeGuid;
                var targetRoom = rooms.First(x => x.roomGUID == targetRoomGuid);

                var toPoint = targetRoom.entryPoints.First(x => x.Myname == connections[j].inPortName);

                LinkTwoEntryPoint(fromPoint, toPoint);

            }
        }
    }

    /// <summary>
    /// Link Two EntryPoint
    /// </summary>
    /// <param name="from">from</param>
    /// <param name="to">to</param>
    private void LinkTwoEntryPoint(StageEntryPoint from, StageEntryPoint to)
    {
        from.myTargetPoint = to;
        to.myTargetPoint = from;
    }



}
