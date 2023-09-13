using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(Collider))]
public class OperatingZoneLimit : MonoBehaviour
{

    // The handler this limit refere to
    private OperatingErrorsHandler _operatingErrorsHandler = null;

    private void Awake()
    {
        _operatingErrorsHandler = GetComponentInParent<OperatingErrorsHandler>();
        if (_operatingErrorsHandler == null)
            throw new NullReferenceException("Operating Limit should always be parented to an OperatingZone.");
        _operatingErrorsHandler.RegisterLimit(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        Grabbable grabbable = other.GetComponentInParent<Grabbable>();
        if (grabbable != null)
            _operatingErrorsHandler.NotifyGrabbableLimitEnter(grabbable);
    }

    private void OnTriggerExit(Collider other)
    {
        Grabbable grabbable = other.GetComponentInParent<Grabbable>();
        if (grabbable != null)
            _operatingErrorsHandler.NotifyGrabbableLimitExit(grabbable);
    }
}
