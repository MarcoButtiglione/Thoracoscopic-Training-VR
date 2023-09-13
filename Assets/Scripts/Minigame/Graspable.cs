using System;
using System.Collections.Generic;
using UnityEngine;

namespace MinigameSystem
{
    [RequireComponent(typeof(Rigidbody))]
    public class Graspable : MonoBehaviour
    {
        private const string NOT_AVAILABLE_YET = "NOT_AVAILABLE_YET";
        private Rigidbody _rb = null;
        private KinematicGraspZone _graspedBy = null;

        /// <value>
        /// List of logs for the <see cref="Graspable"/>. Each tuple contains the timestamp in which it was grasped and the one it was released.
        /// </value>
        private List<GraspableTimestamps> _graspableLogs = new List<GraspableTimestamps>();
        private List<string> _correctPlacementTimestamp = new List<string>();
        [SerializeField] private GraspableColor _color = GraspableColor.ALL;
        [SerializeField] private GraspableShape _shape = GraspableShape.ALL;

        public KinematicGraspZone graspedBy
        {
            get { return _graspedBy; }
            // set { _graspedBy = value; }
        }

        public List<GraspableTimestamps> graspableTimestampsLogs
        {
            get { return _graspableLogs; }
        }

        public List<string> graspableCorrectPlacementTimestampLogs
        {
            get { return _correctPlacementTimestamp; }
        }

        public GraspableColor color
        {
            get { return _color; }
        }

        public GraspableShape shape
        {
            get { return _shape; }
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        public void SetTransform(Transform t)
        {
            transform.position = t.position;
            transform.rotation = t.rotation;
        }

        public void GetGrasped(KinematicGraspZone graspZone, Transform graspZoneTransform)
        {
            _graspedBy = graspZone;
            _rb.isKinematic = true;
            _rb.useGravity = false;
            transform.SetParent(graspZoneTransform);
            _graspableLogs.Add(new GraspableTimestamps(DateTime.Now.ToString(ExerciseLogger.Instance.cultureInfo), NOT_AVAILABLE_YET));
        }

        public void GetReleased()
        {
            _rb.isKinematic = false;
            _rb.useGravity = true;
            _graspedBy = null;
            transform.SetParent(null);
            if (_graspableLogs.Count > 0)
            {
                GraspableTimestamps lastTimestampEntry = _graspableLogs[_graspableLogs.Count - 1];
                if (lastTimestampEntry.graspEndedTimestamp == NOT_AVAILABLE_YET)
                {
                    lastTimestampEntry.graspEndedTimestamp = DateTime.Now.ToString(ExerciseLogger.Instance.cultureInfo);
                }
            }
        }

        public void RegisterCorrectPlacement()
        {
            _correctPlacementTimestamp.Add(DateTime.Now.ToString(ExerciseLogger.Instance.cultureInfo));
        }
    }

    public class GraspableTimestamps
    {
        public string graspStartedTimestamp;
        public string graspEndedTimestamp;

        public GraspableTimestamps(string start, string end)
        {
            graspStartedTimestamp = start;
            graspEndedTimestamp = end;
        }
    }

    public enum GraspableColor { ORANGE = 0, GREEN = 1, BLUE = 2, PURPLE = 3, ALL = 4 }
    public enum GraspableShape { CUBE = 0, SPHERE = 1, PARALLELEPIPED = 2, PYRAMID = 3, ALL = 4 }
}
