// using UnityEngine;
// using UnityEditor;

// [CustomEditor(typeof(KinematicMaster))]
// public class KinematicMasterEditor : Editor
// {

//     KinematicMaster t;
//     GUIStyle padding = new GUIStyle();
//     GUIStyle margin = new GUIStyle();

//     private void OnEnable()
//     {
//         t = target as KinematicMaster;

//         padding.padding = new RectOffset(5, 5, 5, 5);
//         margin.margin = new RectOffset(0, 0, 5, 5);
//     }
//     public override void OnInspectorGUI()
//     {
//         // base.OnInspectorGUI();
//         KinematicMaster t = target as KinematicMaster;
//         serializedObject.Update();

//         GUILayout.BeginHorizontal();
//         GUILayout.FlexibleSpace();
//         EditorGUILayout.LabelField("Links ", GUILayout.Width(35));
//         GUILayout.FlexibleSpace();
//         GUILayout.EndHorizontal();

//         var lnks = serializedObject.FindProperty("links");
//         for (int i = 0; i < lnks.arraySize; i++)
//         {
//             GUILayout.BeginVertical(margin);
//             GUILayout.BeginVertical(EditorStyles.helpBox);
//             GUILayout.BeginVertical(padding);

//             GUILayout.BeginHorizontal();
//             GUILayout.FlexibleSpace();
//             EditorGUILayout.LabelField("Link " + i, GUILayout.Width(40));
//             GUILayout.FlexibleSpace();
//             GUILayout.EndHorizontal();


//             EditorGUILayout.PropertyField(lnks.GetArrayElementAtIndex(i));

//             //Centred Button
//             GUILayout.BeginHorizontal();
//             GUILayout.FlexibleSpace();
//             if (GUILayout.Button("Remove", GUILayout.Width(100)))
//             {
//                 lnks.DeleteArrayElementAtIndex(i);
//             }
//             GUILayout.FlexibleSpace();
//             GUILayout.EndHorizontal();
//             //end Button

//             GUILayout.EndVertical();
//             GUILayout.EndVertical();
//             GUILayout.EndVertical();

//         }

//         //Centred Button
//         GUILayout.BeginHorizontal();
//         GUILayout.FlexibleSpace();
//         if (GUILayout.Button("New", GUILayout.Width(50)))
//         {
//             int idx = lnks.arraySize;
//             lnks.InsertArrayElementAtIndex(idx);
//             var lnk = lnks.GetArrayElementAtIndex(idx);
//             lnk.FindPropertyRelative("master").FindPropertyRelative("bone").objectReferenceValue = t.GetComponent<KinematicConstraints>();
//             lnk.FindPropertyRelative("master").FindPropertyRelative("transformation").enumValueIndex = 0;
//             lnk.FindPropertyRelative("master").FindPropertyRelative("axis").enumValueIndex = 0;
//             lnk.FindPropertyRelative("slaves").ClearArray();
//         }
//         GUILayout.FlexibleSpace();
//         GUILayout.EndHorizontal();
//         //end Button

//         PrefabUtility.RecordPrefabInstancePropertyModifications(t);

//         serializedObject.ApplyModifiedProperties();
//     }
// }