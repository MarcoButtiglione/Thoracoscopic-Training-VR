using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonCollisionHandler : MonoBehaviour
{

    private Collider _buttonCollider;
    private Rigidbody _rb;

    private void Awake()
    {
        _buttonCollider = GetComponent<Collider>();
        _rb = GetComponent<Rigidbody>();
    }

    // AVOID COLLISIONS FROM BOTTOM SIDE
    private void OnCollisionEnter(Collision other)
    {
        bool existAbove = false;
        for (int i = 0; i < other.contactCount; i++)
        {
            Vector3 localContact = transform.InverseTransformPoint(other.GetContact(i).point);
            if (localContact.y > 0)
            {
                existAbove = true;
            }
        }

        // NOT WORKING
        if (!existAbove)
        {
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }
    }

}
