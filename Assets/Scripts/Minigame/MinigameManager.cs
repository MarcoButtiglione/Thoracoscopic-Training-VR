using System.IO;
using System.Globalization;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


namespace MinigameSystem
{
    public class MinigameManager : MonoBehaviour
    {
        [SerializeField] private string _minigameName = "Default Exercise";
        [SerializeField] private Collector[] _collectors = null;
        [SerializeField] private List<Graspable> _graspables = new List<Graspable>();

        [SerializeField] private OperatingBox _operatingBox = null;
        [SerializeField] private GameObject _degreePrefab = null;
        [SerializeField] private Transform _degreeDefaultPosition = null;
        [SerializeField] private AudioClip _solvedClip = null;
        private Tuple<Vector3, Quaternion>[] _graspablesInit = null;
        private AudioSource _audioSource = null;

        private GameObject _degreeGO = null;
        private Degree _degree = null;
        private bool _solved = false;
        private bool _isGameStarted = false;
        private bool _isGameFinished = false;
        private int _errors;

        private Timer _timer;

        public bool solved
        {
            get { return _solved; }
        }

        public int errors
        {
            get { return _errors; }
        }

        private void Awake()
        {
            _timer = new Timer(this);
            _errors = 0;

            _graspables.Sort((e1, e2) => e1.gameObject.name.CompareTo(e2.gameObject.name));
            _graspablesInit = new Tuple<Vector3, Quaternion>[_graspables.Count()];
            for (int i = 0; i < _graspables.Count; i++)
            {
                _graspables[i].gameObject.SetActive(true);
                _graspablesInit[i] = new Tuple<Vector3, Quaternion>(_graspables[i].transform.position, _graspables[i].transform.rotation);
                _graspables[i].gameObject.SetActive(false);
            }

            foreach (var collector in _collectors)
            {
                collector.gameObject.SetActive(true);
                collector.manager = this;
                collector.gameObject.SetActive(false);
            }

            if (_degreeDefaultPosition == null)
                throw new ArgumentException("Degree Default Position should not be null in " + this.name);

            if (_degreePrefab != null)
            {
                _degreeGO = GameObject.Instantiate(_degreePrefab, transform);
                _degree = _degreeGO.GetComponent<Degree>();
                if (_degree != null)
                    _degree.SetUp();
                _degreeGO.SetActive(false);
            }

            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
                throw new ArgumentException("Audio source should not be null in " + this.name);
        }

        public List<Graspable> GetSolution(GraspableColor color, GraspableShape shape)
        {
            if (color == GraspableColor.ALL && shape == GraspableShape.ALL)
                return _graspables;

            List<Graspable> list = new List<Graspable>();
            if (color == GraspableColor.ALL)
            {
                list = _graspables.FindAll(elem => (elem.shape == shape));
                list.Sort((e1, e2) => e1.gameObject.name.CompareTo(e2.gameObject.name));
                return list;
            }
            if (shape == GraspableShape.ALL)
            {
                list = _graspables.FindAll(elem => (elem.color == color));
                list.Sort((e1, e2) => e1.gameObject.name.CompareTo(e2.gameObject.name));
                return list;
            }

            list = _graspables.FindAll(elem => (elem.color == color && elem.shape == shape));
            list.Sort((e1, e2) => e1.gameObject.name.CompareTo(e2.gameObject.name));
            return list;
        }

        public void UpdateSolvedStatus()
        {
            bool allSolved = true;
            foreach (var collector in _collectors)
            {
                if (!collector.solved)
                {
                    allSolved = false;
                    break;
                }
            }
            _solved = allSolved;
            if (_isGameStarted && _solved && !_isGameFinished)
            {
                _isGameStarted = false;
                _isGameFinished = true;
                _timer.Stop();
                _degreeGO.SetActive(true);
                _degree.ResetState();

                Tuple<string, float> mostUsedOperatingZone = new Tuple<string, float>("None", 0.0f);
                Tuple<string, int> mostErrorZone = new Tuple<string, int>("None", 0);

                string endSessionTime = DateTime.Now.ToString(ExerciseLogger.Instance.cultureInfo);
                _degree.sessionId = _minigameName + " " + endSessionTime;
                _degree.timeSpent = Timer.Format(_timer.time);
                _degree.minigameErrors = _errors.ToString();
                if (_operatingBox != null)
                {
                    mostUsedOperatingZone = _operatingBox.GetMostUsedOperatingZone();
                    mostErrorZone = _operatingBox.GetOperatingZoneWithMostErrors();

                    _degree.operatingErrors = _operatingBox.GetErrors().ToString();
                    _degree.operatingErrorTime = Timer.Format(_operatingBox.GetErrorTime());
                    _degree.mostUsedOperatingZone = mostUsedOperatingZone.Item1 + " for " + Timer.Format(mostUsedOperatingZone.Item2);
                    _degree.mostErrorZone = mostErrorZone.Item1 + " with " + mostErrorZone.Item2 + " errors";
                }

                _degreeGO.transform.position = _degreeDefaultPosition.position;
                _degreeGO.transform.rotation = _degreeDefaultPosition.rotation;
                _audioSource.PlayOneShot(_solvedClip);
                StoreSessionData("Yes", endSessionTime);
            }
        }

        public void ResetUnsolvedGraspablesPosition()
        {
            Debug.Log("Reset---");
            if (_isGameStarted && !_isGameFinished)
            {
                var list = new List<Graspable>(_graspables);
                foreach (var collector in _collectors)
                {
                    list.RemoveAll(e => collector.insertedGraspables.Contains(e));
                }

                for (int i = 0; i < _graspables.Count; i++)
                {
                    if (list.Contains(_graspables[i]))
                    {
                        _graspables[i].transform.position = _graspablesInit[i].Item1;
                        _graspables[i].transform.rotation = _graspablesInit[i].Item2;
                    }
                }
            }
        }

        public void NotifyError(int n = 1)
        {
            _errors += n;
        }

        public void Init()
        {
            Debug.Log("___INIT___");
            if (_isGameStarted && !_isGameFinished)
            {
                string endSessionTime = DateTime.Now.ToString(ExerciseLogger.Instance.cultureInfo);
                StoreSessionData("No", endSessionTime);
            }

            for (int i = 0; i < _graspables.Count; i++)
            {
                _graspables[i].gameObject.SetActive(true);
                _graspables[i].transform.position = _graspablesInit[i].Item1;
                _graspables[i].transform.rotation = _graspablesInit[i].Item2;
            }
            foreach (var collector in _collectors)
            {
                collector.gameObject.SetActive(true);
                collector.Init();
            }

            _timer.Start();
            _errors = 0;
            if (_operatingBox != null)
                _operatingBox.ResetBox();

            _degreeGO.SetActive(false);

            _isGameStarted = true;
            _isGameFinished = false;
        }

        public void End()
        {
            foreach (var graspable in _graspables)
            {
                graspable.gameObject.SetActive(false);
            }
            foreach (var collector in _collectors)
            {
                collector.gameObject.SetActive(false);
            }

            _timer.Stop();
        }

        private void StoreSessionData(string wasExerciseComplete, string timestamp)
        {
            int boxErrors = 0;
            float boxErrorsTime = 0.0f;
            Tuple<string, float> mostUsedOperatingZone = new Tuple<string, float>("None", 0.0f);
            Tuple<string, int> mostErrorZone = new Tuple<string, int>("None", 0);
            List<Tuple<string, float, int, float>> zonesInfo = new List<Tuple<string, float, int, float>>();
            List<Tuple<string, string, string>> graspablesInfo = GetGraspablesInfo();
            List<Tuple<string, string>> graspablesCorrectPlacementInfo = GetGraspablesCorrectPlacementInfo();

            if (_operatingBox != null)
            {
                boxErrors = _operatingBox.GetErrors();
                boxErrorsTime = _operatingBox.GetErrorTime();
                mostUsedOperatingZone = _operatingBox.GetMostUsedOperatingZone();
                mostErrorZone = _operatingBox.GetOperatingZoneWithMostErrors();
                zonesInfo = _operatingBox.GetPerOperatingZoneUsageTimeAndErrorsAndErrorTime();
            }

            ExerciseLogger.Instance.LogSessionData(timestamp, _minigameName, wasExerciseComplete, _timer.time, _errors, boxErrors, boxErrorsTime, mostUsedOperatingZone.Item1,
                                            mostUsedOperatingZone.Item2, mostErrorZone.Item1, mostErrorZone.Item2, zonesInfo, graspablesInfo, graspablesCorrectPlacementInfo);
        }

        private List<Tuple<string, string, string>> GetGraspablesInfo()
        {
            List<Tuple<string, string, string>> graspablesInfo = new List<Tuple<string, string, string>>();
            foreach (Graspable graspable in _graspables)
            {
                foreach (GraspableTimestamps timestamps in graspable.graspableTimestampsLogs)
                {
                    Tuple<string, string, string> graspableData = new Tuple<string, string, string>(graspable.gameObject.name, timestamps.graspStartedTimestamp, timestamps.graspEndedTimestamp);
                    graspablesInfo.Add(graspableData);
                }
            }

            return graspablesInfo;
        }

        private List<Tuple<string, string>> GetGraspablesCorrectPlacementInfo()
        {
            List<Tuple<string, string>> correctGraspablesPlacementInfo = new List<Tuple<string, string>>();
            foreach (Graspable graspable in _graspables)
            {
                foreach (string correctPlacementTimestamp in graspable.graspableCorrectPlacementTimestampLogs)
                {
                    Tuple<string, string> graspableCorrectPlacementData = new Tuple<string, string>(graspable.gameObject.name, correctPlacementTimestamp);
                    correctGraspablesPlacementInfo.Add(graspableCorrectPlacementData);
                }
            }

            return correctGraspablesPlacementInfo;
        }
    }
}
