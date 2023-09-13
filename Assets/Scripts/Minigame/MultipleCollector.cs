using UnityEngine;
using System.Linq;
using System;

namespace MinigameSystem
{
    [RequireComponent(typeof(BoxCollider))]
    public class MultipleCollector : Collector
    {
        [SerializeField] private GameObject _lightBulb = null;
        [SerializeField] private Material _lightBulbIdle = null;
        [SerializeField] private Material _lightBulbeWrong = null;
        [SerializeField] private Material _lightBulbSolved = null;
        private Material[] _lightBulbMaterials = null;

        protected override void Awake()
        {
            base.Awake();

            if (_lightBulb == null || _lightBulbIdle == null || _lightBulbeWrong == null || _lightBulbSolved == null)
                throw new ArgumentException("Some lightBulb references are missing in collector: " + this.name);

            _lightBulbMaterials = new Material[3];
            _lightBulbMaterials[(int)Status.IDLE] = _lightBulbIdle;
            _lightBulbMaterials[(int)Status.WRONG] = _lightBulbeWrong;
            _lightBulbMaterials[(int)Status.SOLVED] = _lightBulbSolved;
        }

        protected override void Start()
        {
            base.Start();
            // Add private Start behaviour below.
        }

        public override void Init()
        {
            _status = Status.IDLE;
            _lightBulb.GetComponent<Renderer>().material = _lightBulbMaterials[(int)_status];
            base.Init();
        }

        private void CheckValidity(Graspable graspable)
        {
            if (_solution.Contains(graspable))
            {
                graspable.RegisterCorrectPlacement();
                _audioSource.PlayOneShot(_goodClip);
            }
            else
            {
                _manager.NotifyError();
                _audioSource.PlayOneShot(_errorClip);
            }
        }

        protected override void UpdateSolution()
        {
            var beforeStatus = _status;
            _insertedGraspables.Sort((e1, e2) => e1.gameObject.name.CompareTo(e2.gameObject.name));
            if (_insertedGraspables.SequenceEqual(_solution))
                _status = Status.SOLVED;
            else
            {
                bool wrong = false;
                foreach (var graspable in _insertedGraspables)
                {
                    if (!_solution.Contains(graspable))
                    {
                        wrong = true;
                        break;
                    }
                }

                if (wrong)
                    _status = Status.WRONG;
                else
                    _status = Status.IDLE;
            }

            if (_status != beforeStatus)
            {
                _lightBulb.GetComponent<Renderer>().material = _lightBulbMaterials[(int)_status];
                if (beforeStatus == Status.WRONG)
                    _audioSource.PlayOneShot(_goodClip);
                manager.UpdateSolvedStatus();
            }
        }

        protected override void HandleEnteredGraspable(Graspable enteredGraspable)
        {
            if (!_insertedGraspables.Contains(enteredGraspable))
            {
                _insertedGraspables.Add(enteredGraspable);
                CheckValidity(enteredGraspable);
                UpdateSolution();
            }
        }

        protected override void HandleExitedGraspable(Graspable exitedGraspable)
        {
            BoxCollider trigger = (BoxCollider)_trigger;
            Collider[] found = Physics.OverlapBox(transform.TransformPoint(trigger.center), trigger.size / 2, transform.rotation, _collectorMask);
            if (Array.Find(found, e => e.gameObject == exitedGraspable.gameObject) == null)
            {
                _insertedGraspables.Remove(exitedGraspable);
                UpdateSolution();
            }
        }
    }
}


