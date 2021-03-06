﻿/* 
 * Original Author: Paul J. Manley
 * Modified By:
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Path))] // Tells the Editor to only run this script if the selected object is a type of it.
public class PathGUI : Editor
{
    Path _path;
    List<bool> _toggleGroup;
    private static CustomColor[] _colors = new CustomColor[(int)PathPoint.PointBehavior.End + 1]; // The color handle representation of the points. Made static so all points are consistent and easy to identify across all instances.

    private void OnEnable()
    { // Makes sure no necessary data types are empty and if they are then make a new one of it.

        SceneView.onSceneGUIDelegate += OnSceneGUI; // Adds the OnSceneGUI to the delegate to allow rendering in editor.

        _path = (Path)target;
        EditorUtility.SetDirty(_path);

        if (_path.pathPoints == null)
            _path.pathPoints = new List<PathPoint>();

        _toggleGroup = new List<bool>(); // creates the new toggleGroup

        for (int i = 0; i < _path.pathPoints.Capacity; i++)
        { // intializes the new toggleGroup
            _toggleGroup.Add(false);
            _toggleGroup.TrimExcess(); // clears any junk data automatically added on <List>.Add()
        }

        //Debug.Log(Resources.FindObjectsOfTypeAll(typeof(CustomColor)));
        //Debug.Log(Resources.FindObjectsOfTypeAll(typeof(CustomColor)).Length);
        Object[] l_colors = Resources.FindObjectsOfTypeAll(typeof(CustomColor));
        foreach (CustomColor c in l_colors)
        {
            switch (c.name)
            {
                case "Start":
                    _colors[0] = c;
                    break;
                case "End":
                    _colors[_colors.Length - 1] = c;
                    break;
                case "Idle":
                    _colors[1] = c;
                    break;
                case "Interact":
                    _colors[3] = c;
                    break;
                case "PassThrough":
                    _colors[2] = c;
                    break;
                default:
                    break;
            }
        }
    }

    private void OnDisable()
    {
        SceneView.onSceneGUIDelegate -= OnSceneGUI; // Clears the OnSceneGUI to the delegate to disabling the render in editor.
        _toggleGroup.Clear(); // clears the toggleGroup of all data
        _toggleGroup.TrimExcess(); // ensures no junk data is maintained in the list.
    }

    public override void OnInspectorGUI()
    {
        PathPoint.PointBehavior l_pb = 0;
        for (int i = 0; i < _colors.Length; i++)
        {// Layouts the color fields that are associated with the behaviourPoints.
            GUILayout.BeginHorizontal();
            l_pb = (PathPoint.PointBehavior)i;
            GUILayout.Label(l_pb.ToString());
            _colors[i] = (CustomColor)EditorGUILayout.ObjectField(_colors[i], typeof(CustomColor), false, GUILayout.Width(Screen.width / 2));
            //_colors[i] = EditorGUILayout.ColorField(_colors[i], GUILayout.Width(Screen.width / 2));
            GUILayout.EndHorizontal();

            if (_colors[i] != null)
            {
                EditorUtility.SetDirty(_colors[i]);
                _colors[i].color = EditorGUILayout.ColorField(_colors[i].color);
            }
        }

        GUILayout.Label("Number of points: " + _path.pathPoints.Capacity); // spot to see the total number of points in the list.

        #region Buttons!
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Point"))
        {
            _path.pathPoints.Add(new PathPoint());
            _toggleGroup.Add(false);
        }

        if (GUILayout.Button("Remove Point"))
        {
            _toggleGroup.Remove(_toggleGroup[_toggleGroup.Capacity - 1]);
            _path.pathPoints.Remove(_path.pathPoints[_path.pathPoints.Capacity - 1]);
        }

        if (GUILayout.Button("Clear List"))
        {
            _path.pathPoints.Clear();
            _toggleGroup.Clear();
        }

        GUILayout.EndHorizontal();

        _path.pathPoints.TrimExcess();
        _toggleGroup.TrimExcess();

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        #endregion

        for (int i = 0; i < _path.pathPoints.Capacity; i++)
        { // Displayes the point information via the toggle group.
            GUILayout.BeginHorizontal();
            _toggleGroup[i] = EditorGUILayout.Foldout(_toggleGroup[i], "Point: " + i + "  -  " + _path.pathPoints[i].beviourAtPoint.ToString(), true);
            if (GUILayout.Button("Remove point!", GUILayout.Width(Screen.width / 3)))
            {
                _toggleGroup.RemoveAt(i);
                _path.pathPoints.RemoveAt(i);
                _toggleGroup.TrimExcess();
                _path.pathPoints.TrimExcess();
                return;
            }
            GUILayout.EndHorizontal();

            if (_toggleGroup[i])
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Point Behavior: ");
                _path.pathPoints[i].beviourAtPoint = (PathPoint.PointBehavior)EditorGUILayout.EnumPopup(_path.pathPoints[i].beviourAtPoint);
                GUILayout.EndHorizontal();

                _path.pathPoints[i].location = EditorGUILayout.Vector3Field("", _path.pathPoints[i].location);
            }
        }
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        for (int i = 0; i < _path.pathPoints.Capacity; i++)
        { // Makes each point show in the scene view and you can edit them using the position handles.
            if(_colors[(int)_path.pathPoints[i].beviourAtPoint] != null)
                Handles.color = _colors[(int)_path.pathPoints[i].beviourAtPoint].color;

            if (_toggleGroup[i]) // makes it so that only if the point detail is visible in the inspector then it is adjustable.
                _path.pathPoints[i].location = Handles.PositionHandle(_path.pathPoints[i].location, Quaternion.identity);

            Handles.SphereHandleCap(1, _path.pathPoints[i].location, Quaternion.identity, .5f, EventType.Repaint); // shows where the points are.
        }

        Handles.color = Color.white; // sets the color to something default.

        for (int i = 0; i + 1 < _path.pathPoints.Capacity; i++)
        { // Shoes the connection (linearly) between points.
            Handles.DrawLine(_path.pathPoints[i].location, _path.pathPoints[i + 1].location);
        }

        Handles.color = Color.gray;
    }
}
