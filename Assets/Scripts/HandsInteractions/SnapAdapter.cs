using UnityEngine;
using System;

public enum Orientation { POSITIVE_X = 0, POSITIVE_Y = 1, POSITIVE_Z = 2, NEGATIVE_X = 3, NEGATIVE_Y = 4, NEGATIVE_Z = 5 }

[Serializable]
public class SnapAdapter
{
    private static readonly Vector3[] _orientations = { Vector3.right, Vector3.up, Vector3.forward, Vector3.left, Vector3.down, Vector3.back };

    [SerializeField] private bool _snapPosition = false;
    [SerializeField] private bool _snapOrientation = false;
    [SerializeField] private HandTransform _transformOffset = HandTransform.zero;
    [SerializeField] private Orientation _palm = Orientation.POSITIVE_X;
    [SerializeField] private Orientation _fingers = Orientation.POSITIVE_Z;

    /// <summary>
    /// If true, the object's position will snap to match snapOffset when grabbed.
    /// </summary>
    public bool snapPosition
    {
        get { return _snapPosition; }
    }

    /// <summary>
    /// If true, the object's orientation will snap to match snapOffset when grabbed.
    /// </summary>
    public bool snapOrientation
    {
        get { return _snapOrientation; }
    }

    public HandTransform transformOffset
    {
        get { return _transformOffset; }
    }

    public Orientation palm
    {
        get { return _palm; }
    }

    public Orientation fingers
    {
        get { return _fingers; }
    }

    /// <value>
    /// Direction of the grabbable that will face the palm of the hand while grabbed with snap enabled.
    /// </value>
    public Vector3 palmLocalDirection
    {
        get { return _orientations[(int)_palm]; }
    }

    /// <value>
    /// Direction of the grabbable that will face forward (fingers direction) while grabbed with snap enabled. 
    /// </value>
    public Vector3 fingersLocalDirection
    {
        get { return _orientations[(int)_fingers]; }
    }

    public SnapAdapter()
    {
        _transformOffset = HandTransform.zero;
        _palm = Orientation.POSITIVE_X;
        _fingers = Orientation.POSITIVE_Z;
    }

    public SnapAdapter(HandTransform transformOffset, Orientation palmOrientation, Orientation fingersOrientation)
    {
        _transformOffset = transformOffset;
        _palm = palmOrientation;
        _fingers = fingersOrientation;

        if (Mathf.Abs(_palm - _fingers) == 3 || _palm == _fingers)
            throw new ArgumentException("Grabbables palm direction cannot be the same axis as figners direction.");
    }

    /// <summary>
    /// Overwrite the current snap offsets.
    /// </summary>
    /// <param name="newPosOffset"> The new position offset.</param>
    /// <param name="newRotOffset">The new rotation offset.</param>
    public void OverrideOffsets(HandTransform handTransform)
    {
        _transformOffset = handTransform;
    }

    public void OverrideOrientations(Orientation palm, Orientation fingers)
    {
        _palm = palm;
        _fingers = fingers;
    }

    public void OverrideSnaps(HandTransform handTransform, Orientation palm, Orientation fingers)
    {
        _transformOffset = new HandTransform(handTransform.position, handTransform.rotation);
        _palm = palm;
        _fingers = fingers;
    }

    public void OverrideSnaps(SnapAdapter copyAdapter)
    {
        _transformOffset = new HandTransform(copyAdapter.transformOffset.position, copyAdapter.transformOffset.rotation);
        _palm = copyAdapter.palm;
        _fingers = copyAdapter.fingers;
    }

    /// <summary>
    /// Reset the snap offsets to default value.
    /// </summary>
    public void ResetSnaps()
    {
        _transformOffset = HandTransform.zero;
        _palm = Orientation.POSITIVE_X;
        _fingers = Orientation.POSITIVE_Z;
    }
}
