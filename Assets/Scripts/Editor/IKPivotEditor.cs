using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

[CustomEditor(typeof(IKPivot))]
public class IKPivotEditor : Editor
{

    IKPivot t;
    ArcHandle upperArc = new ArcHandle();
    ArcHandle lowerArc = new ArcHandle();

    private void OnEnable()
    {
        t = target as IKPivot;

    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }

    private void OnSceneGUI()
    {
        serializedObject.Update();

        if (!serializedObject.FindProperty("_singleSide").boolValue)
            DrawCone(.1f, -t.transform.forward);
        DrawCone(.1f, t.transform.forward);

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawCone(float radius, Vector3 forward)
    {
        Color orange = new Color(1, .5f, 0);
        Color lightOrange = new Color(1, .5f, 0, .1f);

        Handles.color = orange;

        float angle = serializedObject.FindProperty("_angleLimit").floatValue;
        upperArc.angle = angle / 2;
        lowerArc.angle = -angle / 2;
        upperArc.radius = radius;
        lowerArc.radius = radius;

        var mat1 = Matrix4x4.TRS(t.transform.position, Quaternion.LookRotation(forward, t.transform.up), t.transform.lossyScale);
        using (new Handles.DrawingScope(mat1))
        {
            EditorGUI.BeginChangeCheck();
            upperArc.DrawHandle();
            lowerArc.DrawHandle();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Changed Angle Limit");

                float newAngleUpper = (Mathf.Round(Mathf.Clamp(upperArc.angle, 0, 90) * 10f) / 10f) * 2;
                float newAngleLower = -(Mathf.Round(Mathf.Clamp(lowerArc.angle, -90, 0) * 10f) / 10f) * 2;
                if (newAngleUpper != angle)
                    serializedObject.FindProperty("_angleLimit").floatValue = newAngleUpper;
                else if (newAngleLower != angle)
                    serializedObject.FindProperty("_angleLimit").floatValue = newAngleLower;
            }
        }

        var mat2 = Matrix4x4.TRS(t.transform.position, Quaternion.LookRotation(forward, t.transform.right), t.transform.lossyScale);
        using (new Handles.DrawingScope(mat2))
        {
            EditorGUI.BeginChangeCheck();
            upperArc.DrawHandle();
            lowerArc.DrawHandle();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Changed Angle Limit");

                float newAngleUpper = (Mathf.Round(Mathf.Clamp(upperArc.angle, 0, 90) * 10f) / 10f) * 2;
                float newAngleLower = -(Mathf.Round(Mathf.Clamp(lowerArc.angle, -90, 0) * 10f) / 10f) * 2;
                if (newAngleUpper != angle)
                    serializedObject.FindProperty("_angleLimit").floatValue = newAngleUpper;
                else if (newAngleLower != angle)
                    serializedObject.FindProperty("_angleLimit").floatValue = newAngleLower;
            }
        }

        Vector3 edgeDir = (Quaternion.AngleAxis(angle / 2, t.transform.right) * forward).normalized;
        Vector3 center = t.transform.TransformPoint(Vector3.Project(t.transform.InverseTransformDirection(edgeDir * radius), Vector3.forward));
        float halfSize = Mathf.Abs((center - (t.transform.position + edgeDir * radius)).magnitude);

        if (serializedObject.FindProperty("_coneConstraints").boolValue)
        {
            Handles.DrawWireDisc(center, forward, halfSize);
            Handles.color = lightOrange;
            Handles.DrawSolidDisc(center, forward, halfSize);
        }
        else
        {
            using (new Handles.DrawingScope(mat1))
            {
                Handles.DrawWireCube(t.transform.InverseTransformPoint(center), new Vector3(halfSize * 2, halfSize * 2, 0));
            }
        }
    }

}
