using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(CustomHand))]
public class Grabber : MonoBehaviour
{

    /// <value>
    /// Grip trigger thresholds for picking up objects, with some hysteresis.
    /// </value>
    [Range(0.4f, 0.7f)]
    public float grabBegin = 0.55f;

    /// <value>
    /// Grip trigger thresholds for putting down objects, with some hysteresis.
    /// </value>
    [Range(0.0f, 0.39f)]
    public float grabEnd = 0.35f;

    private float _grabState;   // Current grab level to compare with grabBegin & grabEnd (INPUT TRIGGER)

    /// <value>
    /// Child/attached transforms of the grabber, indicating where to snap held objects to (if you snap them).
    /// Its parent is also used for ranking grab targets in case of multiple candidates.
    /// </value>
    [SerializeField] private Transform _gripAnchor = null;

    /// <value>
    /// Child/attached Colliders to detect candidate grabbable objects.
    /// </value>
    [SerializeField] private Collider[] _grabVolumes = null;

    private bool _grabVolumeEnable = true;  // True if grabs volume must be used during grabbing.
    private CustomHand _hand;   // The hand associated to this grabber.
    private Grabbable _grabbedObj = null;   // The object currently grabbed (if any)
    private Vector3 _grabbedObjectPosOffset;    // Position offset of the grabbed object
    private Quaternion _grabbedObjectRotOffset; // Rotation offset ot the grabbed object
    private Dictionary<Grabbable, int> _grabCandidates = new Dictionary<Grabbable, int>();  // All the grabbing candidates

    /// <summary>
    /// The object currently grabbed.
    /// </summary>
    /// <value></value>
    public Grabbable grabbedObj
    {
        get { return _grabbedObj; }
    }

    /// <summary>
    /// True if the grabbed is currently grabbing something
    /// </summary>
    /// <value></value>
    public bool isGrabbing
    {
        get { return _grabbedObj != null; }
    }

    /// <summary>
    /// Returns the type of hand (left or right).
    /// </summary>
    public HandType handType
    {
        get { return _hand.handType; }
    }

    private void Start()
    {
        // Init the hand
        _hand = GetComponent<CustomHand>();

        // LAYER PLAYER COLLISION
    }

    void OnDestroy()
    {
        // Force the grab end if is currently grabbing something
        if (_grabbedObj != null)
        {
            GrabEnd();
        }
    }

    /// <summary>
    /// Check if a grab action has to start or to end.
    /// </summary>
    /// <param name="newGrabState"> The grab float value to compare with the thresholds (the trigger value) </param>
    public void CheckForGrabOrRelease(float newGrabState)
    {
        if ((newGrabState >= grabBegin) && (_grabState < grabBegin))
            GrabBegin();
        else if ((newGrabState <= grabEnd) && (_grabState > grabEnd))
            GrabEnd();

        _grabState = newGrabState;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Get the grabbale
        Grabbable grabbable = other.GetComponent<Grabbable>() ?? other.GetComponentInParent<Grabbable>();
        if (grabbable == null) return;

        // Add the grabbale
        int refCount = 0;
        _grabCandidates.TryGetValue(grabbable, out refCount);
        _grabCandidates[grabbable] = refCount + 1;
    }

    private void OnTriggerExit(Collider other)
    {
        // Get the grabbale
        Grabbable grabbable = other.GetComponent<Grabbable>() ?? other.GetComponentInParent<Grabbable>();
        if (grabbable == null) return;

        // Remove the grabbable
        int refCount = 0;
        bool found = _grabCandidates.TryGetValue(grabbable, out refCount);
        if (!found) return;

        if (refCount > 1)
            _grabCandidates[grabbable] = refCount - 1;
        else
            _grabCandidates.Remove(grabbable);
    }

    private void GrabBegin()
    {
        float closestMagSq = float.MaxValue;    // Init the closest magnitude sqr
        Grabbable closestGrabbable = null;  // The current closes grabbable
        Collider closestGrabbableCollider = null;   // The collider of the current closes grabbable

        // Search for grab target
        foreach (Grabbable grabbable in _grabCandidates.Keys)
        {
            if (!grabbable.CanBeGrabbedBy(_hand.handType)) // if can't grab
                continue;

            // foreach grab points of the current grabbable
            for (int j = 0; j < grabbable.grabPoints.Length; ++j)
            {
                Collider grabbableCollider = grabbable.grabPoints[j];

                // Store/Update closest
                Vector3 closestPointOnBounds = grabbableCollider.ClosestPointOnBounds(_gripAnchor.parent.position);
                float grabbableMagSq = (_gripAnchor.parent.position - closestPointOnBounds).sqrMagnitude;
                if (grabbableMagSq < closestMagSq)
                {
                    closestMagSq = grabbableMagSq;
                    closestGrabbable = grabbable;
                    closestGrabbableCollider = grabbableCollider;
                }
            }
        }

        // Prevent overlaps
        GrabVolumeEnable(false);

        // Perform grabbing
        if (closestGrabbable != null)
        {
            if (closestGrabbable.isGrabbed)
                closestGrabbable.grabbedBy.OffhandGrabbed(closestGrabbable);

            _grabbedObj = closestGrabbable;
            _grabbedObj.Grab(this, closestGrabbableCollider);
            SetUpSnaps();

            // Use teleport to avoid set other colliding object flying during grab
            // Pos and Rot offset previously calculated in this method used here
            MoveGrabbedObject(_hand.lastPos, _hand.lastRot, true);

            // HANDLE COLLISION LAYERS TO AVOID PLAYER COLLISIONS

            // Parenting object (we do not use this feature)
        }
    }

    /// <summary>
    /// Move the current grabbed object
    /// </summary>
    /// <param name="pos"> The target position (usually the hand current position) </param>
    /// <param name="rot"> The target rotation (usually the hand current rotation) </param>
    /// <param name="forceTeleport"> True if the object has to be moved ignoring physics </param>
    public void MoveGrabbedObject(Vector3 pos, Quaternion rot, bool forceTeleport = false)
    {
        if (_grabbedObj == null) return;

#if UNITY_EDITOR
        // SetUpSnaps(); // Debug: uncommet it to set up snaps fluently in the editor (NB: if this line not commented not snapped grab will not work)
#endif

        Rigidbody grabbedRb = _grabbedObj.grabbedRigidbody;
        Vector3 grabbablePos = _gripAnchor.position;
        Quaternion grabbableRot = _gripAnchor.rotation;

        if (forceTeleport)
        {
            grabbedRb.transform.position = grabbablePos;
            grabbedRb.transform.rotation = grabbableRot;
        }
        else
        {
            grabbedRb.MovePosition(grabbablePos);
            grabbedRb.MoveRotation(grabbableRot);
        }
    }

    private void SetUpSnaps()
    {
        // Set up offsets for grabbed object desired position realtive to hand
        if (_grabbedObj.currentSnapAdapter.snapPosition)
        {
            // Apply defined snap position
            if (_hand.handType == HandType.Left)
            {
                _gripAnchor.localPosition = _grabbedObj.currentSnapAdapter.transformOffset.position.leftHand;
            }
            else
            {
                _gripAnchor.localPosition = _grabbedObj.currentSnapAdapter.transformOffset.position.rightHand;
            }
        }
        else
        {
            // Save current relative position when grabbed as snap position
            _gripAnchor.position = _grabbedObj.transform.position;
        }

        if (_grabbedObj.currentSnapAdapter.snapOrientation)
        {
            Quaternion reorientation = Quaternion.identity;
            Quaternion midRotation = Quaternion.FromToRotation(reorientation * _grabbedObj.currentSnapAdapter.fingersLocalDirection, HandTransform.fingerDirection);
            reorientation = midRotation * reorientation;

            float angle = Vector3.SignedAngle(reorientation * _grabbedObj.currentSnapAdapter.palmLocalDirection, HandTransform.palmDirection, HandTransform.fingerDirection);
            midRotation = Quaternion.AngleAxis(angle, reorientation * _grabbedObj.currentSnapAdapter.fingersLocalDirection);
            reorientation = midRotation * reorientation;

            if (_hand.handType == HandType.Left)
            {
                _gripAnchor.localRotation = _grabbedObj.currentSnapAdapter.transformOffset.rotation.leftHand * reorientation;
            }
            else
            {
                _gripAnchor.localRotation = _grabbedObj.currentSnapAdapter.transformOffset.rotation.rightHand * reorientation;
            }
        }
        else
            _gripAnchor.rotation = _grabbedObj.transform.rotation;
    }

    public void ChangeSnaps()
    {
        if (_grabbedObj != null)
            SetUpSnaps();
    }

    private void GrabEnd()
    {
        if (_grabbedObj != null)
        {
            GrabbableRelease(_hand.worldLinearVelocity, _hand.worldAngularVelocity);
        }

        // Re-enable grab volumes to allow overlap events
        GrabVolumeEnable(true);
    }

    private void GrabbableRelease(Vector3 linearVelocity, Vector3 angularVelocity)
    {
        _grabbedObj.Release(linearVelocity, angularVelocity);
        _grabbedObj = null;
    }

    private void GrabVolumeEnable(bool enabled)
    {
        if (_grabVolumeEnable == enabled) return;

        _grabVolumeEnable = enabled;
        for (int i = 0; i < _grabVolumes.Length; ++i)
        {
            _grabVolumes[i].enabled = _grabVolumeEnable;
        }

        if (!_grabVolumeEnable)
            _grabCandidates.Clear();
    }

    private void OffhandGrabbed(Grabbable grabbable)
    {
        if (_grabbedObj == grabbable)
        {
            GrabbableRelease(Vector3.zero, Vector3.zero);
        }
    }

    public void ForceGrab(Grabbable grabbable)
    {
        _grabbedObj = grabbable;
        grabbable.ForceGrab(this);
        SetUpSnaps();

        // Use teleport to avoid set other colliding object flying during grab
        // Pos and Rot offset previously calculated in this method used here
        MoveGrabbedObject(_hand.lastPos, _hand.lastRot, true);
    }

    public void ForceRelease()
    {
        _grabbedObj.ForceRelease(Vector3.zero, Vector3.zero, false);
        _grabbedObj = null;
    }
}
