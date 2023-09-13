using KinematicMechanism;
using KinematicMechanism.Utils;

public class KinematicRelativeMovementCommand : BaseCommand
{
    public KinematicTransformation leader;
    public KinematicTransformation[] followers;

    public override void Activate(float value)
    {
        SetValue(leader.bone, value, leader.transformation, leader.axis);

        foreach (var follower in followers)
        {
            SetValue(follower.bone, value, follower.transformation, follower.axis);
        }
    }

    private void SetValue(KinematicConstraints target, float value, Transformation transformation, Axis axis)
    {
        switch (transformation)
        {
            case Transformation.Translation:
                switch (axis)
                {
                    case Axis.X:
                        target.SetRelativeTranslationX(value);
                        break;
                    case Axis.Y:
                        target.SetRelativeTranslationY(value);
                        break;
                    case Axis.Z:
                        target.SetRelativeTranslationZ(value);
                        break;
                }
                break;

            case Transformation.Rotation:
                switch (axis)
                {
                    case Axis.X:
                        target.SetRelativeRotationX(value);
                        break;
                    case Axis.Y:
                        target.SetRelativeRotationY(value);
                        break;
                    case Axis.Z:
                        target.SetRelativeRotationZ(value);
                        break;
                }
                break;
        }
    }
}
