using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class NodeSearchWindow : ScriptableObject,ISearchWindowProvider
{
    private MapGraphView _graphView;
    private EditorWindow _window;
    private Texture2D _indentationIcon;
    public void Init(EditorWindow window,MapGraphView graphView)
    {
        _window = window;
        _graphView = graphView;

        _indentationIcon = new Texture2D(1, 1);
        _indentationIcon.SetPixel(0, 0, new Color(0, 0, 0, 0));
        _indentationIcon.Apply();
    }


    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        var tree = new List<SearchTreeEntry> 
        {
            new SearchTreeGroupEntry(new GUIContent("Create Element"),0),
            new SearchTreeGroupEntry(new GUIContent("Map Node"),1),
            new SearchTreeEntry(new GUIContent("Map Node",_indentationIcon))
            {
                userData = new MapNode(),level = 2
            },

        };
        return tree;
    }

    public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
    {
        //var worldMousePosition = _window.rootVisualElement.ChangeCoordinatesTo(_window.rootVisualElement.parent, context.screenMousePosition - _window.position.position);
        //var localMousePosition = _graphView.contentViewContainer.WorldToLocal(worldMousePosition);

        //switch (SearchTreeEntry.userData)
        //{
        //    case MapNode _:
        //        _graphView.CreateNode("Map Node",localMousePosition);
        //        return true;
        //    default:
        //        break;
        //}

        return true;
    }

    
}
