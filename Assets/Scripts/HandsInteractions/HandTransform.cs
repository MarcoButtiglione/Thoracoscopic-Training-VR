using UnityEngine;
using System;

[Serializable]
public struct HandTransform
{
    public HandVector position;
    public HandQuaternion rotation;

    public HandTransform(HandVector position, HandQuaternion rotation)
    {
        this.position = position;
        this.rotation = rotation;
    }

    public static Vector3 palmDirection
    {
        get { return new Vector3(0, 1, 0); }
    }

    public static Vector3 fingerDirection
    {
        get { return new Vector3(0, 0, 1); }
    }

    public static Vector3 thumbDirection
    {
        get { return new Vector3(-1, 0, 0); }
    }

    public static HandTransform zero
    {
        get { return new HandTransform(HandVector.zero, HandQuaternion.zero); }
    }
}