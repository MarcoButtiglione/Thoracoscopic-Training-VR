using UnityEngine;

public class PhysicalButton : Button3D
{
    [SerializeField] private Transform _button = null;

    [Range(0, 0.01f)]
    [SerializeField] private float upperLimit = .001f;
    [Range(0, 0.01f)]
    [SerializeField] private float lowerLimit = .003f;


    private Vector3 _buttonOriginalPosition;
    private Quaternion _buttonOriginalOrientation;



    protected override void Awake()
    {
        // if (_buttonTrigger == null)
        //     throw new ArgumentException("Button Trigger must not be null.");
        base.Awake();
        _buttonOriginalPosition = _button.localPosition;
        _buttonOriginalOrientation = _button.localRotation;
    }

    // Update is called once per frame
    void Update()
    {
        CheckPositionLimits();
    }

    private void CheckPositionLimits()
    {
        Vector3 newPos = new Vector3(_buttonOriginalPosition.x, _button.localPosition.y, _buttonOriginalPosition.z);
        if (upperLimit > 0)
        {
            if (_button.localPosition.y > _buttonOriginalPosition.y + upperLimit)
                newPos.y = _buttonOriginalPosition.y + upperLimit;
        }

        if (lowerLimit > 0)
        {
            if (_button.localPosition.y < _buttonOriginalPosition.y - lowerLimit)
                newPos.y = _buttonOriginalPosition.y - lowerLimit;
        }

        _button.localPosition = newPos;
        _button.localRotation = _buttonOriginalOrientation;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(_button.TransformPoint(new Vector3(_buttonOriginalPosition.x, _buttonOriginalPosition.y + upperLimit, _buttonOriginalPosition.z)), .005f);
        Gizmos.DrawWireSphere(_button.TransformPoint(new Vector3(_buttonOriginalPosition.x, _buttonOriginalPosition.y - lowerLimit, _buttonOriginalPosition.z)), .005f);
    }
}
