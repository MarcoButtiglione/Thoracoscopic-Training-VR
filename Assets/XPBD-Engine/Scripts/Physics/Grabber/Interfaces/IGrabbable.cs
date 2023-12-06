using UnityEngine;
using XPBD_Engine.Scripts.Utilities.Data_structures;

namespace XPBD_Engine.Scripts.Physics.Grabber.Interfaces
{
    public interface IGrabbable
    {
         int StartGrab(Vector3 grabPos);

         void StartGrabVertex(Vector3 grabPos, int vertexIndex);

         void MoveGrabbed(Vector3 grabPos,int vertexIndex);

         void EndGrab(Vector3 grabPos, Vector3 vel,int vertexIndex);

         void IsRayHittingBody(Ray ray, out PointerHit hit);

         bool IsSphereInsideBody(Vector3 center,float radius, out SphereHit bestVertex);

         Vector3 GetGrabbedPos(int vertexIndex);
    }
}
