using System.Collections.Generic;
using System;
using UnityEngine;

[RequireComponent(typeof(Grabbable))]
public class IKPivotTarget : MonoBehaviour
{
    [SerializeField] private GameObject _grabPointsParent = null;
    private Grabbable _grabbable;
    private Collider[] _originalGrabPoints;
    private List<GameObject> _grabPointsOverriders;

    /// <value>The grabbable of the target.</value>
    public Grabbable grabbable
    {
        get { return _grabbable; }
    }

    /// <value>The Grabber currently grabbing the target.</value>
    public Grabber grabbedBy
    {
        get { return _grabbable.grabbedBy; }
    }


    // Start is called before the first frame update
    void Start()
    {
        // Init the IKTarget
        _grabbable = GetComponent<Grabbable>();
        _grabbable.allowedGrabbinghand = HandType.None;
        _grabbable.enableGrab = false;
        _originalGrabPoints = _grabbable.grabPoints;
        _grabPointsOverriders = new List<GameObject>();
        if (_grabPointsParent == null)
            throw new ArgumentException("Grab Points Parent must be defined and parented to IKPivotTarget.");

        if (_grabPointsParent.transform.parent != transform)
        {
            _grabPointsParent.transform.parent = transform;
            Debug.LogWarning("Grab Points Parent must be child of IKPivotTarget. Forced.");
        }

        Collider initFakeGrabPoint = GetComponent<Collider>();
        initFakeGrabPoint.isTrigger = true;
    }

    /// <summary>
    /// Overrinde the target input dispatcher.
    /// </summary>
    /// <param name="overrider">The overrider</param>
    public void OverrideInputs(InputDispatcher overrider)
    {
        // Set a copy of the component to this.
        Component copy = gameObject.AddComponent(overrider.GetType());
        System.Reflection.FieldInfo[] fields = overrider.GetType().GetFields();
        foreach (System.Reflection.FieldInfo field in fields)
        {
            field.SetValue(copy, field.GetValue(overrider));
        }
    }

    /// <summary>
    /// Reset the input dispatcher of the target.
    /// </summary>
    public void ResetInputs()
    {
        // Destroy the input dispatcher
        Destroy(gameObject.GetComponent<InputDispatcher>());
    }

    public void OverrideGrabPoints(Collider[] overriders, Quaternion insertedOrientation)
    {
        Collider[] newGrabPoints = new Collider[overriders.Length];

        int i = 0;
        foreach (Collider overrider in overriders)
        {
            GameObject grabPoint = new GameObject(this.name + " grabPoint overrider " + i);
            grabPoint.transform.parent = _grabPointsParent.transform;

            if (overrider.transform.parent == null)
            {
                grabPoint.transform.localPosition = Vector3.zero;
                grabPoint.transform.localRotation = Quaternion.identity;
            }
            else
            {
                grabPoint.transform.localPosition = overrider.transform.localPosition;
                grabPoint.transform.localRotation = overrider.transform.localRotation;
            }

            // // World space version (does not work properly)
            // grabPoint.transform.position = overrider.transform.position;
            // grabPoint.transform.rotation = overrider.transform.localRotation * insertedOrientation;

            Collider newCollider = new Collider(); // default
            if (overrider.GetType() == typeof(BoxCollider))
            {
                BoxCollider over = (BoxCollider)grabPoint.AddComponent(overrider.GetType());
                over.center = ((BoxCollider)overrider).center;
                over.size = ((BoxCollider)overrider).size;
                newCollider = over;
            }
            else if (overrider.GetType() == typeof(SphereCollider))
            {
                SphereCollider over = (SphereCollider)grabPoint.AddComponent(overrider.GetType());
                over.center = ((SphereCollider)overrider).center;
                over.radius = ((SphereCollider)overrider).radius;
                newCollider = over;
            }
            else if (overrider.GetType() == typeof(CapsuleCollider))
            {
                CapsuleCollider over = (CapsuleCollider)grabPoint.AddComponent(overrider.GetType());
                over.center = ((CapsuleCollider)overrider).center;
                over.radius = ((CapsuleCollider)overrider).radius;
                over.height = ((CapsuleCollider)overrider).height;
                over.direction = ((CapsuleCollider)overrider).direction;
                newCollider = over;
            }

            _grabPointsOverriders.Add(grabPoint);
            newGrabPoints[i] = newCollider;

            i++;
        }

        _grabbable.grabPoints = newGrabPoints;
    }

    public void ResetGrabPoints()
    {
        _grabbable.grabPoints = _originalGrabPoints;
        foreach (var overrider in _grabPointsOverriders)
        {
            GameObject temp = overrider;
            GameObject.Destroy(temp);
        }

        _grabPointsOverriders.Clear();
    }

    public void AdjustGrabPoints(Transform target)
    {
        _grabPointsParent.transform.position = target.position;
        _grabPointsParent.transform.rotation = target.rotation;
    }
}
