using System.Collections.Generic;
using UnityEngine;
using System;

namespace MinigameSystem
{
    [RequireComponent(typeof(Collider))]
    public abstract class Collector : MonoBehaviour
    {
        [SerializeField] protected GraspableColor _color = GraspableColor.ALL;
        [SerializeField] protected GraspableShape _shape = GraspableShape.ALL;
        [SerializeField] protected LayerMask _collectorMask;
        [SerializeField] protected AudioClip _errorClip = null;
        [SerializeField] protected AudioClip _goodClip = null;
        protected Collider _trigger = null;
        protected AudioSource _audioSource = null;
        protected List<Graspable> _insertedGraspables = null;
        protected List<Graspable> _solution = null;
        protected Status _status = Status.IDLE;
        protected MinigameManager _manager;
        public MinigameManager manager
        {
            get { return _manager; }
            set
            {
                if (_manager == null) _manager = value;
            }
        }
        public bool solved
        {
            get
            {
                if (_status == Status.SOLVED)
                    return true;
                return false;
            }
        }

        public List<Graspable> insertedGraspables
        {
            get { return _insertedGraspables; }
        }

        protected virtual void Awake()
        {
            if (_errorClip == null || _goodClip == null)
                throw new ArgumentException("Audio clip missing from: " + this.name);

            _audioSource = GetComponentInParent<AudioSource>();
            if (_audioSource == null)
                throw new ArgumentException("Audio Source missing from: " + this.name);

            _trigger = GetComponent<Collider>();
            if (!_trigger.isTrigger)
            {
                _trigger.isTrigger = true;
                Debug.LogWarning("Collider in the minigame collector " + gameObject.name + " was not trigger. Forced to trigger.");
            }

            _insertedGraspables = new List<Graspable>();
        }

        protected virtual void Start()
        {
            Init();
        }

        private void OnTriggerEnter(Collider other)
        {
            Graspable graspable = other.GetComponent<Graspable>();
            if (graspable != null) HandleEnteredGraspable(graspable);
        }

        private void OnTriggerExit(Collider other)
        {
            Graspable graspable = other.GetComponent<Graspable>();
            if (graspable != null) HandleExitedGraspable(graspable);
        }

        public virtual void Init()
        {
            _insertedGraspables = new List<Graspable>();
            _solution = _manager.GetSolution(_color, _shape);
            UpdateSolution();
        }

        protected abstract void UpdateSolution();
        protected abstract void HandleEnteredGraspable(Graspable enteredGraspable);
        protected abstract void HandleExitedGraspable(Graspable exitedGraspable);
    }

    public enum Status { IDLE = 0, WRONG = 1, SOLVED = 2 }
}