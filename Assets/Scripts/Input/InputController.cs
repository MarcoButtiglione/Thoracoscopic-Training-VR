using System.Collections.Generic;
using UnityEngine;
using System;

public class InputController : Singleton<InputController>
{
    [SerializeField] private Transform trackingSpace = null;

    /// <summary>
    /// 
    /// </summary>
    public CustomHand leftHand;

    /// <summary>
    /// 
    /// </summary>
    public CustomHand rightHand;

    [Range(0.0f, 0.2f)]
    public float inputDeadzone = 0.05f;

    private class ControllerInputUtils
    {
        public bool resetIndexTrigger;
        public bool resetHandTrigger;
        public bool resetThumbstick;
    }

    [Serializable]
    private class VibrationsUtils
    {
        [Tooltip("Frequency of the controller vibration.")]
        [Range(0.0f, 1.0f)]
        public float frequency = 1;

        [Tooltip("Strength of the controller vibration.")]
        [Range(0.0f, 1.0f)]
        public float amplitude = .5f;

        public VibrationsUtils(float freq = 1f, float ampl = .5f)
        {
            frequency = freq;
            amplitude = ampl;
        }
    }

    private ControllerInputUtils _leftInputUtils = new ControllerInputUtils();
    private ControllerInputUtils _rightUtils = new ControllerInputUtils();
    private Dictionary<OVRInput.Controller, ControllerInputUtils> _inputUtils;
    private Dictionary<OVRInput.Controller, CustomHand> _controllers;

    [SerializeField] private VibrationsUtils _leftControllerVibrationSpecs = new VibrationsUtils();
    [SerializeField] private VibrationsUtils _rightControllerVibrationSpecs = new VibrationsUtils();
    private Dictionary<OVRInput.Controller, VibrationsUtils> _vibrationsUtils;
    // private Dictionary<OVRInput.Controller, float> _vibrations;

    private Dictionary<Grabbable, VibrationsUtils> _vibrationLeftHand;
    private Dictionary<Grabbable, VibrationsUtils> _vibrationRightHand;


    private void Awake()
    {
        if (leftHand == null || rightHand == null)
            throw new Exception("Hand missing reference.");

        _controllers = new Dictionary<OVRInput.Controller, CustomHand>() {
            {OVRInput.Controller.LTouch, leftHand},
            {OVRInput.Controller.RTouch, rightHand}
        };

        _inputUtils = new Dictionary<OVRInput.Controller, ControllerInputUtils>(){
            {OVRInput.Controller.LTouch, _leftInputUtils},
            {OVRInput.Controller.RTouch, _rightUtils}
        };

        // _vibrations = new Dictionary<OVRInput.Controller, float>(){
        //     {OVRInput.Controller.LTouch, 0f},
        //     {OVRInput.Controller.RTouch, 0f}
        // };

        _vibrationsUtils = new Dictionary<OVRInput.Controller, VibrationsUtils>(){
            {OVRInput.Controller.LTouch, _leftControllerVibrationSpecs},
            {OVRInput.Controller.RTouch, _rightControllerVibrationSpecs}
        };

        _vibrationLeftHand = new Dictionary<Grabbable, VibrationsUtils>();
        _vibrationRightHand = new Dictionary<Grabbable, VibrationsUtils>();


    }

    private void Update()
    {
        ListenController(OVRInput.Controller.RTouch);
        ListenController(OVRInput.Controller.LTouch);
        TrackControllerMotion(OVRInput.Controller.RTouch);
        TrackControllerMotion(OVRInput.Controller.LTouch);
        UpdateControllerVibration(OVRInput.Controller.RTouch);
        UpdateControllerVibration(OVRInput.Controller.LTouch);
    }

    public void SetHandVibration(HandType targetHand, Grabbable vibratingObj, float amplitude = .5f, float frequency = 1f)
    {

        VibrationsUtils vibration = new VibrationsUtils(frequency, amplitude);
        switch (targetHand)
        {
            case HandType.Left:
                _vibrationLeftHand[vibratingObj] = vibration;
                break;
            case HandType.Right:
                _vibrationRightHand[vibratingObj] = vibration;
                break;
            case HandType.Both:
                _vibrationLeftHand[vibratingObj] = vibration;
                _vibrationRightHand[vibratingObj] = vibration;
                break;
            default:
                break;
        }

        // switch (targetHand)
        // {
        //     case HandType.Left:
        //         if (vibration > _vibrations[OVRInput.Controller.LTouch])
        //             _vibrations[OVRInput.Controller.LTouch] = vibration;
        //         break;
        //     case HandType.Right:
        //         if (vibration > _vibrations[OVRInput.Controller.RTouch])
        //             _vibrations[OVRInput.Controller.RTouch] = vibration;
        //         break;
        //     case HandType.Both:
        //         if (vibration > _vibrations[OVRInput.Controller.LTouch])
        //             _vibrations[OVRInput.Controller.LTouch] = vibration;
        //         if (vibration > _vibrations[OVRInput.Controller.RTouch])
        //             _vibrations[OVRInput.Controller.RTouch] = vibration;
        //         break;
        //     default:
        //         _vibrations[OVRInput.Controller.LTouch] = 0f;
        //         _vibrations[OVRInput.Controller.RTouch] = 0f;
        //         break;
        // }
    }

    public void StopHandVibration(HandType targetHand, Grabbable vibratingObj)
    {
        switch (targetHand)
        {
            case HandType.Left:
                _vibrationLeftHand.Remove(vibratingObj);
                break;
            case HandType.Right:
                _vibrationRightHand.Remove(vibratingObj);
                break;
            case HandType.Both:
                _vibrationLeftHand.Remove(vibratingObj);
                _vibrationRightHand.Remove(vibratingObj);
                break;
            default:
                break;
        }
        // switch (targetHand)
        // {
        //     case HandType.Left:
        //         _vibrations[OVRInput.Controller.LTouch] = 0f;
        //         break;
        //     case HandType.Right:
        //         _vibrations[OVRInput.Controller.RTouch] = 0f;
        //         break;
        //     case HandType.Both:
        //         _vibrations[OVRInput.Controller.LTouch] = 0f;
        //         _vibrations[OVRInput.Controller.RTouch] = 0f;
        //         break;
        //     default:
        //         _vibrations[OVRInput.Controller.LTouch] = 0f;
        //         _vibrations[OVRInput.Controller.RTouch] = 0f;
        //         break;
        // }
    }

    // public void OverrideVibrationSpecs(HandType targetHand, float frequency, float amplitude)
    // {
    //     switch (targetHand)
    //     {
    //         case HandType.Left:
    //             _vibrationsUtils[OVRInput.Controller.LTouch].frequency = frequency;
    //             // _vibrationsUtils[OVRInput.Controller.LTouch].amplitude = amplitude;
    //             break;
    //         case HandType.Right:
    //             _vibrationsUtils[OVRInput.Controller.RTouch].frequency = frequency;
    //             // _vibrationsUtils[OVRInput.Controller.RTouch].amplitude = amplitude;
    //             break;
    //         case HandType.Both:
    //             _vibrationsUtils[OVRInput.Controller.LTouch].frequency = frequency;
    //             // _vibrationsUtils[OVRInput.Controller.LTouch].amplitude = amplitude;
    //             _vibrationsUtils[OVRInput.Controller.RTouch].frequency = frequency;
    //             // _vibrationsUtils[OVRInput.Controller.RTouch].amplitude = amplitude;
    //             break;
    //         default:
    //             // Do nothing
    //             break;
    //     }
    // }

    private void UpdateControllerVibration(OVRInput.Controller targetController)
    {
        Dictionary<Grabbable, VibrationsUtils> dict;
        if (targetController == OVRInput.Controller.LTouch)
            dict = _vibrationLeftHand;
        else
            dict = _vibrationRightHand;


        VibrationsUtils vibration = new VibrationsUtils(0f, 0f);
        float max = 0f;
        foreach (var key in dict.Keys)
        {
            if (dict[key].amplitude >= max)
            {
                max = dict[key].amplitude;
                vibration = dict[key];
            }
        }

        if (max > 0f)
            OVRInput.SetControllerVibration(vibration.frequency, vibration.amplitude, targetController);
        else
            OVRInput.SetControllerVibration(0f, 0f, targetController);

        // if (_vibrations[targetController] > 0)
        //     OVRInput.SetControllerVibration(_vibrationsUtils[targetController].frequency, _vibrations[targetController], targetController);
        // else
        //     OVRInput.SetControllerVibration(.0f, .0f, targetController);
    }

    private void ListenController(OVRInput.Controller targetController)
    {
        CustomHand hand = _controllers[targetController];
        var utils = _inputUtils[targetController];

        bool buttonPressed;
        bool nearTouchState;
        float axis1DValue;
        Vector2 axis2DValue;

        // Index trigger
        axis1DValue = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, targetController);
        if (axis1DValue > inputDeadzone)
        {
            utils.resetIndexTrigger = true;
            hand.OnIndexTriggerPressed(axis1DValue);
        }
        else if (utils.resetIndexTrigger)
        {
            utils.resetIndexTrigger = false;
            hand.OnIndexTriggerPressed(0.0f);
        }

        // Hand trigger
        axis1DValue = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, targetController);
        if (axis1DValue > inputDeadzone)
        {
            utils.resetHandTrigger = true;
            hand.OnHandTriggerPressed(axis1DValue);
        }
        else if (utils.resetHandTrigger)
        {
            utils.resetHandTrigger = false;
            hand.OnHandTriggerPressed(0.0f);
        }

        // Thumbstick
        axis2DValue = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, targetController);
        if (Mathf.Abs(axis2DValue.x) > inputDeadzone || Mathf.Abs(axis2DValue.y) > inputDeadzone)
        {
            utils.resetThumbstick = true;
            axis2DValue.x = Mathf.Abs(axis2DValue.x) <= inputDeadzone ? 0 : axis2DValue.x; // Apply deadzone to single axis
            axis2DValue.y = Mathf.Abs(axis2DValue.y) <= inputDeadzone ? 0 : axis2DValue.y;
            hand.OnThumbstickHorizontalMoved(axis2DValue.x);
            hand.OnThumbstickVerticalMoved(axis2DValue.y);
        }
        else if (utils.resetThumbstick)
        {
            utils.resetThumbstick = false;
            hand.OnThumbstickHorizontalMoved(0.0f);
            hand.OnThumbstickVerticalMoved(0.0f);
        }

        // Thumbstick button
        buttonPressed = OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick, targetController);
        if (buttonPressed)
            hand.OnButtonThumbstickPressed();

        // South button
        buttonPressed = OVRInput.GetDown(OVRInput.Button.One, targetController);
        if (buttonPressed)
            hand.OnButtonSouthPressed();

        // North button
        buttonPressed = OVRInput.GetDown(OVRInput.Button.Two, targetController);
        if (buttonPressed)
            hand.OnButtonNorthPressed();

        // Start button
        if (targetController == OVRInput.Controller.LTouch) // Perform just once per update
        {
            buttonPressed = OVRInput.GetDown(OVRInput.Button.Start);
            if (buttonPressed)
            {
                //Missing method
            }
        }

        // Index point pose
        nearTouchState = OVRInput.Get(OVRInput.NearTouch.PrimaryIndexTrigger, targetController);
        hand.OnIndexPointPose(!nearTouchState);

        // Thumbs up pose
        nearTouchState = OVRInput.Get(OVRInput.NearTouch.PrimaryThumbButtons, targetController);
        hand.OnThumbsUpPose(!nearTouchState);
    }

    // This method was achieved with lot of trail and error, OVRInput method for controller tracking are not well documented and it is not clear the ref system (or the space) of the traking procedure
    // it seems that controller velocity is tracked in tracking space local orientation while controller angular velocity is traked in each controller local axis
    private void TrackControllerMotion(OVRInput.Controller targetController)
    {
        CustomHand hand = _controllers[targetController];
        // OVRPose localPose = new OVRPose()
        // {
        //     position = OVRInput.GetLocalControllerPosition(targetController),
        //     orientation = OVRInput.GetLocalControllerRotation(targetController)
        // };
        // OVRPose trackingSpace = localPose.Inverse();

        Vector3 linearVelocity = trackingSpace.TransformDirection(OVRInput.GetLocalControllerVelocity(targetController));
        Vector3 angularVelocity = -trackingSpace.TransformDirection(OVRInput.GetLocalControllerAngularVelocity(targetController));

#if UNITY_EDITOR
        OVRPose localPose = new OVRPose()
        {
            position = OVRInput.GetLocalControllerPosition(targetController),
            orientation = OVRInput.GetLocalControllerRotation(targetController),
        };
        angularVelocity = localPose.orientation * OVRInput.GetLocalControllerAngularVelocity(targetController);
        angularVelocity = trackingSpace.TransformDirection(angularVelocity);
#endif


        hand.OnControllerMoved(linearVelocity, angularVelocity);
    }
}
