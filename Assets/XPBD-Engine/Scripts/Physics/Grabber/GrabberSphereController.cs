using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XPBD_Engine.Scripts.Physics.Grabber.Interfaces;

namespace XPBD_Engine.Scripts.Physics.Grabber
{
    public class GrabberSphereController : MonoBehaviour
    {
        private PhysicalWorld _physicalWorld;
        public float colliderRadius;
        private GrabberSphere _grabber;
        
        private void Start()
        {
            _grabber = new GrabberSphere(transform.position,colliderRadius);
            if (FindObjectsOfType<PhysicalWorld>().Length>0)
            {
                _physicalWorld = FindObjectsOfType<PhysicalWorld>()[0];
            }
            else
            {
                Debug.LogError("You must include at least one Physical World component in the scene");
            }
        }

        private void Update()
        {
            _grabber.MoveGrab(transform.position);
        }

        public void StartGrabbing()
        {
            var temp = new List<IGrabbable>(_physicalWorld.GetSoftBodies().ToList());
            _grabber.StartGrab(temp);
        }

        public void EndGrabbing()
        {
            _grabber.EndGrab(transform.position);
        }
    }
}
