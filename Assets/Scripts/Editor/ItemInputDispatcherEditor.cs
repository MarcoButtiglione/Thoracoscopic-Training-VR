// using UnityEngine;
// using UnityEditor;
// using System;

// [CustomEditor(typeof(ItemInputDispatcher))]
// public class ItemInputDispatcherEditor : Editor
// {

//     ItemInputDispatcher t;
//     GUIStyle padding = new GUIStyle();
//     GUIStyle margin = new GUIStyle();

//     bool defaultEditor = false;
//     bool defaultEditorFoldout = false;

//     private void OnEnable()
//     {
//         t = target as ItemInputDispatcher;
//         padding.padding = new RectOffset(5, 5, 5, 5);
//         margin.margin = new RectOffset(0, 0, 5, 5);
//     }

//     public override void OnInspectorGUI()
//     {
//         defaultEditor = false;

//         GUILayout.BeginHorizontal();
//         GUILayout.FlexibleSpace();
//         EditorGUILayout.LabelField("Control Maps", GUILayout.Width(80));
//         GUILayout.FlexibleSpace();
//         GUILayout.EndHorizontal();

//         for (int i = 0; i < t.associations.Length; i++)
//         {
//             EditorGUILayout.BeginVertical(margin);
//             EditorGUILayout.BeginVertical(EditorStyles.helpBox);
//             EditorGUILayout.BeginVertical(padding);

//             GUILayout.BeginHorizontal();
//             GUILayout.FlexibleSpace();
//             EditorGUILayout.LabelField("Control Map " + i, GUILayout.Width(90));
//             GUILayout.FlexibleSpace();
//             GUILayout.EndHorizontal();

//             EditorGUILayout.Space();

//             t.associations[i].control = (Control)EditorGUILayout.EnumPopup("Control", t.associations[i].control);
//             t.associations[i].action = (BaseAction)EditorGUILayout.ObjectField("Action", t.associations[i].action, typeof(BaseAction), true);
//             EditorGUILayout.Space();

//             if (t.associations[i].action != null)
//             {
//                 if (t.associations[i].action.GetType() == typeof(KinematicMaster))
//                 {
//                     DrawKinematicMaster((KinematicMaster)t.associations[i].action, ref t.associations[i].subAction);

//                 }
//                 else //default
//                 {
//                     GUIStyle box = new GUIStyle();
//                     box.normal.background = MakeTex(2, 2, new Color(1f, 1f, 0f, 0.1f));

//                     EditorGUILayout.Space();
//                     EditorGUILayout.BeginVertical(box);
//                     EditorGUILayout.LabelField("Action not handled: add it in ItemInputDispatcherEditor.");
//                     EditorGUILayout.LabelField("Use default editor below.");
//                     EditorGUILayout.EndVertical();

//                     defaultEditor = true;
//                 }
//             }

//             EditorGUILayout.Space();

//             //Centred Button
//             GUILayout.BeginHorizontal();
//             GUILayout.FlexibleSpace();
//             if (GUILayout.Button("Remove", GUILayout.Width(100)))
//             {
//                 ArrayUtility.RemoveAt(ref t.associations, i);
//             }
//             GUILayout.FlexibleSpace();
//             GUILayout.EndHorizontal();
//             //end Button

//             EditorGUILayout.EndVertical();
//             EditorGUILayout.EndVertical();
//             EditorGUILayout.EndVertical();
//         }

//         //Centred Button
//         GUILayout.BeginHorizontal();
//         GUILayout.FlexibleSpace();
//         if (GUILayout.Button("New", GUILayout.Width(50)))
//         {
//             int idx = t.associations.Length;
//             ArrayUtility.Insert(ref t.associations, idx, new ControlAssociation());
//         }
//         GUILayout.FlexibleSpace();
//         GUILayout.EndHorizontal();
//         //end Button

//         if (defaultEditor)
//         {
//             defaultEditorFoldout = EditorGUILayout.Foldout(defaultEditorFoldout, "Default Editor (not handled BaseAction). Use it carefully.");
//             if (defaultEditorFoldout)
//                 base.OnInspectorGUI();
//         }

//         PrefabUtility.RecordPrefabInstancePropertyModifications(t);
//     }

//     private void DrawKinematicMaster(KinematicMaster master, ref int subAction)
//     {
//         var length = master.links.Length;

//         if (length > 0)
//         {
//             int[] ar = new int[length];
//             string[] opts = new string[length];
//             MakeIntSelectors(ref opts, ref ar, 0, length - 1, "Link");
//             if (subAction >= length)
//             {
//                 subAction = 0;
//             }
//             subAction = EditorGUILayout.IntPopup("Kinematic Link", subAction, opts, ar);
//         }
//         else
//         {
//             EditorGUI.BeginDisabledGroup(true);
//             int[] ar = { -1 };
//             string[] opts = { "No Sub-Action exists." };
//             EditorGUILayout.IntPopup(-1, opts, ar);
//             EditorGUI.EndDisabledGroup();
//         }
//     }

//     private void MakeIntSelectors(ref string[] display, ref int[] options, int from, int to, string displaySuffix)
//     {
//         for (int i = from; i <= to; i++)
//         {
//             display[i] = displaySuffix + " " + i.ToString();
//             options[i] = i;
//         }
//     }

//     private Texture2D MakeTex(int width, int height, Color col)
//     {
//         Color[] pix = new Color[width * height];
//         for (int i = 0; i < pix.Length; ++i)
//         {
//             pix[i] = col;
//         }
//         Texture2D result = new Texture2D(width, height);
//         result.SetPixels(pix);
//         result.Apply();
//         return result;
//     }
// }