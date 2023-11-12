using UnityEngine;

//Standardized Tetrahedron methods
namespace XPBD_Engine.Scripts.Utilities
{
    public static class Tetrahedron
    {
        //
        // The volume of a tetrahedron
        //
        private const float OneOverSix = 1f / 6f;

        public static float Volume(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            var d0 = p1 - p0;
            var d1 = p2 - p0;
            var d2 = p3 - p0;

            var volume = Vector3.Dot(Vector3.Cross(d0, d1), d2) * OneOverSix;

            return volume;
        }
    }
}
