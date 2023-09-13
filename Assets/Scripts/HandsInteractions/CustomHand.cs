using UnityEngine;
using System;

public class CustomHand : MonoBehaviour
{
    // The type of the hand, must be changed in Left or Right.
    [SerializeField] private HandType _handType = HandType.None;

    /// <summary>
    /// This is the object that the user may hold. This field must be assigned by the grab action.
    /// </summary>
    public InputDispatcher item;

    // The grabber of this han (if any)
    [SerializeField] private Grabber _handGrabber = null;

    // The anchor that must be folowed for tracking space movement
    [SerializeField] private Transform _anchorParent = null;

    // The animator used for hand pose animations
    [SerializeField] private Animator _animator = null;

    [SerializeField] private CustomHandPose _defaultGrabPose = null;

    private Quaternion _lastRot;    // Reference to the last tracking space orientation
    private Vector3 _lastPos;   // Reference to the last tracking space position
    private Vector3 _anchorOffsetPos;
    private Quaternion _anchorOffsetRot;
    private Vector3 _handLinearVelocity = new Vector3();    // Current hand linear velocity
    private Vector3 _handAngularVelocity = new Vector3();   // Current hand angular velocity

    #region Animation

    // IDs of the layers an parameters of the animator.
    // private int _animLayerFlex = -1;
    private int _animLayerPoint = -1;
    private int _animLayerThumb = -1;
    private int _animParamFlex = -1;
    private int _animParamPose = -1;
    private int _animParamPinch = -1;

    // Name of animator's layers and parameters.
    // public const string ANIM_LAYER_NAME_FLEX = "Flex Layer";
    public const string ANIM_LAYER_NAME_POINT = "Point Layer";
    public const string ANIM_LAYER_NAME_THUMB = "Thumb Layer";
    public const string ANIM_PARAM_NAME_FLEX = "Flex";
    public const string ANIM_PARAM_NAME_PINCH = "Pinch";
    public const string ANIM_PARAM_NAME_POSE = "Pose";

    // Animator utils.
    private float _flexingLevel = 0.0f;
    private float _pinchLevel = 0.0f;
    private bool _isPointing = false;
    private bool _isGivingThumbsUp = false;
    private float _pointBlend = 0.0f;
    private float _thumbsUpBlend = 0.0f;
    public float inputRateChange = 20.0f;

    #endregion

    /// <value>The type of the hand.</value>
    public HandType handType
    {
        get { return _handType; }
    }

    /// <value>True if the hand is currently pointing.</value>
    public bool isPointing
    {
        get { return _isPointing; }
    }

    /// <value>Last hand position in the tracking space.</value>
    public Vector3 lastPos
    {
        get { return _lastPos; }
    }

    /// <value>Last hand orientation in the tracking space.</value>
    public Quaternion lastRot
    {
        get { return _lastRot; }
    }

    /// <value>The current linear velocity of the hand in the space.</value>
    public Vector3 worldLinearVelocity
    {
        get { return _handLinearVelocity; }
    }

    /// <value>The current angular velocity of the hand in the space.</value>
    public Vector3 worldAngularVelocity
    {
        get { return _handAngularVelocity; }
    }

    private void Awake()
    {
        // Validate hand type
        if ((_handType & (HandType.Left | HandType.Right)) == 0)
            throw new ArgumentException("Hand type must have a value between Left or Right.");

        // Init anchor offsets
        _anchorOffsetPos = transform.localPosition;
        _anchorOffsetRot = transform.localRotation;

        // Register the anchor update to the camera rig tracking space update
        OVRCameraRig rig = transform.GetComponentInParent<OVRCameraRig>();
        if (rig != null)
        {
            rig.UpdatedAnchors += (r) => { OnUpdatedAnchors(); };
        }
    }

    private void Start()
    {
        _lastPos = transform.position;
        _lastRot = transform.rotation;

        // If a specific anchor is not set make this gameobject the anchor as default
        if (_anchorParent == null)
        {
            _anchorParent = gameObject.transform;
        }

        // _animLayerFlex = _animator.GetLayerIndex(ANIM_LAYER_NAME_FLEX);
        _animLayerPoint = _animator.GetLayerIndex(ANIM_LAYER_NAME_POINT);
        _animLayerThumb = _animator.GetLayerIndex(ANIM_LAYER_NAME_THUMB);
        _animParamFlex = Animator.StringToHash(ANIM_PARAM_NAME_FLEX);
        _animParamPinch = Animator.StringToHash(ANIM_PARAM_NAME_PINCH);
        _animParamPose = Animator.StringToHash(ANIM_PARAM_NAME_POSE);
    }

    private void Update()
    {
        _pointBlend = InputValueRateChange(_isPointing, _pointBlend);
        _thumbsUpBlend = InputValueRateChange(_isGivingThumbsUp, _thumbsUpBlend);

        // todo check collisions behaviour.

        UpdateAnimationState();
    }

    /// <summary>
    /// The method called on index trigger pressed.
    /// </summary>
    /// <param name="value"> Trigger flex value </param>
    public void OnIndexTriggerPressed(float value)
    {
        if (item)
        {
            // item.OnIndexTriggerPressed(value);
            item.DispatchInputValue(Control.IndexTrigger, value);
            return;
        }

        _pinchLevel = value;
    }

    /// <summary>
    /// The method called on hand trigger pressed.
    /// </summary>
    /// <param name="value"> Trigger flex value </param>
    public void OnHandTriggerPressed(float value)
    {
        _handGrabber.CheckForGrabOrRelease(value);
        if (_handGrabber.grabbedObj != null)
        {
            item = _handGrabber.grabbedObj.GetComponent<InputDispatcher>();
            return;
        }

        item = null;
        _flexingLevel = value;
    }

    /// <summary>
    /// The method called on thumbstick horizontal movement.
    /// </summary>
    /// <param name="value"> Thumbstick horizontal position value  </param>
    public void OnThumbstickHorizontalMoved(float value)
    {
        if (item)
        {
            item.DispatchInputValue(Control.ThumbstickHorizontal, value);
        }
    }

    /// <summary>
    /// The method called on thumbstick vertical movement.
    /// </summary>
    /// <param name="value"> Thumbstick vertical position value </param>
    public void OnThumbstickVerticalMoved(float value)
    {
        if (item)
        {
            item.DispatchInputValue(Control.ThumbstickVertical, value);
        }
    }

    /// <summary>
    /// The method called on thumbstick button pressed.
    /// </summary>
    public void OnButtonThumbstickPressed()
    {
        if (item)
        {
            item.DispatchInput(Control.ButtonThumbstick);
        }
    }

    /// <summary>
    /// The method called on south button pressed.
    /// </summary>
    public void OnButtonSouthPressed()
    {
        if (item)
        {
            item.DispatchInput(Control.ButtonSouth);
        }
    }

    /// <summary>
    /// The method called on north button pressed.
    /// </summary>
    public void OnButtonNorthPressed()
    {
        if (item)
        {
            item.DispatchInput(Control.ButtonNorth);
        }
    }

    /// <summary>
    /// The method called on controller movement in the tracking space.
    /// </summary>
    /// <param name="handLinearVelocity"> The controller linear velocity (relative to tracking space)</param>
    /// <param name="handAngularVelocity"> The controller angular velocity (relative to controller space)</param>
    public void OnControllerMoved(Vector3 handLinearVelocity, Vector3 handAngularVelocity)
    {
        // If something seem to behave wrongly, check the OVRGrabber implementation with OVRPose (NB: OVRGrabber angular velocity is bugged)
        _handLinearVelocity = WorldScaler.Instance.GetScaledVector3(handLinearVelocity);
        // _handAngularVelocity = _anchorParent.localToWorldMatrix * handAngularVelocity;
        _handAngularVelocity = WorldScaler.Instance.GetScaledVector3(handAngularVelocity);
    }

    /// <summary>
    /// The method called on index pointing pose.
    /// </summary>
    /// <param name="isPointing"> Tre if the index is currently pointing </param>
    public void OnIndexPointPose(bool isPointing)
    {
        _isPointing = isPointing;
    }

    /// <summary>
    /// The method called on thumbs up pose.
    /// </summary>
    /// <param name="isGivingThumbsUp"> True if the thumbs is currently up </param>
    public void OnThumbsUpPose(bool isGivingThumbsUp)
    {
        _isGivingThumbsUp = isGivingThumbsUp;
    }

    private float InputValueRateChange(bool isDown, float value)
    {
        float rateDelta = Time.deltaTime * inputRateChange;
        float sign = isDown ? 1.0f : -1.0f;
        return Mathf.Clamp01(value + rateDelta * sign);
    }

    private void UpdateAnimationState()
    {
        // Handling of the grab pose w.r.t. the grabbed item.
        CustomHandPose grabPose = _defaultGrabPose;
        if (_handGrabber.isGrabbing)
        {
            CustomHandPose customPose = _handGrabber.grabbedObj.GetComponent<CustomHandPose>();
            if (customPose != null) grabPose = customPose;
        }
        CustomHandPoseId handPoseId = grabPose.PoseId;
        _animator.SetInteger(_animParamPose, (int)handPoseId);

        // Flex animation (blend between open hand and fully closed fist).
        _animator.SetFloat(_animParamFlex, _flexingLevel);

        // Pinch animation.
        _animator.SetFloat(_animParamPinch, _pinchLevel);

        // Point animation.
        bool canPoint = !_handGrabber.isGrabbing; // grabPose.AllowPointing;
        float pointLevel = canPoint ? _pointBlend : 0.0f;
        _animator.SetLayerWeight(_animLayerPoint, pointLevel);

        // Thumbs up animation.
        bool canThumbsUp = !_handGrabber.isGrabbing; // grabPose.AllowThumbsUp;
        float thumbsUpLevel = canThumbsUp ? _thumbsUpBlend : 0.0f;
        _animator.SetLayerWeight(_animLayerThumb, thumbsUpLevel);
    }

    // Update the hands anchors and move the grabbed object
    private void OnUpdatedAnchors()
    {
        Vector3 destPos = _anchorParent.TransformPoint(_anchorOffsetPos);
        Quaternion destRot = _anchorParent.rotation * _anchorOffsetRot;

        // Hand moved by parenting

        _handGrabber.MoveGrabbedObject(destPos, destRot);

        _lastPos = transform.position;
        _lastRot = transform.rotation;
    }
}

/// <summary>
/// The possible types of an hand.
/// </summary>
public enum HandType
{
    None = OVRInput.Controller.None,
    Both = OVRInput.Controller.Touch,
    Right = OVRInput.Controller.RTouch,
    Left = OVRInput.Controller.LTouch
}
