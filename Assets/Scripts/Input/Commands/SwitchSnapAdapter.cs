using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchSnapAdapter : BaseCommand
{
    [SerializeField] private Grabbable _grabbable = null;

    private void Awake()
    {
        if (_grabbable == null)
        {
            throw new MissingReferenceException("Grabbbale in command " + this.name + " must be assigned.");
        }
    }

    public override void Activate()
    {
        _grabbable.SwitchSnapAdapter();
    }
}
