using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LaparoscopicZone : OperatingZone
{

    protected override void Awake()
    {
        IKPivot pivot = GetComponentInChildren<IKPivot>();
        if (pivot == null)
        {
            throw new NullReferenceException("Laparoscoping Zone " + this.name + " needs an IKPivot in its hierarchy. You can find it in prefabs folder.");
        }
        base.Awake();
    }

    public override void Insert(Grabbable grabbable)
    {
        if (_insertedGrabbables.Count == 0)
            base.Insert(grabbable);
    }

    public override void Remove(Grabbable grabbable)
    {
        base.Remove(grabbable);
    }
}
