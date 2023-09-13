using UnityEngine;
using System;

[Serializable]
public struct HandVector
{
    public float finger;
    public float thumb;
    public float palm;


    public Vector3 leftHand
    {
        get { return new Vector3(-thumb, palm, finger); }
    }
    public Vector3 rightHand
    {
        get { return new Vector3(-thumb, -palm, finger); }
    }

    public HandVector(float palm, float finger, float thumb)
    {
        this.palm = palm;
        this.finger = finger;
        this.thumb = thumb;
    }

    public static HandVector zero
    {
        get { return new HandVector(); }
    }
}
