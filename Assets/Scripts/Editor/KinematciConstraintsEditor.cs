using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using KinematicMechanism;
using UnityEditor.UIElements;

[CustomEditor(typeof(KinematicConstraints))]
public class KinematicConstraintsEditor : Editor
{

    private KinematicConstraints t;

    Vector3 box_center;
    BoxBoundsHandle trans_constraints = new BoxBoundsHandle();

    ArcHandle xl_arc = new ArcHandle();
    ArcHandle xu_arc = new ArcHandle();

    ArcHandle yl_arc = new ArcHandle();
    ArcHandle yu_arc = new ArcHandle();

    ArcHandle zl_arc = new ArcHandle();
    ArcHandle zu_arc = new ArcHandle();


    bool isPlaying;
    static int axisTabsInt = 0;
    string[] axisTabs = { "x", "y", "z" };

    static int dofTabsInt = 0;
    string[] dofTabs = { "Translation", "Rotation" };

    private SerializedProperty _initPos;
    private SerializedProperty _forward;
    private SerializedProperty _up;
    private SerializedProperty _right;

    private void OnEnable()
    {
        xl_arc.SetColorWithRadiusHandle(Color.red, .1f);
        xu_arc.SetColorWithRadiusHandle(Color.red, .1f);

        yl_arc.SetColorWithRadiusHandle(Color.green, .1f);
        yu_arc.SetColorWithRadiusHandle(Color.green, .1f);

        zl_arc.SetColorWithRadiusHandle(Color.blue, .1f);
        zu_arc.SetColorWithRadiusHandle(Color.blue, .1f);

        t = target as KinematicConstraints;
        _initPos = serializedObject.FindProperty("_initPos");
        _forward = serializedObject.FindProperty("_forward");
        _up = serializedObject.FindProperty("_up");
        _right = serializedObject.FindProperty("_right");


    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        dofTabsInt = GUILayout.Toolbar(dofTabsInt, dofTabs);
        axisTabsInt = GUILayout.Toolbar(axisTabsInt, axisTabs);

        if (dofTabsInt == 0)
        {
            switch (axisTabsInt)
            {
                case 0:
                    ShowTranslationProperties(ref t.xTranslationLocked, ref t.xTranslationClamped, ref t.xTranslationInverted, ref t.xTranslationLower, ref t.xTranslationUpper);
                    break;
                case 1:
                    ShowTranslationProperties(ref t.yTranslationLocked, ref t.ytranslationClamped, ref t.yTranslationInverted, ref t.yTranslationLower, ref t.yTranslationUpper);
                    break;
                case 2:
                    ShowTranslationProperties(ref t.zTranslationLocked, ref t.zTranslationClamped, ref t.zTranslationInverted, ref t.zTranslationLower, ref t.zTranslationUpper);
                    break;
            }
        }
        else
        {
            switch (axisTabsInt)
            {
                case 0:
                    ShowRotationsProperties(ref t.xRotationLocked, ref t.xRotationClamped, ref t.xRotationInverted, ref t.xRotationLower, ref t.xRotationUpper);
                    break;
                case 1:
                    ShowRotationsProperties(ref t.yRotationLocked, ref t.yRotationClamped, ref t.yRotationInverted, ref t.yRotationLower, ref t.yRotationUpper);
                    break;
                case 2:
                    ShowRotationsProperties(ref t.zRotationLocked, ref t.zRotationClamped, ref t.zRotationInverted, ref t.zRotationLower, ref t.zRotationUpper);
                    break;
            }
        }

        PrefabUtility.RecordPrefabInstancePropertyModifications(t);

    }



    private void OnSceneGUI()
    {

        serializedObject.Update();
        isPlaying = Application.isPlaying;


        Vector3 pos = isPlaying ? _initPos.vector3Value : t.transform.position;
        Vector3 right = isPlaying ? _right.vector3Value : t.transform.right;
        Vector3 forward = isPlaying ? _forward.vector3Value : t.transform.forward;
        Vector3 up = isPlaying ? _up.vector3Value : t.transform.up;

        DrawFacingSphere(t.transform, .01f);
        DrawTranslationsDOF(pos, forward, up, right, 5f, .1f);
        DrawRotationDOF(forward, up, right, 1f);

    }

    //Build the inspector for a specific translation asix
    private void ShowTranslationProperties(ref bool locked, ref bool clamped, ref bool inverted, ref float lower, ref float upper)
    {
        bool previous = GUI.enabled;
        locked = EditorGUILayout.Toggle("Locked", locked);
        GUI.enabled = !locked;
        EditorGUILayout.BeginHorizontal();
        clamped = EditorGUILayout.Toggle("Clamped", clamped);
        GUI.enabled = !locked && clamped;
        inverted = EditorGUILayout.Toggle("Inverted", inverted);
        EditorGUILayout.EndHorizontal();
        if (!inverted)
        {
            lower = Mathf.Clamp(EditorGUILayout.FloatField("From", lower), -Mathf.Infinity, 0);
            upper = Mathf.Clamp(EditorGUILayout.FloatField("To", upper), 0, Mathf.Infinity);
        }
        else
        {
            upper = Mathf.Clamp(EditorGUILayout.FloatField("From", upper), 0, Mathf.Infinity);
            lower = Mathf.Clamp(EditorGUILayout.FloatField("To", lower), -Mathf.Infinity, 0);

        }

        GUI.enabled = previous;
    }

    //Build the inspector for a specific rotation axis
    private void ShowRotationsProperties(ref bool locked, ref bool clamped, ref bool inverted, ref float lower, ref float upper)
    {
        bool previous = GUI.enabled;
        locked = EditorGUILayout.Toggle("Locked", locked);
        GUI.enabled = !locked;
        EditorGUILayout.BeginHorizontal();
        clamped = EditorGUILayout.Toggle("Clamped", clamped);
        GUI.enabled = !locked && clamped;
        inverted = EditorGUILayout.Toggle("Inverted", inverted);
        EditorGUILayout.EndHorizontal();
        if (!inverted)
        {
            lower = EditorGUILayout.Slider("From", lower, 0, -180);
            upper = EditorGUILayout.Slider("To", upper, 0, 180);
        }
        else
        {
            upper = EditorGUILayout.Slider("From", upper, 0, 180);
            lower = EditorGUILayout.Slider("To", lower, 0, -180);

        }

        GUI.enabled = previous;
    }


    //Draw the constraints of the translations in the GUI
    private void DrawTranslationsDOF(Vector3 position, Vector3 forward, Vector3 up, Vector3 right, float unclampedlenght, float handlesSize)
    {
        Color prev = Handles.color; //save previous color

        trans_constraints.center = position; //center the box in the position
        Vector3 sizes = Vector3.zero; //size of the box
        Vector3 offset = Vector3.zero; //offset of the box center wrt the object position
        Color final = new Color(0, 0, 0); //the boc color
        trans_constraints.axes = PrimitiveBoundsHandle.Axes.None; //allowed axis dragging

        //if not locked
        if (!t.xTranslationLocked)
        {
            //Set axis color
            Handles.color = Color.red;
            if (t.xTranslationClamped) //if clamped
            {
                trans_constraints.axes = trans_constraints.axes | PrimitiveBoundsHandle.Axes.X; //allow dragging
                final.r = 255; //update box color
                sizes.x = t.xTranslationUpper - t.xTranslationLower; //update box size
                offset.x = (sizes.x / 2) - Mathf.Abs(t.xTranslationLower); //update box offset

                //Update the low and upper constraints with the distance of a draggable arrow-slider
                EditorGUI.BeginChangeCheck();
                var x_upper = Vector3.Distance(position, Handles.Slider(position + right * (t.xTranslationUpper + handlesSize), right, handlesSize, Handles.ConeHandleCap, .01f)) - handlesSize;
                var x_low = -Vector3.Distance(position, Handles.Slider(position + right * (t.xTranslationLower - handlesSize), -right, handlesSize, Handles.ConeHandleCap, .01f)) + handlesSize;
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Degrees of freedom. Translation X.");

                    //change values (clamp and round to 2 decimal)
                    t.xTranslationUpper = Mathf.Round(Mathf.Clamp(x_upper, 0, Mathf.Infinity) * 100f) / 100f;
                    t.xTranslationLower = Mathf.Round(Mathf.Clamp(x_low, -Mathf.Infinity, 0) * 100f) / 100f;
                }
            }
            else//if not clamped use the dotted-line to represent infinite movement
            {
                Handles.DrawDottedLine(t.transform.position - right, t.transform.position + right, .5f);
            }

        }

        if (!t.yTranslationLocked)
        {
            Handles.color = Color.green;
            if (t.ytranslationClamped)
            {
                trans_constraints.axes = trans_constraints.axes | PrimitiveBoundsHandle.Axes.Y;
                final.g = 255;
                sizes.y = t.yTranslationUpper - t.yTranslationLower;
                offset.y = (sizes.y / 2) - Mathf.Abs(t.yTranslationLower);

                EditorGUI.BeginChangeCheck();
                var y_upper = Vector3.Distance(position, Handles.Slider(position + up * (t.yTranslationUpper + handlesSize), up, handlesSize, Handles.ConeHandleCap, .01f)) - handlesSize;
                var y_low = -Vector3.Distance(position, Handles.Slider(position + up * (t.yTranslationLower - handlesSize), -up, handlesSize, Handles.ConeHandleCap, .01f)) + handlesSize;
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Degrees of freedom. Translation Y.");

                    //change values (clamp and round to 2 decimal)
                    t.yTranslationUpper = Mathf.Round(Mathf.Clamp(y_upper, 0, Mathf.Infinity) * 100f) / 100f;
                    t.yTranslationLower = Mathf.Round(Mathf.Clamp(y_low, -Mathf.Infinity, 0) * 100f) / 100f; ;
                }
            }
            else
            {
                Handles.DrawDottedLine(t.transform.position - up, t.transform.position + up, .5f);
            }

        }

        if (!t.zTranslationLocked)
        {
            Handles.color = Color.blue;
            if (t.zTranslationClamped)
            {
                trans_constraints.axes = trans_constraints.axes | PrimitiveBoundsHandle.Axes.Z;
                final.b = 255;
                sizes.z = t.zTranslationUpper - t.zTranslationLower;
                offset.z = (sizes.z / 2) - Mathf.Abs(t.zTranslationLower);

                EditorGUI.BeginChangeCheck();
                var z_upper = Vector3.Distance(position, Handles.Slider(position + forward * (t.zTranslationUpper + handlesSize), forward, handlesSize, Handles.ConeHandleCap, .01f)) - handlesSize;
                var z_low = -Vector3.Distance(position, Handles.Slider(position + forward * (t.zTranslationLower - handlesSize), -forward, handlesSize, Handles.ConeHandleCap, .01f)) + handlesSize;
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Degrees of freedom. Translation Z.");

                    //change values (clamp and round to 2 decimal)
                    t.zTranslationUpper = Mathf.Round(Mathf.Clamp(z_upper, 0, Mathf.Infinity) * 100f) / 100f;
                    t.zTranslationLower = Mathf.Round(Mathf.Clamp(z_low, -Mathf.Infinity, 0) * 100f) / 100f;
                }
            }
            else
            {
                Handles.DrawDottedLine(t.transform.position - forward, t.transform.position + forward, .5f);
            }

        }

        Handles.color = prev; //reset the color
        trans_constraints.center = offset; //set the center to offset
        trans_constraints.size = sizes; //set the size
        trans_constraints.SetColor(final); // set box color

        //define a spece centred in target position and oriented as the target
        var mat = Matrix4x4.TRS(position, Quaternion.LookRotation(forward, up), Vector3.one);
        using (new Handles.DrawingScope(mat))
        {
            trans_constraints.DrawHandle(); //draw the box
        }
    }

    private void DrawRotationDOF(Vector3 forward, Vector3 up, Vector3 right, float radius)
    {
        if (!t.xRotationLocked)
        {
            if (!t.xRotationClamped)
            {
                DrawFullAngle(t.transform.position, right, radius, Color.red);
            }
            else
            {
                DrawAngle(t.transform.position, forward, -right, xl_arc, ref t.xRotationLower, xu_arc, ref t.xRotationUpper, radius);
            }
        }

        if (!t.yRotationLocked)
        {

            if (!t.yRotationClamped)
            {
                DrawFullAngle(t.transform.position, up, radius, Color.green);
            }
            else
            {
                DrawAngle(t.transform.position, forward, up, yl_arc, ref t.yRotationLower, yu_arc, ref t.yRotationUpper, radius);
            }
        }

        if (!t.zRotationLocked)
        {

            if (!t.zRotationClamped)
            {
                DrawFullAngle(t.transform.position, forward, radius, Color.blue);
            }
            else
            {
                DrawAngle(t.transform.position, up, -forward, zl_arc, ref t.zRotationLower, zu_arc, ref t.zRotationUpper, radius);
            }
        }
    }

    private void DrawAngle(Vector3 center, Vector3 direction, Vector3 normal, ArcHandle lower_arc, ref float lower_angle, ArcHandle upper_arc, ref float upper_angle, float radius)
    {
        lower_arc.angle = lower_angle;
        upper_arc.angle = upper_angle;
        lower_arc.radius = radius;
        upper_arc.radius = radius;
        var mat = Matrix4x4.TRS(center, Quaternion.LookRotation(direction, normal), Vector3.one);
        using (new Handles.DrawingScope(mat))
        {
            EditorGUI.BeginChangeCheck();
            lower_arc.DrawHandle();
            upper_arc.DrawHandle();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Changed degrees of fredom b");

                //change values (clamp and round to 1 decimal)
                lower_angle = Mathf.Round(Mathf.Clamp(lower_arc.angle, -180, 0) * 10f) / 10f;
                upper_angle = Mathf.Round(Mathf.Clamp(upper_arc.angle, 0, 180) * 10f) / 10f;
            }
        }

    }

    private void DrawFullAngle(Vector3 center, Vector3 normal, float radius, Color color)
    {
        Color pre = Handles.color;
        Color fill = color;
        fill.a = .1f;
        Handles.color = color;
        Handles.DrawWireDisc(center, normal, radius);
        Handles.color = fill;
        Handles.DrawSolidDisc(center, normal, radius);
    }

    private void DrawFacingSphere(Transform t, float radius)
    {
        var initMatrix = Handles.matrix;
        Handles.matrix = t.localToWorldMatrix;
        Vector3 position = Vector3.zero;

        Handles.DrawWireDisc(position, Vector3.right, radius);
        Handles.DrawWireDisc(position, Vector3.up, radius);
        Handles.DrawWireDisc(position, Vector3.forward, radius);

        if (Camera.current.orthographic)
        {
            Vector3 normal = position - Handles.inverseMatrix.MultiplyVector(Camera.current.transform.forward);
            float sqrMagnitude = normal.sqrMagnitude;
            float num0 = radius * radius;
            Handles.DrawWireDisc(position - num0 * normal / sqrMagnitude, normal, radius);
        }
        else
        {
            Vector3 normal = position - Handles.inverseMatrix.MultiplyPoint(Camera.current.transform.position);
            float sqrMagnitude = normal.sqrMagnitude;
            float num0 = radius * radius;
            float num1 = num0 * num0 / sqrMagnitude;
            float num2 = Mathf.Sqrt(num0 - num1);
            Handles.DrawWireDisc(position - num0 * normal / sqrMagnitude, normal, num2);
        }

        Handles.matrix = initMatrix;
    }

}