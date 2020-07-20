using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

[CustomEditor(typeof(SceneLoader))]
public class SceneLoaderInspector : Editor
{
    SceneLoader loader;
    private void OnEnable()
    {
        loader = (SceneLoader)target;
    }


    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.BeginVertical();


        if (loader.container != null)
        {
            if (GUILayout.Button("Build!"))
            {
                loader.InstatiateRoom();
                loader.LinkRooms();
            }
        }
        EditorGUILayout.EndVertical();

    }
}
