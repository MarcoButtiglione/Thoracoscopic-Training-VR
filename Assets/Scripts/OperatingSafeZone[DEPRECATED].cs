using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script handles the vibration of the controllers when a grabbed object trepasses one or more limits it should not go past.
/// NOTE: there should be only one instance of this script per scene! The operation in the <see cref="OperatingSafeZone.Awake"/> method handle that.
/// </summary>
public class OperatingSafeZone : Singleton<OperatingSafeZone>
{
    private List<OperatingZoneLimit> _operatingZoneLimits = new List<OperatingZoneLimit>();

    /// <summary>
    /// Frequency of the controller vibration.
    /// </summary>
    [Tooltip("Frequency of the controller vibration.")]
    [Range(0.0f, 1.0f)]
    public float vibrationFrequency = 1.0f;

    /// <summary>
    /// Strength of the controller vibration.
    /// </summary>
    [Tooltip("Strength of the controller vibration.")]
    [Range(0.0f, 1.0f)]
    public float vibrationAmplitude = 0.5f;

    /// <summary>
    /// List of limits whose collider have to be considered when deciding if controllers should vibrate or not.
    /// </summary>
    public List<OperatingZoneLimit> operatingZoneLimits
    {
        get { return _operatingZoneLimits; }
    }

    private void Update()
    {
        HandleControllersVibration();
    }

    /// <summary>
    /// Retrieves the <see cref="HandType"/> of the <see cref="Grabber"/> that holds the <see cref="Grabbable"/>
    /// and makes the respective controller vibrate with the given <see cref="OperatingSafeZone.vibrationAmplitude"/> and <see cref="OperatingSafeZone.vibrationFrequency"/>.
    /// </summary>
    private void HandleControllersVibration()
    {
        // bool shouldLeftControllerVibrate = false;
        // bool shouldRightControllerVibrate = false;
        // foreach (OperatingZoneLimit zoneLimit in _operatingZoneLimits)
        // {
        //     foreach (Grabbable collidedGrabbable in zoneLimit.grabbables)
        //     {
        //         if (collidedGrabbable.grabbedBy != null)
        //         {
        //             switch (collidedGrabbable.grabbedBy.handType)
        //             {
        //                 case HandType.Left:
        //                     shouldLeftControllerVibrate = true;
        //                     break;
        //                 case HandType.Right:
        //                     shouldRightControllerVibrate = true;
        //                     break;
        //                 case HandType.Both:
        //                     shouldLeftControllerVibrate = true;
        //                     shouldRightControllerVibrate = true;
        //                     break;
        //                 default:
        //                     shouldLeftControllerVibrate = false;
        //                     shouldRightControllerVibrate = false;
        //                     break;
        //             }
        //         }
        //     }

        //     if (shouldLeftControllerVibrate)
        //     {
        //         OVRInput.SetControllerVibration(vibrationFrequency, vibrationAmplitude, OVRInput.Controller.LTouch);
        //     }
        //     else
        //     {
        //         OVRInput.SetControllerVibration(0.0f, 0.0f, OVRInput.Controller.LTouch);
        //     }

        //     if (shouldRightControllerVibrate)
        //     {
        //         OVRInput.SetControllerVibration(vibrationFrequency, vibrationAmplitude, OVRInput.Controller.RTouch);
        //     }
        //     else
        //     {
        //         OVRInput.SetControllerVibration(0.0f, 0.0f, OVRInput.Controller.RTouch);
        //     }
        // }
    }
}
