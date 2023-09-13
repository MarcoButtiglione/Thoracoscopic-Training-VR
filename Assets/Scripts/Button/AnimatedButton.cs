using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedButton : Button3D
{

    private Animator _animator = null;

    protected override void Awake()
    {
        base.Awake();
        _animator = GetComponentInChildren<Animator>();
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        if (_canBePressed)
            PressBehaviour();
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
        if (_enteredIndexesNumber == 0)
            ReleaseBehaviour();
    }

    public override void PressBehaviour()
    {
        base.PressBehaviour();
        if (_canBePressed)
        {
            _animator.SetBool("press", true);
        }
    }

    public override void ReleaseBehaviour()
    {
        base.ReleaseBehaviour();
        _animator.SetBool("press", false);
    }

}
