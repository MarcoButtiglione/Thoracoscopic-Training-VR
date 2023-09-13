using UnityEngine;
using MinigameSystem;

public class KinematicGraspCommand : BaseCommand
{
    private Graspable _grasperTarget = null;

    [Range(0.1f, 1.0f)]
    [Tooltip("Value above which the input should be in order to enable the grasping.")]
    public float sensitivity = 0.1f;
    public Transform graspZoneTransform;
    public KinematicGraspZone graspZone;

    private void Awake()
    {
        if (graspZoneTransform == null)
        {
            throw new MissingReferenceException("graspPoint must be assigned.");
        }
    }

    public override void Activate(float value)
    {
        if (value > sensitivity)
        {
            HandleGrasp();
        }
        else
        {
            ReleaseGraspable();
        }
    }

    private void HandleGrasp()
    {
        graspZone.EnableGrasp();
        if (graspZone.touchedGraspable != null)
        {
            Grasp(graspZone.touchedGraspable);
        }
    }

    private void Grasp(Graspable graspable)
    {
        if (_grasperTarget == null)
        {
            _grasperTarget = graspable;
            graspable.GetGrasped(graspZone, graspZoneTransform);
        }
    }

    private void ReleaseGraspable()
    {
        if (_grasperTarget != null)
        {
            if (graspZone.touchedGraspable.graspedBy == graspZone)
            {
                _grasperTarget.GetReleased();
            }
            graspZone.DisableGrasp();
            _grasperTarget = null;
        }
    }
}
