using System.Collections.Generic;
using UnityEngine;
using XPBD_Engine.Scripts.Managers;
using XPBD_Engine.Scripts.Physics.Grabber.Interfaces;
using XPBD_Engine.Scripts.Utilities.Data_structures;

namespace XPBD_Engine.Scripts.Physics.Grabber
{
    public class GrabberSphere : IGrabber
    {
        private Vector3 _centerPos;
        private readonly float _radius;
        //The mesh we grab
        private GrabbedBodyVertex? _grabbedBodyVertex;

        //To give the mesh a velocity when we release it
        private Vector3 _lastGrabPos;
        
        public GrabberSphere(Vector3 center,float radius )
        {
            _centerPos = center;
            _radius = radius;
        }

        public void StartGrab(List<IGrabbable> bodies)
        {
            if (_grabbedBodyVertex.HasValue) return;
            
            var maxDist = float.MaxValue;
            IGrabbable closestBody = null;
            int bestVertexIndex = -1;
            
            foreach (var body in bodies)
            {
                if (!body.IsSphereInsideBody(_centerPos, _radius, out SphereHit bestVertex)) continue;
                if (!(bestVertex.distance < maxDist)) continue;
                closestBody = body;
                bestVertexIndex = bestVertex.index;
                maxDist = bestVertex.distance;
            }
            if (closestBody == null) return;
            _grabbedBodyVertex = new GrabbedBodyVertex(closestBody,bestVertexIndex);
            _grabbedBodyVertex.Value.grabbedBody.StartGrabVertex(_centerPos,bestVertexIndex);
            _lastGrabPos = _centerPos;
            if (ActivityManager.instance != null)
            {
                ActivityManager.instance.StartGrabbingVertex(bestVertexIndex);
            }

        }

        public void MoveGrab(Vector3 position)
        {
            _centerPos = position;
            if (_grabbedBodyVertex.HasValue)
            { 
                //Cache the old pos before we assign it
                _lastGrabPos = _grabbedBodyVertex.Value.grabbedBody.GetGrabbedPos(_grabbedBodyVertex.Value.indexVertex);
                //Moved the vertex to the new pos
                _grabbedBodyVertex.Value.grabbedBody.MoveGrabbed(_centerPos,_grabbedBodyVertex.Value.indexVertex);
            }
            

        }

        public void EndGrab(Vector3 position)
        {
            if (_grabbedBodyVertex.HasValue)
            {
                //Add a velocity to the ball
                var vel = (position - _lastGrabPos).magnitude / Time.deltaTime;
                var dir = (position - _lastGrabPos).normalized;

                _grabbedBodyVertex.Value.grabbedBody.EndGrab(position, dir * vel,_grabbedBodyVertex.Value.indexVertex);
                _grabbedBodyVertex = null;
            }

           
        }
    }
}