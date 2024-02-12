using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XPBD_Engine.Scripts.Managers;
using XPBD_Engine.Scripts.Physics.Grabber.Interfaces;

namespace XPBD_Engine.Scripts.Physics.Grabber
{
    public class GrabberSphereController : MonoBehaviour
    {
        public float colliderRadius;
        private GrabberSphere _grabber;
        [SerializeField] private Transform _grabbingAnchorTransform;
        
        private void Start()
        {
            _grabber = new GrabberSphere(transform.position,colliderRadius);
        }

        private void FixedUpdate()
        {
            _grabber.MoveGrab(_grabbingAnchorTransform.position);
        }

        public void StartGrabbing()
        {
            if (PhysicalWorld.instance == null)
            {
                Debug.LogError("There is no PhysicalWorld instance in the scene.");
                return;
            }
            var temp = new List<IGrabbable>(PhysicalWorld.instance.GetSoftBodies().ToList());
            _grabber.StartGrab(temp,transform.position);
        }

        public void EndGrabbing()
        {
            _grabber.EndGrab(_grabbingAnchorTransform.position);
        }
        private void OnDrawGizmos()
        {
            if (_grabbingAnchorTransform != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(_grabbingAnchorTransform.position, colliderRadius/10);
            }
            var color = Color.yellow;
            color.a = 0.5f;
            Gizmos.color = color;
            Gizmos.DrawSphere(transform.position, colliderRadius);
        }
    }
    
}
