using UnityEngine;

public enum CustomHandPoseId
{
    Default,
    Generic,
    LaparoscopicPliers
}

/// <summary>
/// Stores pose-specific data such as the animation id.
/// </summary>
public class CustomHandPose : MonoBehaviour
{
    [SerializeField] private CustomHandPoseId _poseId = CustomHandPoseId.Default;

    public CustomHandPoseId PoseId
    {
        get { return _poseId; }
    }
}
