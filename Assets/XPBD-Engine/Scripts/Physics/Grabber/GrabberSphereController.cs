using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XPBD_Engine.Scripts.Physics.Grabber.Interfaces;

namespace XPBD_Engine.Scripts.Physics.Grabber
{
    public class GrabberSphereController : MonoBehaviour
    {
        public float colliderRadius;
        private GrabberSphere _grabber;
        
        private void Start()
        {
            _grabber = new GrabberSphere(transform.position,colliderRadius);
        }

        private void Update()
        {
            _grabber.MoveGrab(transform.position);
        }

        public void StartGrabbing()
        {
            if (PhysicalWorld.instance == null)
            {
                Debug.LogError("There is no PhysicalWorld instance in the scene.");
                return;
            }
            var temp = new List<IGrabbable>(PhysicalWorld.instance.GetSoftBodies().ToList());
            _grabber.StartGrab(temp);
        }

        public void EndGrabbing()
        {
            _grabber.EndGrab(transform.position);
        }
    }
}
