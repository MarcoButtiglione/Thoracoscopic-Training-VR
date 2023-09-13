using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody))]
public class Grabbable : MonoBehaviour
{
    [SerializeField] private HandType _allowedGrabbingHand = HandType.Both;
    [SerializeField] private bool _allowStealGrab = true;
    [SerializeField] private SnapAdapter[] _snapAdapters;
    [SerializeField] private Collider[] _grabPoints = null;

    private bool _enableGrab = true;
    private Collider _grabbedCollider = null;
    private Grabber _grabbedBy = null;
    private bool _grabbedKinematic = false;
    private Rigidbody _rb;
    private int _snapIndex = 0;

    private float _vibrationFeedback = 0f;

    /// <summary>
    /// The type of the hand that can grab this object.
    /// </summary>
    /// <value></value>
    public HandType allowedGrabbinghand
    {
        get { return _allowedGrabbingHand; }
        set { _allowedGrabbingHand = value; }
    }

    /// <summary>
    /// If true, the object can currently be grabbed.
    /// </summary>
    public bool enableGrab
    {
        get { return _enableGrab; }
        set { _enableGrab = value; }
    }

    /// <summary>
    /// If true, the object is currently grabbed.
    /// </summary>
    public bool isGrabbed
    {
        get { return _grabbedBy != null; }
    }


    /// <value> The default kinematic value of the grabbable </value>
    public bool defaultKinematic
    {
        get { return _grabbedKinematic; }
        set { _grabbedKinematic = value; }
    }

    public SnapAdapter currentSnapAdapter
    {
        get { return _snapAdapters[_snapIndex]; }
    }

    /// <summary>
    /// The Grabber currently grabbing this object.
    /// </summary>
    public Grabber grabbedBy
    {
        get { return _grabbedBy; }
    }

    /// <summary>
    /// The Rigidbody of the collider that was used to grab this object.
    /// </summary>
    public Rigidbody grabbedRigidbody
    {
        get { return _grabbedCollider != null ? _grabbedCollider.attachedRigidbody : _grabPoints[0].attachedRigidbody; }
    }

    /// <summary>
    /// The contact point(s) where the object was grabbed.
    /// </summary>
    public Collider[] grabPoints
    {
        get { return _grabPoints; }
        set { _grabPoints = value; }
    }

    public float vibrationFeedback
    {
        get { return _vibrationFeedback; }
        set
        {
            _vibrationFeedback = value;
            if (_grabbedBy != null)
                if (vibrationFeedback > 0)
                    InputController.Instance.SetHandVibration(_grabbedBy.handType, this, _vibrationFeedback);
                else
                    InputController.Instance.StopHandVibration(_grabbedBy.handType, this);
        }
    }

    private void Awake()
    {
        if (_grabPoints.Length == 0)
        {
            // Get the collider from the grabbable
            Collider collider = this.GetComponent<Collider>();
            if (collider == null)
            {
                throw new ArgumentException("Grabbables cannot have zero grab points and no collider -- please add a grab point or collider.");
            }

            // Create a default grab point
            _grabPoints = new Collider[1] { collider };
        }

        if (_snapAdapters.Length == 0)
        {
            _snapAdapters = new SnapAdapter[1] { new SnapAdapter() };
        }
    }

    private void Start()
    {
        _rb = gameObject.GetComponent<Rigidbody>();
        _grabbedKinematic = _rb.isKinematic;
    }

    /// <summary>
    /// Method to be called to check if the object can be currently grabbed by a specific hand.
    /// </summary>
    /// <param name="handType"> The grabbing hand type </param>
    /// <returns> Rrue if the object can be currently grabbed by a specific hand. </returns>
    public bool CanBeGrabbedBy(HandType handType)
    {
        if (_allowedGrabbingHand == HandType.Both || handType == _allowedGrabbingHand)
        {
            if (!_enableGrab)
                return false;

            if (!isGrabbed || _allowStealGrab)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Grab the object with a specifica grabber.
    /// </summary>
    /// <param name="grabber"> The grabber that is grabbing. </param>
    /// <param name="grabPoint"> The considered object collider.  </param>
    public void Grab(Grabber grabber, Collider grabPoint)
    {
        _grabbedBy = grabber;
        _grabbedCollider = grabPoint;
        _rb.isKinematic = true;

        InputController.Instance.SetHandVibration(grabber.handType, this, _vibrationFeedback);
    }

    /// <summary>
    /// Force the object to a specific Grabber.
    /// </summary>
    /// <param name="grabber"> The target Grabber</param>
    public void ForceGrab(Grabber grabber)
    {
        _grabbedBy = grabber;
        _grabbedCollider = _grabPoints[0];
        _rb.isKinematic = true;

        InputController.Instance.SetHandVibration(grabber.handType, this, _vibrationFeedback);
    }

    /// <summary>
    /// Release the object from a grabber.
    /// </summary>
    /// <param name="linearVelocity"> The linear velocity of the hand during the release. </param>
    /// <param name="angularVelocity"> The angular velocity of the hand during the release. </param>
    public void Release(Vector3 linearVelocity, Vector3 angularVelocity)
    {
        _rb.isKinematic = _grabbedKinematic;
        _rb.velocity = linearVelocity;
        _rb.angularVelocity = angularVelocity;

        InputController.Instance.SetHandVibration(_grabbedBy.handType, this, 0f);

        _grabbedBy = null;
        _grabbedCollider = null;
    }

    /// <summary>
    /// Force the release of the object.
    /// </summary>
    /// <param name="linearVelocity">The linear velocity of the hand during the release.</param>
    /// <param name="angularVelocity">The angular velocity of the hand during the release.</param>
    /// <param name="resetKinematic">True if the object must reset its kinmatic state to the original one. Default to true.</param>
    public void ForceRelease(Vector3 linearVelocity, Vector3 angularVelocity, bool resetKinematic = true)
    {
        if (resetKinematic)
            _rb.isKinematic = _grabbedKinematic;
        _rb.velocity = linearVelocity;
        _rb.angularVelocity = angularVelocity;

        InputController.Instance.SetHandVibration(_grabbedBy.handType, this, 0f);

        _grabbedBy = null;
        _grabbedCollider = null;
    }

    /// <summary>
    /// Overwrite the current grabber.
    /// </summary>
    /// <param name="newGrabber">New <see cref="Grabber"/> grabber to be set to this <see cref="Grabbable"/>.</param>
    public void OverrideGrabber(Grabber newGrabber)
    {
        if (_grabbedBy != null)
            InputController.Instance.StopHandVibration(_grabbedBy.handType, this);
        if (newGrabber != null)
            InputController.Instance.SetHandVibration(newGrabber.handType, this, _vibrationFeedback);
        _grabbedBy = newGrabber;
    }

    public void SwitchSnapAdapter()
    {
        _snapIndex++;
        if (_snapIndex >= _snapAdapters.Length)
            _snapIndex = 0;

        grabbedBy.ChangeSnaps();
    }
}



