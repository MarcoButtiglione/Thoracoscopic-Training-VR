using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace KinematicMechanism
{
    [DisallowMultipleComponent]
    public class KinematicConstraints : MonoBehaviour
    {
        public bool xTranslationLocked = true;
        public bool xTranslationClamped = true;
        public bool xTranslationInverted = false;
        public float xTranslationUpper = 1;
        public float xTranslationLower = -1;

        public bool yTranslationLocked = true;
        public bool ytranslationClamped = true;
        public bool yTranslationInverted = false;
        public float yTranslationUpper = 1;
        public float yTranslationLower = -1;

        public bool zTranslationLocked = true;
        public bool zTranslationClamped = true;
        public bool zTranslationInverted = false;
        public float zTranslationUpper = 1;
        public float zTranslationLower = -1;

        public bool xRotationLocked = true;
        public bool xRotationClamped = true;
        public bool xRotationInverted = false;
        public float xRotationUpper = 10;
        public float xRotationLower = -10;

        public bool yRotationLocked = true;
        public bool yRotationClamped = true;
        public bool yRotationInverted = false;
        public float yRotationUpper = 10;
        public float yRotationLower = -10;

        public bool zRotationLocked = true;
        public bool zRotationClamped = true;
        public bool zRotationInverted = false;
        public float zRotationUpper = 10;
        public float zRotationLower = -10;


        [SerializeField] private Vector3 _initPos;
        private Quaternion _initRot;

        [SerializeField] private Vector3 _forward;
        [SerializeField] private Vector3 _up;
        [SerializeField] private Vector3 _right;

        private Vector3 _translations;
        private Vector3 _rotations;

        public Vector3 relativeTranslations
        {
            get
            {
                return new Vector3(
                    ToRelative(_translations.x, xTranslationLower, xTranslationUpper, xTranslationInverted),
                    ToRelative(_translations.y, yTranslationLower, yTranslationUpper, yTranslationInverted),
                    ToRelative(_translations.z, zTranslationLower, zTranslationUpper, zTranslationInverted)
                );
            }
        }
        public Vector3 relativeRotations
        {
            get
            {
                return new Vector3(
                    ToRelative(_rotations.x, xRotationLower, xRotationUpper, xRotationInverted),
                    ToRelative(_rotations.y, yRotationLower, yRotationUpper, yRotationInverted),
                    ToRelative(_rotations.z, zRotationLower, zRotationUpper, zRotationInverted)
                );
            }
        }

        private void Awake()
        {
            _initPos = transform.position;
            // _initRot = transform.rotation;
            _initRot = transform.localRotation;

            _forward = transform.forward;
            _up = transform.up;
            _right = transform.right;
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        private void FixedUpdate()
        {
            // Translate(0.05f, 0.05f, 0.05f);
            // Rotate(0.2f, 0.2f, 0.2f);
        }

        //Translate the object in the allowed clamped directions with reference to its initial orientation
        public void Translate(float x, float y, float z)
        {
            SetTranslation(_translations.x + x, _translations.y + y, _translations.z + z);
        }

        public void SetTranslation(float x, float y, float z)
        {

            if (!xTranslationLocked)
                if (xTranslationClamped)
                    _translations.x = Mathf.Clamp(x, xTranslationLower, xTranslationUpper);
                else
                    _translations.x = x;

            if (!yTranslationLocked)
                if (ytranslationClamped)
                    _translations.y = Mathf.Clamp(y, yTranslationLower, yTranslationUpper);
                else
                    _translations.y = y;

            if (!zTranslationLocked)
                if (zTranslationClamped)
                    _translations.z = Mathf.Clamp(z, zTranslationLower, zTranslationUpper);
                else
                    _translations.z = z;

            //recalculate object position from its starting point
            transform.position = _initPos;
            transform.position += _right.normalized * _translations.x;
            transform.position += _up.normalized * _translations.y;
            transform.position += _forward.normalized * _translations.z;
        }

        public void SetTranslationX(float x)
        {
            SetTranslation(x, _translations.y, _translations.z);
        }

        public void SetTranslationY(float y)
        {
            SetTranslation(_translations.x, y, _translations.z);
        }

        public void SetTranslationZ(float z)
        {
            SetTranslation(_translations.x, _translations.y, z);
        }

        public void TranslateRelative(float x, float y, float z)
        {
            var rx = FromRelativeIncrement(x, xTranslationLower, xTranslationUpper, xTranslationInverted);
            var ry = FromRelativeIncrement(y, yTranslationLower, yTranslationUpper, yTranslationInverted);
            var rz = FromRelativeIncrement(z, zTranslationLower, zTranslationUpper, zTranslationInverted);

            Translate(rx, ry, rz);
        }

        public void SetRelativeTranslationX(float x)
        {
            float rx;
            if (xTranslationClamped)
                rx = FromRelative(x, xTranslationLower, xTranslationUpper, xTranslationInverted);
            else
                throw new Exception("Relative Movement not allowed to unclamped translations.");

            SetTranslationX(rx);
        }

        public void SetRelativeTranslationY(float y)
        {
            float ry;
            if (ytranslationClamped)
                ry = FromRelative(y, yTranslationLower, yTranslationUpper, yTranslationInverted);
            else
                throw new Exception("Relative Movement not allowed to unclamped translations.");

            SetTranslationY(ry);
        }
        public void SetRelativeTranslationZ(float z)
        {
            float rz;
            if (zTranslationClamped)
                rz = FromRelative(z, zTranslationLower, zTranslationUpper, zTranslationInverted);
            else
                throw new Exception("Relative Movement not allowed to unclamped translations.");

            SetTranslationZ(rz);
        }

        //Rotate the object by the allowd (clamped) angles with reference to its initial orientation
        public void Rotate(float x, float y, float z)
        {
            SetRotation(_rotations.x + x, _rotations.y + y, _rotations.z + z);
        }

        public void SetRotation(float x, float y, float z)
        {
            if (!xRotationLocked)
                if (xRotationClamped)
                    _rotations.x = Mathf.Clamp(x, xRotationLower, xRotationUpper);
                else
                {
                    if (x >= 360) _rotations.x = x - 360;
                    else _rotations.x = x;
                }

            if (!yRotationLocked)
                if (yRotationClamped)
                    _rotations.y = Mathf.Clamp(y, yRotationLower, yRotationUpper);
                else
                {
                    if (y >= 360) _rotations.y = y - 360;
                    else _rotations.y = y;
                }

            if (!zRotationLocked)
                if (zRotationClamped)
                    _rotations.z = Mathf.Clamp(z, zRotationLower, zRotationUpper);
                else
                {
                    if (z >= 360) _rotations.z = z - 360;
                    else _rotations.z = z;
                }


            transform.localRotation = _initRot * Quaternion.Euler(-_rotations.x, _rotations.y, -_rotations.z);

        }

        public void SetRotationX(float x)
        {
            SetRotation(x, _rotations.y, _rotations.z);
        }

        public void SetRotationY(float y)
        {
            SetRotation(_rotations.x, y, _rotations.z);
        }

        public void SetRotationZ(float z)
        {
            SetRotation(_rotations.x, _rotations.y, z);
        }

        public void RotateRelative(float x, float y, float z)
        {
            float rx;
            float rz;
            float ry;

            if (xRotationClamped)
                rx = FromRelativeIncrement(x, xRotationLower, xRotationUpper, xRotationInverted);
            else
                rx = FromRelativeIncrement(x, 0, 360, xRotationInverted);

            if (yRotationClamped)
                ry = FromRelativeIncrement(y, yRotationLower, yRotationUpper, yRotationInverted);
            else
                ry = FromRelativeIncrement(y, 0, 360, yRotationInverted);

            if (zRotationClamped)
                rz = FromRelativeIncrement(z, zRotationLower, zRotationUpper, zRotationInverted);
            else
                rz = FromRelativeIncrement(z, 0, 360, zRotationInverted);

            Rotate(rx, ry, rz);
        }

        public void SetRelativeRotationX(float x)
        {
            float rx;
            if (xRotationClamped)
                rx = FromRelative(x, xRotationLower, xRotationUpper, xRotationInverted);
            else
                rx = FromRelative(x, 0, 360, xRotationInverted);

            SetRotationX(rx);
        }

        public void SetRelativeRotationY(float y)
        {
            float ry;
            if (yRotationClamped)
                ry = FromRelative(y, yRotationLower, yRotationUpper, yRotationInverted);
            else
                ry = FromRelative(y, 0, 360, yRotationInverted);

            SetRotationY(ry);
        }

        public void SetRelativeRotationZ(float z)
        {
            float rz;
            if (zRotationClamped)
                rz = FromRelative(z, zRotationLower, zRotationUpper, zRotationInverted);
            else
                rz = FromRelative(z, 0, 360, zRotationInverted);

            SetRotationZ(rz);
        }

        private float ToRelative(float value, float min, float max, bool inverted)
        {
            float relative = (value - min) / (max - min);
            if (inverted)
                return 1 - relative;

            return relative;
        }

        private float FromRelative(float relative, float min, float max, bool inverted)
        {
            if (inverted)
                relative = 1 - relative;

            return min + relative * (max - min);
        }

        private float FromRelativeIncrement(float relativeIncrement, float min, float max, bool inverted)
        {
            if (inverted)
                relativeIncrement = -relativeIncrement;
            return relativeIncrement * (max - min);
        }

        // private void OnDrawGizmos()
        // {
        //     Gizmos.color = Color.black;
        //     Gizmos.DrawLine(transform.position, transform.position + .5f * (Quaternion.AngleAxis(-_rotations.y, _up.normalized) * transform.right));
        // }

    }
}
