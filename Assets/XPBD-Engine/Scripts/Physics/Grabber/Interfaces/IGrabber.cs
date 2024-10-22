using System.Collections.Generic;
using UnityEngine;

namespace XPBD_Engine.Scripts.Physics.Grabber.Interfaces
{
    public interface IGrabber
    {
         void StartGrab(List<IGrabbable> bodies,Vector3 colliderPosition);

         void MoveGrab(Vector3 position);

         void EndGrab(Vector3 position);

    }
}