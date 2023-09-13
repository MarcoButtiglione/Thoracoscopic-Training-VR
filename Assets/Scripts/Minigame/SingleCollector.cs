using System;
using System.Collections;
using UnityEngine;

namespace MinigameSystem
{
    [RequireComponent(typeof(SphereCollider))]
    public class SingleCollector : Collector
    {
        private Graspable _enteredGraspable = null;
        private bool _snappedGraspable = false;
        private bool _wasGrasped = false;
        private GameObject _snapGuide = null;
        [SerializeField] private bool _enableSnapGuide = false;
        [Range(0, 1)]
        [SerializeField] private float _snapOpacity = 0.5f;
        [SerializeField] private bool _smoothSnap = false;
        [Tooltip("Duration of the smooth snap in seconds.")]
        [SerializeField] private float _smoothSnapDuration = 1.0f;
        [SerializeField] private Transform _snapOffset = null;
        [SerializeField] private Material _snapGuideMaterial = null;

        protected override void Awake()
        {
            base.Awake();

            // Add private Awake behaviour below.
            if (_snapOffset == null)
                throw new ArgumentException("Snap Offset missing from: " + this.name);
        }

        // Update is called once per frame
        private void FixedUpdate()
        {
            if (_enteredGraspable != null) // if there is an object not grasped
            {
                if (_enteredGraspable.graspedBy == null)
                {
                    if (_wasGrasped && (_snappedGraspable == false || _snappedGraspable == _enteredGraspable)) // New toy (or the already snapped) inserted
                    {
                        Debug.Log("Inserted: " + _enteredGraspable.name);
                        UpdateSolution();
                        if (_enableSnapGuide) ToggleGuide();
                    }
                    _wasGrasped = false;
                }
                else
                {
                    if (_insertedGraspables.Contains(_enteredGraspable))
                        Debug.Log("Removed: " + _enteredGraspable.name);
                    _insertedGraspables.Remove(_enteredGraspable);
                    _snappedGraspable = false;
                    if (!_wasGrasped)
                        if (_enableSnapGuide) ToggleGuide();
                    _wasGrasped = true;
                }
            }
        }

        public override void Init()
        {
            _enteredGraspable = null;
            if (_snappedGraspable == true)
            {
                var rb = _insertedGraspables[0].GetComponent<Rigidbody>();
                rb.isKinematic = false;
                _snappedGraspable = false;
            }
            _wasGrasped = false;
            _status = Status.IDLE;
            base.Init();
        }

        private void Snap(Graspable graspable)
        {
            _snappedGraspable = true;
            if (!_insertedGraspables.Contains(graspable))
            {
                _insertedGraspables.Add(graspable);
                var rb = graspable.GetComponent<Rigidbody>();
                rb.isKinematic = true;
                if (_smoothSnap)
                {
                    StartCoroutine(SmoothSnap(graspable));
                }
                else
                {
                    graspable.SetTransform(_snapOffset);
                }
            }
        }

        private IEnumerator SmoothSnap(Graspable graspable)
        {
            float timeElapsed = 0.0f;
            Vector3 startPosition = graspable.transform.position;
            Quaternion startRotation = graspable.transform.rotation;
            while (timeElapsed < _smoothSnapDuration)
            {
                float progress = timeElapsed / _smoothSnapDuration;
                graspable.transform.position = Vector3.Lerp(startPosition, _snapOffset.position, progress);
                graspable.transform.rotation = Quaternion.Lerp(startRotation, _snapOffset.rotation, progress);
                timeElapsed += Time.deltaTime;
                yield return null;  // At this point, the coroutine will pause and resume the following frame.
            }

            // Final adjustments after the lerp.
            graspable.transform.position = _snapOffset.position;
            graspable.transform.rotation = _snapOffset.rotation;
        }

        private void ToggleGuide()
        {
            if (_enteredGraspable != null && _solution.Contains(_enteredGraspable) && _snappedGraspable == false)
            {
                MeshFilter enteredMesh = _enteredGraspable.GetComponentInChildren<MeshFilter>();
                MeshRenderer enteredRend = _enteredGraspable.GetComponentInChildren<MeshRenderer>();
                Color color = enteredRend.material.color;
                color.a = _snapOpacity;

                _snapGuide = new GameObject();
                _snapGuide.AddComponent<MeshFilter>().mesh = enteredMesh.mesh;
                MeshRenderer rend = _snapGuide.AddComponent<MeshRenderer>();
                rend.material = _snapGuideMaterial;
                rend.material.SetColor("_Color", color);

                _snapGuide.transform.localScale = WorldScaler.Instance.GetScaledVector3(_enteredGraspable.transform.localScale);
                _snapGuide.transform.position = _snapOffset.position;
                _snapGuide.transform.rotation = _snapOffset.rotation;
            }
            else
            {
                GameObject.Destroy(_snapGuide);
            }
        }

        protected override void UpdateSolution()
        {
            // This case happens when the graspable is removed or falls from the collector.
            if (_enteredGraspable == null)
            {
                _status = Status.IDLE;
            }
            else if (_solution.Contains(_enteredGraspable))
            {
                Snap(_enteredGraspable);
                _enteredGraspable.RegisterCorrectPlacement();
                _audioSource.PlayOneShot(_goodClip);
                _status = Status.SOLVED;
            }
            else
            {
                _audioSource.PlayOneShot(_errorClip);
                _status = Status.WRONG;
                manager.NotifyError();
            }

            Debug.Log("Status for " + this.name + ": " + _status.ToString());
            manager.UpdateSolvedStatus();
        }

        protected override void HandleEnteredGraspable(Graspable enteredGraspable)
        {
            Debug.Log("enter called");
            if (_enteredGraspable == null && enteredGraspable.graspedBy != null) // If currently there is no enterd in collector and new entered is actively grasped
            {
                Debug.Log("Entered: " + enteredGraspable.name);
                _wasGrasped = true;
                _enteredGraspable = enteredGraspable;
                if (_enableSnapGuide) ToggleGuide();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exitedGraspable"></param>
        protected override void HandleExitedGraspable(Graspable exitedGraspable)
        {
            Debug.Log("exit called");
            if (_enteredGraspable == exitedGraspable)
            {
                SphereCollider trigger = (SphereCollider)_trigger;
                Collider[] found = Physics.OverlapSphere(transform.TransformPoint(trigger.center), WorldScaler.Instance.GetScaledValue(trigger.radius), _collectorMask);
                if (Array.Find(found, e => e.gameObject == exitedGraspable.gameObject) == null)
                {
                    Debug.Log("Exited: " + _enteredGraspable.name);
                    _wasGrasped = false;
                    _enteredGraspable = null;
                    _snappedGraspable = false;
                    if (_enableSnapGuide) ToggleGuide();
                    UpdateSolution();
                }
            }
        }

        private void OnDrawGizmos()
        {
            SphereCollider trigger;
            if (_trigger != null)
                trigger = (SphereCollider)_trigger;
            else
                trigger = GetComponent<SphereCollider>();
            Color color = Color.green;
            color.a = .3f;
            Gizmos.color = color;
            Gizmos.DrawSphere(transform.TransformPoint(trigger.center), transform.lossyScale.x * trigger.radius);
        }
    }
}
