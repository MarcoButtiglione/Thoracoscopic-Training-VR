using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XPBD_Engine.Scripts.Physics.SoftBody;

public class SphericalAnchor : MonoBehaviour
{
    public float radius = 1.0f;
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, radius);
    }
}
