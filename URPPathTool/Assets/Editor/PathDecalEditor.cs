using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PathDecal))]
public class PathDecalEditor : Editor
{
    private int _activeIndex = -1;
    private bool _showPoints = false;
    public override void OnInspectorGUI()
    {   
        if(Application.isPlaying) return;
        PathDecal pathDecal = target as PathDecal;
        if(pathDecal.Positions == null) {
            pathDecal.Positions = new List<Vector3>();
            pathDecal.Positions.Add(Vector3.zero);
            pathDecal.Positions.Add(Vector3.forward);
        }
        var widthProperty = serializedObject.FindProperty("_width");
        var lastWidth = widthProperty.floatValue;
        EditorGUILayout.PropertyField(widthProperty);

        if(lastWidth != widthProperty.floatValue)
        {
            pathDecal.CreateMesh();
        }

        var heightProperty = serializedObject.FindProperty("_height");
        var lastHeight = heightProperty.floatValue;
        EditorGUILayout.PropertyField(heightProperty);
        if(lastHeight != heightProperty.floatValue)
        {
            pathDecal.CreateMesh();
        }
        var heightInfluenceProperty = serializedObject.FindProperty("_heightInfluence");
        var lastheightInfluence = heightInfluenceProperty.floatValue;
        EditorGUILayout.PropertyField(heightInfluenceProperty);

        if(lastheightInfluence != heightInfluenceProperty.floatValue)
        {
            pathDecal.UpdateShaderValues();
        }
        
        var radiusProperty = serializedObject.FindProperty("_radius");
        var lastRadius = radiusProperty.floatValue;
        EditorGUILayout.PropertyField(radiusProperty);
        if(lastRadius != radiusProperty.floatValue)
        {
            pathDecal.UpdateShaderValues();
        }
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Change path points: ");
        if(GUILayout.Button("+"))
        {
            var lastIndex = pathDecal.Positions.Count - 1;
            var lastDir = pathDecal.Positions[lastIndex] - pathDecal.Positions[lastIndex -1];
            
            pathDecal.Positions.Add(pathDecal.Positions[lastIndex] + lastDir);
            pathDecal.CreateMesh();
            pathDecal.UpdateShaderValues();
        }
        GUI.enabled = pathDecal.Positions.Count > 2;
        if(GUILayout.Button("-"))
        {
            pathDecal.Positions.RemoveAt(pathDecal.Positions.Count - 1);
            pathDecal.CreateMesh();
            pathDecal.UpdateShaderValues();
        }
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();
        if(GUILayout.Button("Create Mesh"))
        {
            pathDecal.CreateMesh();
        }
        _showPoints = EditorGUILayout.Foldout(_showPoints, "Points");
        if(_showPoints)
        {
            GUI.enabled = false;
            for (int i = 0; i < pathDecal.Positions.Count; i++)
            {
                EditorGUILayout.Vector3Field($"Point-{i}", pathDecal.Positions[i]);
            }
            
            GUI.enabled = true;
        }
        serializedObject.ApplyModifiedProperties();
    }

    void OnSceneGUI()
    {
        if(Application.isPlaying) return;
        PathDecal PathDecal = target as PathDecal;
        for (int i = 0; i < PathDecal.Positions.Count; i++)
        {
            if(Handles.Button(PathDecal.Positions[i],  Quaternion.identity, 0.25f, 0.5f, Handles.SphereHandleCap)){
                _activeIndex = i;
            }
        }

        if(_activeIndex >= 0 && _activeIndex < PathDecal.Positions.Count) {
            var lastPos = PathDecal.Positions[_activeIndex];
            PathDecal.Positions[_activeIndex] = Handles.PositionHandle(PathDecal.Positions[_activeIndex], Quaternion.identity);
            if(!lastPos.Equals(PathDecal.Positions[_activeIndex])){
                PathDecal.CreateMesh();
                PathDecal.UpdateShaderValues();
            }
        }
    }
}
