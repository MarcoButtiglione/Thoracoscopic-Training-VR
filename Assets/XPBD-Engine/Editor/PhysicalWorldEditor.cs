using System;
using UnityEditor;
using UnityEngine;
using XPBD_Engine.Scripts.Physics;


[CustomEditor(typeof(PhysicalWorld))]
public class PhysicalWorldEditor : Editor
{
    private SerializedProperty _gravity;
    private SerializedProperty _worldBoundCenter;
    private SerializedProperty _worldBoundType;
    private SerializedProperty _worldBoundSize;
    private SerializedProperty _worldBoundRadius;
    
    private SerializedProperty _numSubsteps;
    private SerializedProperty _paused;
    private void OnEnable()
    {
        // Ottieni riferimenti ai campi serializzati
        _gravity = serializedObject.FindProperty("gravity");
        _worldBoundCenter = serializedObject.FindProperty("worldBoundCenter");
        _worldBoundType = serializedObject.FindProperty("worldBoundType");
        _worldBoundSize = serializedObject.FindProperty("worldBoundSize");
        _worldBoundRadius = serializedObject.FindProperty("worldBoundRadius");
        _numSubsteps = serializedObject.FindProperty("numSubsteps");
        _paused = serializedObject.FindProperty("paused");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        var physicalWorld = (PhysicalWorld)target;
        EditorGUILayout.PropertyField(_gravity);
        EditorGUILayout.PropertyField(_numSubsteps);
        EditorGUILayout.PropertyField(_paused);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("World Bound", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_worldBoundType,new GUIContent("Type"));
        if (physicalWorld.worldBoundType != WorldBoundType.None)
        {
            EditorGUILayout.PropertyField(_worldBoundCenter,new GUIContent("Position"));
            switch (physicalWorld.worldBoundType)
            {
                case WorldBoundType.Cube:
                    EditorGUILayout.PropertyField(_worldBoundSize,new GUIContent("Size"));
                    break;
                case WorldBoundType.Sphere:
                    EditorGUILayout.PropertyField(_worldBoundRadius,new GUIContent("Radius"));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        EditorGUILayout.Space();
        GUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Pause Physics"))
        {
            physicalWorld.SwitchPaused();
        }
        
        GUILayout.EndHorizontal();
        
        serializedObject.ApplyModifiedProperties();
    }
}
