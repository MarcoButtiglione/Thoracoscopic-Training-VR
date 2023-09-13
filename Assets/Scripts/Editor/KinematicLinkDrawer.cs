// using UnityEngine;
// using UnityEditor;

// [CustomPropertyDrawer(typeof(KinematicLink))]
// public class KinematicLinkDrawer : PropertyDrawer
// {
//     private int rows = 4;
//     private float rowOffset = 4;
//     private float rowHeight;
//     private float propHeight;


//     public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//     {

//         SerializedProperty master = property.FindPropertyRelative("master");
//         SerializedProperty slaves = property.FindPropertyRelative("slaves");

//         EditorGUI.BeginProperty(position, label, property);

//         float y_init = position.y;
//         float x_init = position.x;
//         float width = position.width;

//         EditorGUI.LabelField(new Rect(position.x + width / 2 - 22, y_init, 44, propHeight), "Master");
//         EditorGUI.BeginDisabledGroup(true);
//         EditorGUI.PropertyField(new Rect(x_init + 25, y_init + rowHeight, position.xMax - (125 + position.x) - 25, propHeight), master.FindPropertyRelative("bone"), GUIContent.none);
//         EditorGUI.EndDisabledGroup();
//         EditorGUI.PropertyField(new Rect(position.xMax - 120, y_init + rowHeight, 85, propHeight), master.FindPropertyRelative("transformation"), GUIContent.none);
//         EditorGUI.PropertyField(new Rect(position.xMax - 30, y_init + rowHeight, 30, propHeight), master.FindPropertyRelative("axis"), GUIContent.none);

//         if (GUI.Button(new Rect(x_init, y_init + rowHeight, 20, propHeight), "x"))
//         {
//             // master.FindPropertyRelative("bone").objectReferenceValue = null; //master must stay to target
//             master.FindPropertyRelative("transformation").enumValueIndex = 0;
//             master.FindPropertyRelative("axis").enumValueIndex = 0;
//         }
//         EditorGUI.LabelField(new Rect(position.x + width / 2 - 20, y_init + 2 * rowHeight, 40, propHeight), "Slaves");

//         for (int i = 0; i < slaves.arraySize; i++)
//         {
//             EditorGUI.PropertyField(new Rect(x_init + 25, y_init + (3 + i) * rowHeight, width - 25, propHeight), slaves.GetArrayElementAtIndex(i), GUIContent.none);
//             if (GUI.Button(new Rect(x_init, y_init + (3 + i) * rowHeight, 20, propHeight), "-"))
//             {
//                 slaves.DeleteArrayElementAtIndex(i);
//             }
//         }
//         if (GUI.Button(new Rect(x_init, y_init + (3 + slaves.arraySize) * rowHeight, 20, propHeight), "+"))
//         {
//             var idx = slaves.arraySize;
//             slaves.InsertArrayElementAtIndex(idx);
//             slaves.GetArrayElementAtIndex(idx).FindPropertyRelative("bone").objectReferenceValue = null;
//             slaves.GetArrayElementAtIndex(idx).FindPropertyRelative("transformation").enumValueIndex = 0;
//             slaves.GetArrayElementAtIndex(idx).FindPropertyRelative("axis").enumValueIndex = 0;

//         }
//         EditorGUI.EndProperty();

//     }

//     public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
//     {
//         rows = 4 + property.FindPropertyRelative("slaves").arraySize;
//         propHeight = base.GetPropertyHeight(property, label);
//         rowHeight = propHeight + rowOffset;
//         var totalHeight = (rowHeight) * rows;
//         return totalHeight;
//     }
// }