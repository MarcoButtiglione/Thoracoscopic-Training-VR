using System;
using UnityEngine;

public class WorldScaler : Singleton<WorldScaler>
{
    private float _worldScale;

    private void Awake()
    {
        if (transform.localScale.x != transform.localScale.y || transform.localScale.x != transform.localScale.z || transform.localScale.y != transform.localScale.z)
            throw new ArgumentException("The GameObject containing WorldScaler (" + gameObject.name + ") must have a uniform scaling (same scale value in x, y and z).");

        _worldScale = transform.localScale.x;
        Physics.gravity = GetScaledVector3(Physics.gravity);
    }

    public float GetScaledValue(float value)
    {
        return value * _worldScale;
    }

    public Vector3 GetScaledVector3(Vector3 vector)
    {
        return vector * _worldScale;
    }
}
