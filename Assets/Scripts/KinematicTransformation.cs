using System;
using UnityEngine;

namespace KinematicMechanism.Utils
{
    public enum Transformation { Translation, Rotation }

    public enum Axis { X, Y, Z }


    [Serializable]
    public struct KinematicTransformation
    {
        public KinematicConstraints bone;
        public Transformation transformation;
        public Axis axis;
    }
}



