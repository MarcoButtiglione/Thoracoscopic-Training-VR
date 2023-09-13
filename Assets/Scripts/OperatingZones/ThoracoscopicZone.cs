using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class ThoracoscopicZone : OperatingZone
{
    protected override void Awake()
    {
        base.Awake();
        Collider trigger = GetComponent<Collider>();
        if (!trigger.isTrigger)
        {
            trigger.isTrigger = true;
            Debug.LogWarning("Collider in ThoracoscopicZone " + gameObject.name + " was not trigger. Forced to trigger.");
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        Grabbable grabbable = other.GetComponentInParent<Grabbable>();
        if (grabbable != null)
        {
            Insert(grabbable);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Grabbable grabbable = other.GetComponentInParent<Grabbable>();
        if (grabbable != null)
        {
            Remove(grabbable);
        }
    }
}
