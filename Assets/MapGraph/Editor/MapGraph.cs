using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class MapGraph : EditorWindow
{
    private MapGraphView _graphView;
    private string _filename = "New Narrative";


    [MenuItem("GraphicMap/Window")]
    public static void OpenMapGraphWindow()
    {
        var window = GetWindow<MapGraph>();
        window.titleContent = new GUIContent(text: "Map Graph");
    }


    private void OnEnable()
    {
        ConstructGraph();
        GenerateToolBar();
        GenerateMiniMap();
        //GenerateBlackboard();
    }


    private void OnDisable()
    {
        if (_graphView != null)
        {
            rootVisualElement.Remove(_graphView);
        }

    }


    private void ConstructGraph()
    {
        _graphView = new MapGraphView(this)
        {
            name = "GraphView"
        };

        _graphView.StretchToParentSize();
        rootVisualElement.Add(_graphView);

    }
    private void GenerateToolBar()
    {
        var toolbar = new Toolbar();
        toolbar.styleSheets.Add(Resources.Load<StyleSheet>("MapGraph"));

        ////FileName TextField
        //var fileNameTextField = new TextField("File Name");
        //fileNameTextField.SetValueWithoutNotify(_filename);
        //fileNameTextField.MarkDirtyRepaint();
        //fileNameTextField.RegisterValueChangedCallback(evt => _filename = evt.newValue);
        //toolbar.Add(fileNameTextField);
        //////
        var textbox = new TextElement
        {
            text = _filename
        };
        textbox.style.color = Color.red;
        var objloader = new ObjectField("Data")
        {
            objectType = typeof(MapContainer),
            allowSceneObjects = false
        };
        objloader.RegisterValueChangedCallback(evt =>
        {

            var saveUtility = GraphSaveUtility.GetInstance(_graphView);
            //saveUtility.SaveGraph(_filename);
            if (evt.newValue == null)
            {
                _filename = "";
                textbox.text = _filename;
                return;
            }
            else
            {
                saveUtility.LoadGraph((MapContainer)evt.newValue);
                _filename = evt.newValue.name;
                textbox.text = _filename;
            }
        });

        toolbar.Add(objloader);
        objloader.style.alignSelf = Align.FlexEnd;
        //toolbar.Add(textbox);
        ////////


        toolbar.Add(new Button(() => RequestDataOperation(true))
        {
            text = "Save Data"
        });

        toolbar.Add(new Button(() => RequestDataOperation(false))
        {
            text = "Load Data"
        });


        ////Create Node Button
        //var nodeCreateButton = new Button(clickEvent: () =>
        //{
        //    _graphView.CreateNode("Map Node");
        //});

        //nodeCreateButton.text = "Create Node";
        //toolbar.Add(nodeCreateButton);

        var roomloader = new ObjectField("RoomData")
        {
            objectType = typeof(RoomData),
            allowSceneObjects = false
        };

        toolbar.Add(roomloader);




        //Create RoomNode Button
        var nodeCreateButton = new Button(clickEvent: () =>
        {
            _graphView.CreateRoomNode((RoomData)roomloader.value, Vector2.zero, true);
        })
        {
            text = "Create Node"
        };
        toolbar.Add(nodeCreateButton);





        rootVisualElement.Add(toolbar);
    }




    //private void GenerateBlackboard()
    //{
    //    var blackBoard = new Blackboard(_graphView);
    //    blackBoard.Add(new BlackboardSection { title = "Exposed Properties" });
    //    blackBoard.addItemRequested = _blackBoard =>
    //    {
    //        _graphView.AddPropertyToBlackBoard(new ExposedProperty());
    //    };

    //    blackBoard.editTextRequested = (blackboard1, element, newValue) =>
    //    {
    //        var oldPropertyName = ((BlackboardField)element).text;
    //        //access to graphview data
    //        if (_graphView.ExposedProperties.Any(x => x.PropertyName == newValue))
    //        {
    //            EditorUtility.DisplayDialog("Error", "This property name already exists, please chose another one", "OK");
    //            return;
    //        }
    //        //if no same name
    //        var propertyIndex = _graphView.ExposedProperties.FindIndex(x => x.PropertyName == oldPropertyName);
    //        _graphView.ExposedProperties[propertyIndex].PropertyName = newValue;
    //        //change UIElement View
    //        ((BlackboardField)element).text = newValue;
    //    };

    //    blackBoard.SetPosition(new Rect(10, 30, 200, 200));
    //    _graphView.Add(blackBoard);
    //    _graphView.Blackboard = blackBoard;
    //}

    private void RequestDataOperation(bool save)
    {
        if (string.IsNullOrEmpty(_filename))
        {
            EditorUtility.DisplayDialog("Invalid file name!", "Please enter a valid file name", "OK");
            return;
        }

        var saveUtility = GraphSaveUtility.GetInstance(_graphView);
        if (save)
        {

            saveUtility.SaveGraph(_filename);


        }
        else
        {
            saveUtility.LoadGraph(_filename);
        }
    }

    private void GenerateMiniMap()
    {
        var miniMap = new MiniMap
        {
            anchored = true
        };

        //This will give 10px offset from left side
        var cords = _graphView.contentViewContainer.WorldToLocal(new Vector2(this.maxSize.x - 10, 30));

        miniMap.SetPosition(new Rect(cords.x, cords.y, width: 200, height: 140));
        _graphView.Add(miniMap);
    }




}
