// using System.Collections.Generic;
// using UnityEngine;
// using UnityEditor;

// [CustomEditor(typeof(KinematicMechanism))]
// public class KinematicMechanismEditor : Editor
// {

//     KinematicMechanism t;
//     GUIStyle padding = new GUIStyle();
//     GUIStyle margin = new GUIStyle();

//     bool linkFold = false;
//     bool basicFold = false;
//     SerializedObject serObj;

//     private void OnEnable()
//     {
//         t = target as KinematicMechanism;

//         padding.padding = new RectOffset(5, 5, 5, 5);
//         margin.margin = new RectOffset(0, 0, 5, 5);
//     }
//     public override void OnInspectorGUI()
//     {
//         // base.OnInspectorGUI();
//         serializedObject.Update();

//         DrawLinks();

//         serializedObject.ApplyModifiedProperties();

//         EditorGUILayout.Space();
//         basicFold = EditorGUILayout.Foldout(basicFold, "BASE INSPECTOR. BECAUSE OF WIP.");
//         if (basicFold)
//             base.OnInspectorGUI();
//     }

//     private void DrawLinks()
//     {
//         linkFold = EditorGUILayout.Foldout(linkFold, "Associations");
//         if (linkFold)
//         {
//             var associations = serializedObject.FindProperty("associations");

//             for (int i = 0; i < associations.arraySize; i++)
//             {
//                 var lnk = associations.GetArrayElementAtIndex(i).FindPropertyRelative("link");
//                 Debug.Log(lnk.type);
//                 // Debug.Log(lnk.type);
//                 GUILayout.BeginVertical(margin);
//                 GUILayout.BeginVertical(EditorStyles.helpBox);
//                 GUILayout.BeginVertical(padding);

//                 EditorGUILayout.PropertyField(associations.GetArrayElementAtIndex(i).FindPropertyRelative("control"));
//                 EditorGUILayout.Space();
//                 EditorGUILayout.PropertyField(lnk);

//                 //Centred Button
//                 GUILayout.BeginHorizontal();
//                 GUILayout.FlexibleSpace();
//                 if (GUILayout.Button("Remove", GUILayout.Width(100)))
//                 {
//                     associations.DeleteArrayElementAtIndex(i);
//                 }
//                 GUILayout.FlexibleSpace();
//                 GUILayout.EndHorizontal();
//                 //end Button


//                 GUILayout.EndVertical();
//                 GUILayout.EndVertical();
//                 GUILayout.EndVertical();

//             }

//             //Centred Button
//             GUILayout.BeginHorizontal();
//             GUILayout.FlexibleSpace();
//             if (GUILayout.Button("New", GUILayout.Width(50)))
//             {
//                 int idx = associations.arraySize;
//                 associations.InsertArrayElementAtIndex(idx);
//                 var control = associations.GetArrayElementAtIndex(idx).FindPropertyRelative("control");
//                 var lnk = associations.GetArrayElementAtIndex(idx).FindPropertyRelative("link");
//                 control.enumValueIndex = 0;
//                 lnk.FindPropertyRelative("master").FindPropertyRelative("bone").objectReferenceValue = null;
//                 lnk.FindPropertyRelative("master").FindPropertyRelative("transformation").enumValueIndex = 0;
//                 lnk.FindPropertyRelative("master").FindPropertyRelative("axis").enumValueIndex = 0;
//                 lnk.FindPropertyRelative("slaves").ClearArray();
//             }
//             GUILayout.FlexibleSpace();
//             GUILayout.EndHorizontal();
//             //end Button

//         }
//     }
// }

