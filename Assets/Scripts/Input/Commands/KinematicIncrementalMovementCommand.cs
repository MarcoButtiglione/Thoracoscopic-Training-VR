using UnityEngine;
using KinematicMechanism;
using KinematicMechanism.Utils;

public class KinematicIncrementalMovementCommand : BaseCommand
{
    [SerializeField] private float deadzone = 0f;
    public KinematicTransformation leader;
    public KinematicTransformation[] followers;
    public float incrementSpeed;

    public override void Activate(float value)
    {
        if (Mathf.Abs(value) > deadzone)
        {
            IncrementValue(leader.bone, value, leader.transformation, leader.axis);

            foreach (var follower in followers)
            {
                IncrementValue(follower.bone, value, follower.transformation, follower.axis);
            }
        }
    }

    private void IncrementValue(KinematicConstraints target, float value, Transformation transformation, Axis axis)
    {
        var correctedValue = value * Time.deltaTime * incrementSpeed;

        switch (transformation)
        {
            case Transformation.Translation:
                switch (axis)
                {
                    case Axis.X:
                        target.TranslateRelative(correctedValue, 0, 0);
                        break;
                    case Axis.Y:
                        target.TranslateRelative(0, correctedValue, 0);
                        break;
                    case Axis.Z:
                        target.TranslateRelative(0, 0, correctedValue);
                        break;
                }
                break;

            case Transformation.Rotation:
                switch (axis)
                {
                    case Axis.X:
                        target.RotateRelative(correctedValue, 0, 0);
                        break;
                    case Axis.Y:
                        target.RotateRelative(0, correctedValue, 0);
                        break;
                    case Axis.Z:
                        target.RotateRelative(0, 0, correctedValue);
                        break;
                }
                break;
        }
    }
}
