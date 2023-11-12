using UnityEngine;

namespace XPBD_Engine.Scripts.Utilities.Data_structures
{
    public class SphereHit
    {
        //Distance from the vertex and the center of the sphere
        public float distance;
        //Vertex inside the sphere
        public Vector3 vertex;
        //The index of the vertex inside the sphere
        public int index;

        public SphereHit(float distance, Vector3 vertex, int index)
        {
            this.distance = distance;
            this.vertex = vertex;
            this.index = index;
        }
        
    }
}
