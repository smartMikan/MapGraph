using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

public class MapGraphView : GraphView
{
    public readonly Vector2 defaultNodeSize = new Vector2(150, 150);
    public readonly Vector2 defaultNodePos = new Vector2(150, 150);

    public Blackboard Blackboard;

    //public List<ExposedProperty> ExposedProperties = new List<ExposedProperty>();


    private NodeSearchWindow _searchWindow;

    public MapGraphView(EditorWindow editowindow)
    {
        styleSheets.Add(Resources.Load<StyleSheet>("MapGraph"));

        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

        //allow drag
        this.AddManipulator(new ContentDragger());
        //allow select
        this.AddManipulator(new SelectionDragger());
        //allow rect select
        this.AddManipulator(new RectangleSelector());

        
        


        var grid = new GridBackground();
        
        Insert(0, grid);

        grid.StretchToParentSize();

        //AddElement(GenerateEntryPointNode());
        //AddSearchWindow(editowindow);
    }

   

    private void AddSearchWindow(EditorWindow editorWindow)
    {
        _searchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
        _searchWindow.Init(editorWindow,this);
        nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchWindow);
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        var compatiblePorts = new List<Port>();

        ports.ForEach((port) =>
        {
            if(startPort != port && startPort.node != port.node)
            {
                compatiblePorts.Add(port);

            }
        });


        return compatiblePorts;
    }




    private Port GeneratePort(MapNode node, Direction portDir, Port.Capacity capacity = Port.Capacity.Single)
    {
        return node.InstantiatePort(Orientation.Horizontal, portDir, capacity, type: typeof(uint));
    }



    private MapNode CreateNode(string nodeName,Vector2 position)
    {
        var node = CreateMapNode(nodeName, position);
        AddElement(node);
        return node;
    }

    


    private MapNode GenerateEntryPointNode()
    {
        var node = new MapNode
        {
            title = "start",
            GUID = Guid.NewGuid().ToString(),
            MapText = "EntryPoint",
            EntryPoint = true

        };

        var port = GeneratePort(node, Direction.Output);

        port.portName = "Next";

        node.outputContainer.Add(port);

        //Lock Entry Point
        node.capabilities &= ~Capabilities.Movable;
        node.capabilities &= ~Capabilities.Deletable;

        node.RefreshExpandedState();
        node.RefreshPorts();

        node.SetPosition(new Rect(x: 100, y: 100, width: 100, height: 100));

        return node;
    }

    private MapNode CreateMapNode(string nodeName,Vector2 position)
    {
        var mapNode = new MapNode
        {
            title = nodeName,
            MapText = nodeName,
            GUID = Guid.NewGuid().ToString(),
            Postion = position,
        };

        mapNode.styleSheets.Add(Resources.Load<StyleSheet>("Node"));

        //var inputPort = GeneratePort(MapNode, Direction.Input, Port.Capacity.Multi);
        //inputPort.portName = "Input";
        //MapNode.inputContainer.Add(inputPort);
        //AddInputPort(mapNode);

        //var inputbutton = new Button(() =>
        //{ AddInputPort(mapNode); })
        //{
        //    text = "New Input"
        //};

        //mapNode.titleContainer.Add(inputbutton);

        //var buttton = new Button(() =>
        //{ AddChoicePort(mapNode);})
        //{
        //    text = "New Choice"
        //};
        //mapNode.titleContainer.Add(buttton);


        var textField = new TextField(string.Empty);
        textField.RegisterValueChangedCallback(evt =>
        {
            mapNode.MapText = evt.newValue;
            mapNode.title = evt.newValue;
        });
        textField.SetValueWithoutNotify(mapNode.title);
        mapNode.mainContainer.Add(textField);

        mapNode.RefreshExpandedState();
        mapNode.RefreshPorts();
        mapNode.SetPosition(new Rect(position,defaultNodeSize));


        return mapNode;
    }

    public MapNode CreateRoomNode(RoomData data,Vector2 postion,bool first)
    {
        if(data == null)
        {
            return null;
        }
        var node = CreateNode(data.RoomName, postion);

        node.roomType = data;

        if (first)
        {
            foreach (var entryPoint in data.EntryPoints)
            {
                AddInputPort(node, entryPoint.MyName);
            }
        }

        return node;
    }

    public void AddInputPort(MapNode MapNode, string overridenPortName = "")
    {
        var generatedPort = GeneratePort(MapNode, Direction.Input, Port.Capacity.Single);

        var oldLabel = generatedPort.contentContainer.Q<Label>("type");

        var inputPortCount = MapNode.inputContainer.Query("connector").ToList().Count;


        while (MapNode.inputContainer.Children().Cast<Port>().Any(x => x.portName == overridenPortName))
        {
            //UserNameProperty,UserNameProperty(1),UserNameProperty(1)(1)...
            overridenPortName = $"{overridenPortName}(1)";
        }



        var choicePortName = string.IsNullOrEmpty(overridenPortName) ? $"Input {inputPortCount + 1}" : overridenPortName;




        var textField = new TextField
        {
            name = string.Empty,
            value = choicePortName
        };
        textField.RegisterValueChangedCallback((evt) => 
        {
            string newvalue = evt.newValue;
            while (MapNode.inputContainer.Children().Cast<Port>().Any(x => x.portName == newvalue))
            {
                //UserNameProperty,UserNameProperty(1),UserNameProperty(1)(1)...
                newvalue = $"{newvalue}(1)";
            }

            generatedPort.portName = newvalue; 
        });

        generatedPort.contentContainer.Add(new Label(" "));
        generatedPort.contentContainer.Add(textField);

        var switchButton = new Button(() => ChangePortSide(MapNode, generatedPort))
        {
            text = "->"
        };

        generatedPort.contentContainer.Add(switchButton);

        //var deleteButton = new Button(() => RemovePort(MapNode, generatedPort))
        //{
        //    text = "X"
        //};
        //generatedPort.contentContainer.Add(deleteButton);
        generatedPort.portName = choicePortName;
        generatedPort.name = choicePortName;

        MapNode.inputContainer.Add(generatedPort);
        MapNode.RefreshExpandedState();
        MapNode.RefreshPorts();
    }

    

    public void AddChoicePort(MapNode MapNode, string overridenPortName = "")
    {
        var generatedPort = GeneratePort(MapNode,Direction.Output,Port.Capacity.Single);

        var oldLabel = generatedPort.contentContainer.Q<Label>("type");

        var outputPortCount = MapNode.outputContainer.Query("connector").ToList().Count;

        while (MapNode.outputContainer.Children().Cast<Port>().Any(x => x.portName == overridenPortName))
        {
            //UserNameProperty,UserNameProperty(1),UserNameProperty(1)(1)...
            overridenPortName = $"{overridenPortName}(1)";
        }


        var choicePortName = string.IsNullOrEmpty(overridenPortName) ? $"Choice {outputPortCount +1}" : overridenPortName;

        var textField = new TextField
        {
            name = string.Empty,
            value = choicePortName
        };
        textField.RegisterValueChangedCallback((evt) =>
        {
            string newvalue = evt.newValue;
            while (MapNode.outputContainer.Children().Cast<Port>().Any(x => x.portName == newvalue))
            {
                //UserNameProperty,UserNameProperty(1),UserNameProperty(1)(1)...
                newvalue = $"{newvalue}(1)";
            }
            generatedPort.portName = newvalue;
        });



        generatedPort.contentContainer.Add(new Label(" "));
        generatedPort.contentContainer.Add(textField);

        var switchButton = new Button(() => ChangePortSide(MapNode, generatedPort))
        {
            text = "<-"
        };

        generatedPort.contentContainer.Add(switchButton);

        //var deleteButton = new Button(() => RemovePort(MapNode, generatedPort))
        //{
        //    text = "X"
        //};
        //generatedPort.contentContainer.Add(deleteButton);
        generatedPort.portName = choicePortName;
        generatedPort.name = choicePortName;

        MapNode.outputContainer.Add(generatedPort);
        MapNode.RefreshExpandedState();
        MapNode.RefreshPorts();
    }



    private void ChangePortSide(MapNode mapNode, Port generatedPort)
    {
        var targetEdge = edges.ToList().Where(x => generatedPort.direction == Direction.Output ? (x.output.portName == generatedPort.portName && x.output.node == generatedPort.node) : (x.input.portName == generatedPort.portName && x.input.node == generatedPort.node));

        if (!targetEdge.Any())
        {
            generatedPort.parent.Remove(generatedPort);
            if(generatedPort.direction == Direction.Output)
            {
                AddInputPort(mapNode, generatedPort.portName);
            }
            else
            {
                AddChoicePort(mapNode, generatedPort.portName);
            }

            //MapNode.outputContainer.Remove(generatedPort);
            mapNode.RefreshPorts();
            mapNode.RefreshExpandedState();
            return;
        }

        var edge = targetEdge.First();
        if (generatedPort.direction == Direction.Output)
        {
            edge.input.Disconnect(edge);
        }
        else
        {
            edge.output.Disconnect(edge);
        }
        RemoveElement(targetEdge.First());

        generatedPort.parent.Remove(generatedPort);
        if (generatedPort.direction == Direction.Output)
        {
            AddInputPort(mapNode, generatedPort.portName);
        }
        else
        {
            AddChoicePort(mapNode, generatedPort.portName);
        }

        //MapNode.outputContainer.Remove(generatedPort);
        mapNode.RefreshPorts();
        mapNode.RefreshExpandedState();

    }
    private void RemovePort(MapNode mapNode, Port generatedPort)
    {
        var targetEdge = edges.ToList().Where(x => generatedPort.direction == Direction.Output ? (x.output.portName == generatedPort.portName && x.output.node == generatedPort.node) : (x.input.portName == generatedPort.portName && x.input.node == generatedPort.node));

        if (!targetEdge.Any())
        {
            if(generatedPort.direction == Direction.Output)
            {
                mapNode.outputContainer.Remove(generatedPort);
            }
            else
            {
                mapNode.inputContainer.Remove(generatedPort);
            }
            
            mapNode.RefreshPorts();
            mapNode.RefreshExpandedState();
            return;
        }

        var edge = targetEdge.First();
        if (generatedPort.direction == Direction.Output)
        {
            edge.input.Disconnect(edge);
        }
        else
        {
            edge.output.Disconnect(edge);
        }
        RemoveElement(targetEdge.First());

        if (generatedPort.direction == Direction.Output)
        {
            mapNode.outputContainer.Remove(generatedPort);
        }
        else
        {
            mapNode.inputContainer.Remove(generatedPort);
        }
        //MapNode.outputContainer.Remove(generatedPort);
        mapNode.RefreshPorts();
        mapNode.RefreshExpandedState();
    }



    //public void ClearBlackBoardAndExposedProperties()
    //{
    //    ExposedProperties.Clear();
    //    //Unity-built-in
    //    Blackboard.Clear(); 
    //}



    ///// <summary>
    ///// BlackBoard AddItem CallBack
    ///// </summary>
    ///// <param name="exposedProperty"></param>
    //public void AddPropertyToBlackBoard(ExposedProperty exposedProperty)
    //{
    //    var localPropertyName = exposedProperty.PropertyName;
    //    var localPropertyValue = exposedProperty.PropertyValue;
    //    while (ExposedProperties.Any(x=>x.PropertyName == localPropertyName))
    //    {
    //        //UserNameProperty,UserNameProperty(1),UserNameProperty(1)(1)...
    //        localPropertyName = $"{localPropertyName}(1)";
    //    }

    //    //get data
    //    var property = new ExposedProperty
    //    {
    //        PropertyName = localPropertyName,
    //        PropertyValue = localPropertyValue
    //    };


    //    ExposedProperties.Add(property);
    //    //new value holder
    //    var container = new VisualElement();
    //    var blackboardField = new BlackboardField { text = property.PropertyName, typeText = "string property" };
    //    //property field
    //    container.Add(blackboardField);

    //    //property TextField
    //    var propertyValueTextField = new TextField("Value:")
    //    {
    //        value = localPropertyValue,
    //    };
    //    //On Value change
    //    propertyValueTextField.RegisterValueChangedCallback(evt =>
    //    {
    //        var changingPropertyIndex = ExposedProperties.FindIndex(x => x.PropertyName == property.PropertyName);
    //        //local saved property 
    //        ExposedProperties[changingPropertyIndex].PropertyValue = evt.newValue;
    //    });
    //    //
    //    container.Add(propertyValueTextField);
    //    //
    //    var blackBoardValueRow = new BlackboardRow(propertyValueTextField, propertyValueTextField);
    //    container.Add(blackBoardValueRow);
    //    Blackboard.Add(container);
    //}

    
}
