using UnityEngine;
using System;

[Serializable]
public struct HandQuaternion
{
    public float fingers;
    public float thumb;
    public float palm;

    public Quaternion leftHand
    {
        get
        {
            return Quaternion.Euler(-thumb, -palm, -fingers);
        }
    }

    public Quaternion rightHand
    {
        get
        {
            return Quaternion.Euler(thumb, -palm, fingers);
        }
    }

    public HandQuaternion(float palm, float finger, float thumb)
    {
        this.palm = palm;
        this.fingers = finger;
        this.thumb = thumb;
    }

    public HandQuaternion(Quaternion quaterion)
    {
        Vector3 euler = quaterion.eulerAngles;
        this.palm = euler.y;
        this.fingers = euler.z;
        this.thumb = euler.x;
    }

    public static HandQuaternion zero
    {
        get { return new HandQuaternion(); }
    }
}
