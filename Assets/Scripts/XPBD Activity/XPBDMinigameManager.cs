using System;
using System.Collections.Generic;
using MinigameSystem;
using UnityEngine;

namespace XPBD_Activity
{
    public class XPBDMinigameManager : MonoBehaviour
    {
        [SerializeField] private OperatingBox operatingBox;
        [SerializeField] private GameObject degreePrefab;
        [SerializeField] private Transform degreeDefaultPosition ;
        [SerializeField] private AudioClip solvedClip ;
        private AudioSource _audioSource ;
        private GameObject _degreeGameObject ;
        private Degree _degree;
        private int _errors;
        private Timer _timer;
        
        private void Awake()
        {
            _timer = new Timer(this);
            _errors = 0;

            if (degreeDefaultPosition == null)
                throw new ArgumentException("Degree Default Position should not be null in " + this.name);

            if (degreePrefab != null)
            {
                _degreeGameObject = GameObject.Instantiate(degreePrefab, transform);
                _degree = _degreeGameObject.GetComponent<Degree>();
                if (_degree != null)
                    _degree.SetUp();
                _degreeGameObject.SetActive(false);
            }

            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
                throw new ArgumentException("Audio source should not be null in " + this.name);
        }
        public void Init()
        {
            _timer.Start();
            _errors = 0;
            
            if (operatingBox != null)
                operatingBox.ResetBox();

            _degreeGameObject.SetActive(false);
        }
        public void MinigameSolved(string minigameName,List<SelectedVertexInfo> selectedVertexInfos, List<GrabbedVertexInfo> grabbedVertexInfos)
        {
            _timer.Stop();
            _degreeGameObject.SetActive(true);
            _degree.ResetState();

            Tuple<string, float> mostUsedOperatingZone = new Tuple<string, float>("None", 0.0f);
            Tuple<string, int> mostErrorZone = new Tuple<string, int>("None", 0);

            string endSessionTime = DateTime.Now.ToString(XPBDExerciseLogger.Instance.cultureInfo);
            _degree.sessionId = minigameName + " " + endSessionTime;
            _degree.timeSpent = Timer.Format(_timer.time);
            _degree.minigameErrors = _errors.ToString();
            if (operatingBox != null)
            {
                mostUsedOperatingZone = operatingBox.GetMostUsedOperatingZone();
                mostErrorZone = operatingBox.GetOperatingZoneWithMostErrors();

                _degree.operatingErrors = operatingBox.GetErrors().ToString();
                _degree.operatingErrorTime = Timer.Format(operatingBox.GetErrorTime());
                _degree.mostUsedOperatingZone = mostUsedOperatingZone.Item1 + " for " + Timer.Format(mostUsedOperatingZone.Item2);
                _degree.mostErrorZone = mostErrorZone.Item1 + " with " + mostErrorZone.Item2 + " errors";
            }

            _degreeGameObject.transform.position = degreeDefaultPosition.position;
            _degreeGameObject.transform.rotation = degreeDefaultPosition.rotation;
            _audioSource.PlayOneShot(solvedClip);
            StoreSessionData(minigameName,"Yes", endSessionTime, selectedVertexInfos, grabbedVertexInfos);
        }
        public void NotifyError(int n = 1)
        {
            _errors += n;
        }
        public void End(string minigameName, List<SelectedVertexInfo> selectedVertexInfos, List<GrabbedVertexInfo> grabbedVertexInfos)
        {
            _timer.Stop();
            string endSessionTime = DateTime.Now.ToString(XPBDExerciseLogger.Instance.cultureInfo);
            StoreSessionData(minigameName,"No", endSessionTime,selectedVertexInfos,grabbedVertexInfos);
        }

        private void StoreSessionData(string minigameName, string wasExerciseComplete, string timestamp , List<SelectedVertexInfo> selectedVertexInfos, List<GrabbedVertexInfo> grabbedVertexInfos)
        {
            int boxErrors = 0;
            float boxErrorsTime = 0.0f;
            Tuple<string, float> mostUsedOperatingZone = new Tuple<string, float>("None", 0.0f);
            Tuple<string, int> mostErrorZone = new Tuple<string, int>("None", 0);
            List<Tuple<string, float, int, float>> zonesInfo = new List<Tuple<string, float, int, float>>();



            if (operatingBox != null)
            {
                boxErrors = operatingBox.GetErrors();
                boxErrorsTime = operatingBox.GetErrorTime();
                mostUsedOperatingZone = operatingBox.GetMostUsedOperatingZone();
                mostErrorZone = operatingBox.GetOperatingZoneWithMostErrors();
                zonesInfo = operatingBox.GetPerOperatingZoneUsageTimeAndErrorsAndErrorTime();
            }
            
            XPBDExerciseLogger.Instance.LogSessionData(
                timestamp, 
                minigameName, 
                wasExerciseComplete, 
                _timer.time,
                _errors, 
                boxErrors,
                
                boxErrorsTime,
                
                
                mostUsedOperatingZone.Item1,
                mostUsedOperatingZone.Item2, 
                
                mostErrorZone.Item1, 
                mostErrorZone.Item2, 
                zonesInfo, 
                selectedVertexInfos:selectedVertexInfos, 
                grabbedVertexInfos:grabbedVertexInfos);
            
        }
        
        
        
        
    }
}
