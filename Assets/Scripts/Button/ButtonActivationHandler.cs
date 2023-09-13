using System;
using UnityEngine;

public class ButtonActivationHandler : MonoBehaviour
{
    [SerializeField] private Collider _buttonTrigger = null;
    [SerializeField] private PhysicalButton _button = null;

    private void Awake()
    {
        if (_buttonTrigger == null)
            throw new ArgumentException("Button Trigger must not be null.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == _buttonTrigger)
        {
            _button.PressBehaviour();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == _buttonTrigger)
        {
            _button.ReleaseBehaviour();
        }
    }
}