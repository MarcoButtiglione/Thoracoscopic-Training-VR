using UnityEngine;
using UnityEditor;
using KinematicMechanism.Utils;

[CustomPropertyDrawer(typeof(KinematicTransformation))]
public class KinematicTransformationDrawer : PropertyDrawer
{

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty bone = property.FindPropertyRelative("bone");
        SerializedProperty transformation = property.FindPropertyRelative("transformation");
        SerializedProperty axis = property.FindPropertyRelative("axis");

        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Don't make child fields be indented
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        // Calculate rects
        var boneRect = new Rect(position.x, position.y, position.xMax - (125 + position.x), position.height);
        var transRect = new Rect(position.xMax - 120, position.y, 85, position.height);
        var axisRect = new Rect(position.xMax - 30, position.y, 30, position.height);

        // Draw fields - passs GUIContent.none to each so they are drawn without labels
        EditorGUI.PropertyField(boneRect, bone, GUIContent.none);
        EditorGUI.PropertyField(transRect, transformation, GUIContent.none);
        EditorGUI.PropertyField(axisRect, axis, GUIContent.none);

        // Set indent back to what it was
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();

    }

}