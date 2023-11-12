using UnityEngine;
using XPBD_Engine.Scripts.Utilities.Data_structures;

namespace XPBD_Engine.Scripts.Utilities
{
    public static class Intersections
    {
        //The value we use to avoid floating point precision issues
        //http://sandervanrossen.blogspot.com/2009/12/realtime-csg-part-1.html
        //Unity has a built-in Mathf.Epsilon;
        //But it's better to use our own so we can test different values
        public const float EPSILON = 0.00001f;
        //
        // Ray-mesh intersection
        //
        //Should return location, normal, index, distance
        //We dont care about the direction the ray hits - just if the ray is hitting a triangle from either side
        public static bool IsRayHittingMesh(Ray ray, Vector3[] vertices,float[] invMass, int[] triangles, out PointerHit bestHit)
        {
            bestHit = null;

            var smallestDistance = float.MaxValue;

            //Loop through all triangles and find the one thats the closest
            for (int i = 0; i < triangles.Length; i += 3)
            {
                if (invMass[triangles[i + 0]]==0 ||invMass[triangles[i + 1]]==0 ||invMass[triangles[i + 2]]==0 )
                {
                    continue;
                }
                var a = vertices[triangles[i + 0]];
                var b = vertices[triangles[i + 1]];
                var c = vertices[triangles[i + 2]];

                if (IsRayHittingTriangle(a, b, c, ray, out PointerHit hit))
                {
                    if (hit.distance < smallestDistance)
                    {
                        smallestDistance = hit.distance;

                        bestHit = hit;

                        bestHit.index = i;
                    }
                }
            }

            //If at least a triangle was hit
            bool hitMesh = bestHit != null;

            return hitMesh;
        }
        //
        // Ray-plane intersection
        //
        //https://www.scratchapixel.com/lessons/3d-basic-rendering/ray-tracing-rendering-a-triangle/ray-triangle-intersection-geometric-solution
        public static bool IsRayHittingPlane(Ray ray, Vector3 planeNormal, Vector3 planePos, out float t)
        {
            //Add default because have to
            t = 0f;

            //First check if the plane and the ray are perpendicular
            float NdotRayDirection = Vector3.Dot(planeNormal, ray.direction);

            //If the dot product is almost 0 then ray is perpendicular to the triangle, so no itersection is possible
            if (Mathf.Abs(NdotRayDirection) < EPSILON)
            {
                return false;
            }

            //Compute d parameter using equation 2 by picking any point on the plane
            float d = -Vector3.Dot(planeNormal, planePos);

            //Compute t (equation 3)
            t = -(Vector3.Dot(planeNormal, ray.origin) + d) / NdotRayDirection;

            //Check if the plane is behind the ray
            if (t < 0)
            {
                return false;
            }

            return true;
        }
        //
        // Ray-triangle intersection
        //
        //First do ray-plane itersection and then check if the itersection point is within the triangle
        //https://www.scratchapixel.com/lessons/3d-basic-rendering/ray-tracing-rendering-a-triangle/ray-triangle-intersection-geometric-solution
        public static bool IsRayHittingTriangle(Vector3 v0, Vector3 v1, Vector3 v2, Ray ray, out PointerHit hit)
        {
            hit = null;

            //Compute plane's normal
            Vector3 v0v1 = v1 - v0;
            Vector3 v0v2 = v2 - v0;

            //No need to normalize
            Vector3 planeNormal = Vector3.Cross(v0v1, v0v2);


            //
            // Step 1: Finding P (the intersection point) by turning the triangle into a plane
            //

            if (!IsRayHittingPlane(ray, planeNormal, v0, out float t))
            {
                return false;
            }

            //Compute the intersection point using equation 1
            Vector3 P = ray.origin + t * ray.direction;


            //
            // Step 2: inside-outside test
            //

            //Vector perpendicular to triangle's plane
            Vector3 C;

            //Edge 0
            Vector3 edge0 = v1 - v0;
            Vector3 vp0 = P - v0;

            C = Vector3.Cross(edge0, vp0);

            //P is on the right side 
            if (Vector3.Dot(planeNormal, C) < 0f)
            {
                return false;
            }

            //Edge 1
            Vector3 edge1 = v2 - v1;
            Vector3 vp1 = P - v1;

            C = Vector3.Cross(edge1, vp1);

            //P is on the right side 
            if (Vector3.Dot(planeNormal, C) < 0f)
            {
                return false;
            }

            //Edge 2
            Vector3 edge2 = v0 - v2;
            Vector3 vp2 = P - v2;

            C = Vector3.Cross(edge2, vp2);

            //P is on the right side 
            if (Vector3.Dot(planeNormal, C) < 0f)
            {
                return false;
            }

            //This ray hits the triangle

            //Calculate the custom data we need
            hit = new PointerHit(t, P, planeNormal.normalized);

            return true;
        }
        
        public static bool IsVertexIntoSphere(Vector3 vert, Vector3 centerSphere, float radiusSphere,out float distanceFromCenter)
        {
            distanceFromCenter = Vector3.Distance(centerSphere, vert);
            return distanceFromCenter <= radiusSphere;
        }
    }
}
