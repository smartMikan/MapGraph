using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UIElements;

public class GraphSaveUtility
{
    private MapGraphView _targetGraphView;
    private MapContainer _containerCache;

    private List<Edge> Edges => _targetGraphView.edges.ToList();
    private List<MapNode> Nodes => _targetGraphView.nodes.ToList().Cast<MapNode>().ToList();

   public static GraphSaveUtility GetInstance(MapGraphView targetGraphView)
    {
        return new GraphSaveUtility {
            _targetGraphView = targetGraphView
        };
    }

    public void SaveGraph(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            EditorUtility.DisplayDialog("NullFile", "Cant Save Null Object", "OK");
            return;
        }

        var MapContainer = ScriptableObject.CreateInstance<MapContainer>();

        if (!SaveNodes(MapContainer)) return;

        //SaveExposedProperties(MapContainer);

        //AutoCreateFolder
        if (!AssetDatabase.IsValidFolder("Assets/Resources/MapGraphDatas"))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }
            AssetDatabase.CreateFolder("Assets/Resources", "MapGraphDatas");
        }


        AssetDatabase.CreateAsset(MapContainer, $"Assets/Resources/MapGraphDatas/{fileName}.asset");
        AssetDatabase.SaveAssets();

    }

    

    public void LoadGraph(string fileName)
    {
        _containerCache = Resources.Load<MapContainer>("MapGraphDatas/" + fileName);

        if(_containerCache==null)
        {
            EditorUtility.DisplayDialog("File Not Found", "Target Map graph file does not exists!", "OK");
            return;
        }

        ClearGraph();
        CreateNode();
        ConnectNodes();
        //CreateExposedProperties();
    }


    public void LoadGraph(MapContainer container)
    {
        _containerCache = container;

        if (_containerCache == null)
        {
            EditorUtility.DisplayDialog("File Not Found", "Target Map graph file does not exists!", "OK");
            return;
        }

        ClearGraph();
        CreateNode();
        ConnectNodes();
        //CreateExposedProperties();
    }

   
    private void ClearGraph()
    {
        //Set entry points guid back from the save. Discard existing guid.
        //Nodes.Find(x => x.EntryPoint).GUID = _containerCache.linkdata[0].FromNodeGuid;

        foreach (var node in Nodes)
        {
            if (node.EntryPoint) continue;
            //remove nodes connected to this node
            Edges.Where(x => x.input.node == node).ToList().
                ForEach(edge => _targetGraphView.RemoveElement(edge));

            //remove the node
            _targetGraphView.RemoveElement(node);
        }


    }

    private void CreateNode()
    {
        foreach (var nodeData in _containerCache.nodedata)
        {
            //pass pos
            //var tempNode = _targetGraphView.CreateNode(nodeData.MapText, nodeData.Position);
            var tempNode = _targetGraphView.CreateRoomNode(nodeData.roomType, nodeData.Position,false);
            tempNode.GUID = nodeData.NodeGUID;
            
            foreach (PortData inportdata in nodeData.inputPorts)
            {
                _targetGraphView.AddInputPort(tempNode, inportdata.portName);
            }
            foreach (PortData outportdata in nodeData.outputPorts)
            {
                _targetGraphView.AddChoicePort(tempNode, outportdata.portName);
            }

            _targetGraphView.AddElement(tempNode);
        }


        //foreach (var node in Nodes)
        //{
        //    var outPort = _containerCache.linkdata.Where(x => x.FromNodeGuid == node.GUID).ToList();
        //    outPort.ForEach(outp => _targetGraphView.AddChoicePort(node, outp.outPortName));


        //    var inPort = _containerCache.linkdata.Where(x => x.TargetNodeGuid == node.GUID).ToList();
        //    inPort.ForEach(inp => _targetGraphView.AddInputPort(node, inp.inPortName));
        //}


    }

    private void ConnectNodes()
    {
        for (int i = 0; i < Nodes.Count; i++)
        {
            //pick link data of this node
            var connections = _containerCache.linkdata.Where(x => x.FromNodeGuid == Nodes[i].GUID).ToList();
            //loop for each conetion
            for (int j = 0; j < connections.Count; j++)
            {

                var fromPort = Nodes[i].outputContainer.Children().ToList().Cast<Port>().First(x=>x.portName == connections[j].outPortName);

                var targetNodeGuid = connections[j].TargetNodeGuid;
                var targetNode = Nodes.First(x => x.GUID == targetNodeGuid);

                var toPort = targetNode.inputContainer.Children().ToList().Cast<Port>().First(x => x.portName == connections[j].inPortName);

                LinkTwoNodes(fromPort, toPort);

                //targetNode.SetPosition(new Rect(_containerCache.nodedata.First(x => x.NodeGUID == targetNodeGuid).Position, _targetGraphView.defaultNodeSize));
            }
        
        }
    }

    /// <summary>
    /// Link Two Nodes' port
    /// </summary>
    /// <param name="output">from</param>
    /// <param name="input">to</param>
    private void LinkTwoNodes(Port output, Port input)
    {
        var tempEdge = new Edge
        {
            output = output,
            input = input
        };
        tempEdge.output.Connect(tempEdge);
        tempEdge.input.Connect(tempEdge);
        _targetGraphView.Add(tempEdge);
    }


    private bool SaveNodes(MapContainer MapContainer)
    {
        //if has edge
        if (Edges.Any())
        {
            //find linked output port (edges inputside != null dedicates that the output port has a link) 
            //note: edges input,output dedicate different nodes' port and there relation;
            //   ________               ________
            //   | Node1 |   edge(x)   | Node2 |
            //  in      out-----------in      out
            //   |_______|             |_______|
            var connectedEdges = Edges.Where(x => x.input.node != null).ToArray();

            //only save output port link message
            for (int i = 0; i < connectedEdges.Length; i++)
            {
                var outputNode = connectedEdges[i].output.node as MapNode;
                var inputNode = connectedEdges[i].input.node as MapNode;

                MapContainer.linkdata.Add(
                        new NodeLinkedData
                        {
                            FromNodeGuid = outputNode.GUID,
                            outPortName = connectedEdges[i].output.portName,
                            TargetNodeGuid = inputNode.GUID,
                            inPortName = connectedEdges[i].input.portName,
                        }
                );
            }
        }

       
        

        foreach (var MapNode in Nodes)
        {
            var mapNodeData = new MapNodeData
            {
                NodeGUID = MapNode.GUID,
                MapText = MapNode.MapText,
                Position = MapNode.GetPosition().position,
                roomType = MapNode.roomType
            };
            foreach (Port inputPort in MapNode.inputContainer.Children().ToList().Cast<Port>().ToList())
            {
                mapNodeData.inputPorts.Add(new PortData { portName = inputPort.portName,direction = Direction.Input});
            }

            foreach (Port outputPort in MapNode.outputContainer.Children().ToList().Cast<Port>().ToList())
            {
                mapNodeData.outputPorts.Add(new PortData { portName = outputPort.portName,direction = Direction.Output});
            }
            MapContainer.nodedata.Add(mapNodeData);

            

        }

        return true;
    }
    //private void SaveExposedProperties(MapContainer MapContainer)
    //{
    //    MapContainer.ExposedProperties.AddRange(_targetGraphView.ExposedProperties);
    //}

    //private void CreateExposedProperties()
    //{
    //    _targetGraphView.ClearBlackBoardAndExposedProperties();

    //    //Clear existing properties on hot-reload
    //    //Add properties from data
    //    foreach (var exposedProperty in _containerCache.ExposedProperties)
    //    {
    //        _targetGraphView.AddPropertyToBlackBoard(exposedProperty);
    //    }


    //}

}
