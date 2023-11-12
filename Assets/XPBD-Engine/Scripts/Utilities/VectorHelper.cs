using UnityEngine;

namespace XPBD_Engine.Scripts.Utilities
{
    public static class VectorHelper
    {
        public static void VecSetZero(float[] a, int anr)
        {
            anr *= 3;
            a[anr++] = 0.0f;
            a[anr++] = 0.0f;
            a[anr] = 0.0f;
        }

        public static void VecScale(float[] a, int anr, float scale)
        {
            anr *= 3;
            a[anr++] *= scale;
            a[anr++] *= scale;
            a[anr] *= scale;
        }

        public static void VecCopy(float[] a, int anr, float[] b, int bnr)
        {
            anr *= 3;
            bnr *= 3;
            a[anr++] = b[bnr++];
            a[anr++] = b[bnr++];
            a[anr] = b[bnr];
        }

        public static void VecAdd(float[] a, int anr, float[] b, int bnr, float scale = 1.0f)
        {
            anr *= 3;
            bnr *= 3;
            a[anr++] += b[bnr++] * scale;
            a[anr++] += b[bnr++] * scale;
            a[anr] += b[bnr] * scale;
        }
        public static void VecSetDiff(float[] dst, int dnr, float[] a, int anr, float[] b, int bnr, float scale = 1.0f)
        {
            dnr *= 3; anr *= 3; bnr *= 3;
            dst[dnr++] = (a[anr++] - b[bnr++]) * scale;
            dst[dnr++] = (a[anr++] - b[bnr++]) * scale;
            dst[dnr] = (a[anr] - b[bnr]) * scale;
        }

        public static float VecLengthSquared(float[] a, int anr)
        {
            anr *= 3;
            float a0 = a[anr];
            float a1 = a[anr + 1];
            float a2 = a[anr + 2];
            return a0 * a0 + a1 * a1 + a2 * a2;
        }

        public static float VecDistSquared(float[] a, int anr, float[] b, int bnr)
        {
            anr *= 3; bnr *= 3;
            float a0 = a[anr] - b[bnr];
            float a1 = a[anr + 1] - b[bnr + 1];
            float a2 = a[anr + 2] - b[bnr + 2];
            return a0 * a0 + a1 * a1 + a2 * a2;
        }

        public static float VecDot(float[] a, int anr, float[] b, int bnr)
        {
            anr *= 3; bnr *= 3;
            return a[anr] * b[bnr] + a[anr + 1] * b[bnr + 1] + a[anr + 2] * b[bnr + 2];
        }

        public static void VecSetCross(float[] a, int anr, float[] b, int bnr, float[] c, int cnr)
        {
            anr *= 3; bnr *= 3; cnr *= 3;
            a[anr++] = b[bnr + 1] * c[cnr + 2] - b[bnr + 2] * c[cnr + 1];
            a[anr++] = b[bnr + 2] * c[cnr + 0] - b[bnr + 0] * c[cnr + 2];
            a[anr] = b[bnr + 0] * c[cnr + 1] - b[bnr + 1] * c[cnr + 0];
        }

        public static float MatGetDeterminant(float[] A)
        {
            var a11 = A[0];var a12 = A[3];var a13 = A[6];
            var a21 = A[1];var a22 = A[4];var a23 = A[7];
            var a31 = A[2];var a32 = A[5];var a33 = A[8];
            return a11*a22*a33 + a12*a23*a31 + a13*a21*a32 - a13*a22*a31 - a12*a21*a33 - a11*a23*a32;
        }
        
        public static Vector3 MatSetMult(Vector3[] A,Vector3 b)
        {
            return  A[0] * b.x + A[1] * b.y + A[2] * b.z;
        }
        
        public static float[] MatInverse (float[] A) {
            var det = MatGetDeterminant(A);
            var invMat = new float[9];
            if (det == 0.0) {
                return invMat;
            }
            var invDet = 1.0f / det;
            var a11 = A[0];var a12 = A[3];var a13 = A[6];
            var a21 = A[1];var a22 = A[4];var a23 = A[7];
            var a31 = A[2];var a32 = A[5];var a33 = A[8];
            invMat[0] =  (a22 * a33 - a23 * a32) * invDet; 
            invMat[3] = -(a12 * a33 - a13 * a32) * invDet;
            invMat[6] =  (a12 * a23 - a13 * a22) * invDet;
            invMat[1] = -(a21 * a33 - a23 * a31) * invDet;
            invMat[4] =  (a11 * a33 - a13 * a31) * invDet;
            invMat[7] = -(a11 * a23 - a13 * a21) * invDet;
            invMat[2] =  (a21 * a32 - a22 * a31) * invDet;
            invMat[5] = -(a11 * a32 - a12 * a31) * invDet;
            invMat[8] =  (a11 * a22 - a12 * a21) * invDet;
            return invMat;
        }

        public static float[] FromVecToFloat(Vector3[] vect)
        {
            var floatVec = new float[9];
            floatVec[0] = vect[0].x;
            floatVec[1] = vect[0].y;
            floatVec[2] = vect[0].z;
            floatVec[3] = vect[1].x;
            floatVec[4] = vect[1].y;
            floatVec[5] = vect[1].z;
            floatVec[6] = vect[2].x;
            floatVec[7] = vect[2].y;
            floatVec[8] = vect[2].z;
            return floatVec;
        }

        public static Vector3[] FromFloatToVec(float[] floatVec)
        {
            var vect = new Vector3[3];
            vect[0].x =  floatVec[0] ;
            vect[0].y =  floatVec[1] ;
            vect[0].z =  floatVec[2] ;
            vect[1].x =  floatVec[3] ;
            vect[1].y =  floatVec[4] ;
            vect[1].z =  floatVec[5] ;
            vect[2].x =  floatVec[6] ;
            vect[2].y =  floatVec[7] ;
            vect[2].z =  floatVec[8] ;
            return vect;
        }
        

        
    }
}