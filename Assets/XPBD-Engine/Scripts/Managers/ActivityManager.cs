using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XPBD_Activity;
using XPBD_Activity.Utility;
using XPBD_Engine.Scripts.Physics.Grabber;
using XPBD_Engine.Scripts.Physics.SoftBody;
using XPBD_Engine.Scripts.ScriptableObjects;

namespace XPBD_Engine.Scripts.Managers
{
    /*
     * This class is used to manage the activity
     * It will handle the selection of the vertices and the change of the grabbed vertex
     */
    public class ActivityManager : MonoBehaviour
    {
        private static readonly Color NormalColor = new Color(1f, 1f, 1f, 0.3f);
        private static readonly Color ConfirmedColor = new Color(0f, 1f, 0f, 0.3f);
        private static readonly Color ErrorColor = new Color(1f, 1f, 0f, 0.3f);
        private const float AlphaNormalColor = 0.3f;
        private const float AlphaNearColor = 0.1f;
        
        public static ActivityManager instance;
        public string activityName;                             //The name of the activity
        public GameObject selectionSpherePrefab;                //The sphere that will be used to select the vertices
        public SoftBodyClassic softbodyActivity;                //The softbody that will be used for the activity
        public ActivitySettings activitySettings;               //The settings of the activity
        public XPBDMinigameManager minigameManager;             //The minigame manager
        public MeshRenderer startButtonMeshRenderer;            //The mesh renderer of the start button
        public Material startButtonActiveMaterial;              //The active material of the start button
        public Material startButtonInactiveMaterial;            //The inactive material of the start button
        public AudioClip confirmClip;                           //The clip that will be played when the vertex is confirmed
        public AudioClip errorClip;                             //The clip that will be played when the vertex is not confirmed
        public ActivityType activityType;                       //The type of the activity
        
        private AudioSource _audioSource;                       //The audio source of the activity
        private List<int> _vertexIndices;                       //The vertex indices
        private bool _isActivityRunning;                        //If the activity is running
        private GameObject _selectionSphere;                    //The sphere that will be used to select the vertices
        private MeshRenderer _selectionSphereMeshRenderer;      //The mesh renderer of the selection sphere
        private int _currentSelectedVertexIndex;                //The index of the current selected vertex
        private bool _isGrabberNearVertex;                      //If the grabber is near the vertex
        private GrabberSphereController[] _grabbers;            //The grabbers that will be used to grab the vertices
        private Vector3 _currentSelectedVertexPos;              //The position of the current selected vertex
        private Color _currentColor;                            //The current color of the selection sphere
        private bool _isPlayingErrorColorCoroutine;             //If the error color coroutine is playing
        private bool _isPlayingConfirmedColorCoroutine;         //If the confirmed color coroutine is playing
        private List<SelectedVertexInfo> _selectedVertexInfos;  //The selected vertex infos
        private List<GrabbedVertexInfo> _grabbedVertexInfos;    //The grabbed vertex infos
        private SelectedVertexInfo _currentSelectedVertexInfo;  //The current selected vertex info
        
        
        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _grabbers = FindObjectsOfType<GrabberSphereController>();
            _selectedVertexInfos = new List<SelectedVertexInfo>();
            _grabbedVertexInfos = new List<GrabbedVertexInfo>();
            HandleErrors();
        }
        private void OnEnable()
        {
            instance = this;
        }
        private void OnDisable()
        {
            if(_isActivityRunning) HandleEndActivity(false);
            instance = null;
        }
        private void Update()
        {
            if (!_isActivityRunning) return;
            MoveSelectionSphere(_currentSelectedVertexPos);
        }

        private void FixedUpdate()
        {
            if (!_isActivityRunning) return;
            _currentSelectedVertexPos = softbodyActivity.GetVertexPos(_vertexIndices[_currentSelectedVertexIndex]);
            
            CheckGrabberNearVertex();
        }
        
        private void CheckGrabberNearVertex()
        {
            if (!_isActivityRunning) return;
            
            var previousIsGrabberNearVertex = _isGrabberNearVertex;
            foreach (var grabber in _grabbers)
            {
                if (Vector3.Distance(grabber.transform.position, _currentSelectedVertexPos) <= activitySettings.grabbingDistance)
                {
                    _isGrabberNearVertex = true;
                    break;
                }
                _isGrabberNearVertex = false;
            }
            if(previousIsGrabberNearVertex != _isGrabberNearVertex)
                SetCurrentSphereAlpha(_isGrabberNearVertex ? AlphaNearColor : AlphaNormalColor);
        }

        
        //Handle the initialization of the activity
        private void HandleInitActivity()
        {
            switch (activityType)
            {
                case ActivityType.Ordered:
                    _vertexIndices = new List<int>(activitySettings.vertexIndices);
                    break;
                case ActivityType.Random:
                    _vertexIndices = new List<int>(activitySettings.vertexIndices);
                    ListUtilityFunctions.Shuffle(_vertexIndices);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            
            _currentSelectedVertexIndex = 0;
            HandleInitInfos();
            startButtonMeshRenderer.material = startButtonActiveMaterial;
            
            _currentSelectedVertexPos = softbodyActivity.GetVertexPos(_vertexIndices[_currentSelectedVertexIndex]);
            HandleInitSelectionSphere(_currentSelectedVertexPos);
            

            _isActivityRunning = true; 
            
            
            minigameManager.Init();
        }

        #region Infos
        private void HandleInitInfos()
        {
            _selectedVertexInfos = new List<SelectedVertexInfo>();
            _grabbedVertexInfos = new List<GrabbedVertexInfo>();
            NewSelectedVertexInfo(_vertexIndices[_currentSelectedVertexIndex], DateTime.Now.ToString(XPBDExerciseLogger.Instance.cultureInfo));
        }
        private void NewSelectedVertexInfo(int vertexIndex, string selectedStartedTimestamp)
        {
            _currentSelectedVertexInfo = new SelectedVertexInfo(
                vertexIndex, 
                selectedStartedTimestamp, 
                ""
            );
        }
        private void AddSelectedVertexInfo(string selectedEndedTimestamp)
        {
            _currentSelectedVertexInfo.selectedEndedTimestamp = selectedEndedTimestamp;
            _selectedVertexInfos.Add(_currentSelectedVertexInfo);
        }
        private void AddGrabbedVertexInfo(int selectedVertexIndex, int grabbedVertexIndex, bool isAccepted, float distanceBetweenVertices, string timestamp)
        {
            _grabbedVertexInfos.Add(new GrabbedVertexInfo(
                selectedVertexIndex,
                grabbedVertexIndex,
                isAccepted,
                distanceBetweenVertices,
                timestamp
            ));
        }
        
        #endregion

        #region SelectionSphere
        private void HandleInitSelectionSphere(Vector3 initPos)
        {
            _selectionSphere = Instantiate(selectionSpherePrefab, initPos, Quaternion.identity);
            _selectionSphereMeshRenderer = _selectionSphere.GetComponent<MeshRenderer>(); 
            _selectionSphere.transform.localScale = Vector3.one * (activitySettings.grabbingDistance * 2f); 
            SetCurrentSphereColor(NormalColor);
        }
        private void MoveSelectionSphere(Vector3 newPos)
        {
            _selectionSphere.transform.position = newPos;
        }
        private void HandleEndSelectionSphere()
        {
            Destroy(_selectionSphere);
        }
        private void SetCurrentSphereColor(Color color)
        {
            _currentColor = color;
            _selectionSphereMeshRenderer.material.color = color;
        }
        private void SetCurrentSphereAlpha(float alpha)
        {
            _currentColor.a = alpha;
            _selectionSphereMeshRenderer.material.color = _currentColor;
        }
        #endregion

        private void HandleEndActivity(bool solved)
        {
            
            HandleEndSelectionSphere();
            startButtonMeshRenderer.material = startButtonInactiveMaterial;
            
            if(solved)
                minigameManager.MinigameSolved(GetActivityName(),_selectedVertexInfos, _grabbedVertexInfos);
            else
                minigameManager.End(GetActivityName(),_selectedVertexInfos, _grabbedVertexInfos);
            
            _isActivityRunning = false;
        }
        
        //Handle the change of the grabbed vertex
        private void ChangeGrabbedVertex()
        {
            AddSelectedVertexInfo(DateTime.Now.ToString(XPBDExerciseLogger.Instance.cultureInfo));
            
            _currentSelectedVertexIndex++;
            
            if (_currentSelectedVertexIndex >= _vertexIndices.Count)
            {
                HandleEndActivity(true);

            }
            else
            {
                NewSelectedVertexInfo( _vertexIndices[_currentSelectedVertexIndex], DateTime.Now.ToString(XPBDExerciseLogger.Instance.cultureInfo));
            }
        }
        
        
        //Called from the grabber to check if the vertex is close enough to be grabbed
        public void StartGrabbingVertex(int index)
        {
            if (!_isActivityRunning) return;
            if(_isPlayingConfirmedColorCoroutine) return;
            
            var dist  = Vector3.Distance(softbodyActivity.GetVertexPos(_vertexIndices[_currentSelectedVertexIndex]), softbodyActivity.GetVertexPos(index));
            if (dist <= activitySettings.grabbingDistance)
            {
                HandleValidGrabbedVertex(index, dist);
            }
            else
            {
                HandleInvalidGrabbedVertex(index, dist);
            }
            
        }
        private void HandleValidGrabbedVertex(int grabbedVertexIndex, float dist)
        {
            if (_isPlayingErrorColorCoroutine)
            {
                StopAllCoroutines();
                _isPlayingErrorColorCoroutine = false;
            }
            AddGrabbedVertexInfo( _vertexIndices[_currentSelectedVertexIndex], grabbedVertexIndex, true, dist, DateTime.Now.ToString(XPBDExerciseLogger.Instance.cultureInfo));
            _audioSource.PlayOneShot(confirmClip);
            StartCoroutine(ChangeGrabbedVertexCoroutine(1f));
        }
        private void HandleInvalidGrabbedVertex(int grabbedVertexIndex, float dist)
        {
            AddGrabbedVertexInfo( _vertexIndices[_currentSelectedVertexIndex], grabbedVertexIndex, false, dist, DateTime.Now.ToString(XPBDExerciseLogger.Instance.cultureInfo));
            _audioSource.PlayOneShot(errorClip);
            StartCoroutine(ChangeErrorColorCoroutine(2f));
            minigameManager.NotifyError();
        }
        private IEnumerator ChangeErrorColorCoroutine(float duration)
        {
            _isPlayingErrorColorCoroutine = true;
            SetCurrentSphereColor(ErrorColor);
            yield return new WaitForSeconds(duration);
            SetCurrentSphereColor(NormalColor);
            _isPlayingErrorColorCoroutine = false;
        }
        private IEnumerator ChangeGrabbedVertexCoroutine(float duration)
        {
            _isPlayingConfirmedColorCoroutine = true;
            SetCurrentSphereColor(ConfirmedColor);
            yield return new WaitForSeconds(duration);
            ChangeGrabbedVertex();
            SetCurrentSphereColor(NormalColor);
            _isPlayingConfirmedColorCoroutine = false;
        }
        
        //Public method to start the activity
        public void StartActivity()
        {
            if (!_isActivityRunning)
            {
                HandleInitActivity();
            }
        }
        //Public method to stop the activity
        public void StopActivity()
        {
            if (_isActivityRunning)
            {
                HandleEndActivity(false);
            }
        }
        //Handle the error messages
        private void HandleErrors()
        {
            if (softbodyActivity == null)
            {
                Debug.LogError("You must include a softbody to the activity manager");
            }

            if (activitySettings == null)
            {
                Debug.LogError("You must include a activity settings to the activity manager");
            }
        
            if(selectionSpherePrefab == null)
            {
                Debug.LogError("You must include a selection sphere prefab to the activity manager");
            }
        }
        
        public void  ChangeActivityType(ActivityType type, out ActivityType activity)
        {
            if (!_isActivityRunning)
            {
                activityType = type;
            } 
            activity = activityType;
        }
        
        private string GetActivityName()
        {
            var name = activityName;
            switch (activityType)
            {
                case ActivityType.Ordered:
                    name += "_Ordered";
                    break;
                    
                case ActivityType.Random:
                    name += "_Random";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return name;
        }
        
        
        
        
    }
}

public struct SelectedVertexInfo
{
    public int vertexIndex;
    public string selectedStartedTimestamp;
    public string selectedEndedTimestamp;
    
    public SelectedVertexInfo(int vertexIndex, string selectedStartedTimestamp, string selectedEndedTimestamp)
    {
        this.vertexIndex = vertexIndex;
        this.selectedStartedTimestamp = selectedStartedTimestamp;
        this.selectedEndedTimestamp = selectedEndedTimestamp;
    }
}
public struct GrabbedVertexInfo
{
    public int selectedVertexIndex;
    public int grabbedVertexIndex;
    public bool isAccepted;
    public float distanceBetweenVertices;
    public string timestamp;
    
    public GrabbedVertexInfo(int selectedVertexIndex, int grabbedVertexIndex, bool isAccepted, float distanceBetweenVertices, string timestamp)
    {
        this.selectedVertexIndex = selectedVertexIndex;
        this.grabbedVertexIndex = grabbedVertexIndex;
        this.isAccepted = isAccepted;
        this.distanceBetweenVertices = distanceBetweenVertices;
        this.timestamp = timestamp;
    }
}

public enum ActivityType
{
    Ordered,
    Random,
}