using UnityEngine;
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
        public static ActivityManager instance;
        public GameObject selectionSpherePrefab;    //The sphere that will be used to select the vertices
        public SoftBodyClassic softbodyActivity;    //The softbody that will be used for the activity
        public ActivitySettings activitySettings;   //The settings of the activity
    
        private bool _isActivityRunning;            //If the activity is running
        private GameObject _selectionSphere;        //The sphere that will be used to select the vertices
        private int _currentSelectedVertexIndex;    //The index of the current selected vertex

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
            HandleErrors();
        }
        private void Update()
        {
            if (_isActivityRunning)
            {
                //Update the selection sphere
                _selectionSphere.transform.position = softbodyActivity.GetVertexPos(activitySettings.vertexIndices[_currentSelectedVertexIndex]);
            }
            else
            {
                //Start the activity for debugging purposes
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    InitActivity();
                }
            }
        }

        //Handle the initialization of the activity
        private void InitActivity()
        {
            _selectionSphere = Instantiate(selectionSpherePrefab, softbodyActivity.GetVertexPos(activitySettings.vertexIndices[_currentSelectedVertexIndex]), Quaternion.identity);
            _selectionSphere.transform.localScale = Vector3.one * (activitySettings.grabbingDistance * 2f);
            _isActivityRunning = true;
        }
        
        //Handle the change of the grabbed vertex
        private void ChangeGrabbedVertex()
        {
            _currentSelectedVertexIndex++;
            if (_currentSelectedVertexIndex >= activitySettings.vertexIndices.Count)
            {
                _currentSelectedVertexIndex = 0;
                _isActivityRunning = false;
                Destroy(_selectionSphere);
                Debug.Log("Activity finished");
            }
        }
        
        //Called from the grabber to check if the vertex is close enough to be grabbed
        public void StartGrabbingVertex(int index)
        {
            if (_isActivityRunning)
            {
                var dist  = Vector3.Distance(softbodyActivity.GetVertexPos(activitySettings.vertexIndices[_currentSelectedVertexIndex]), softbodyActivity.GetVertexPos(index));
                if (dist <= activitySettings.grabbingDistance)
                {
                    ChangeGrabbedVertex();
                } 
            }
        }
        
        //Public method to start the activity
        public void StartActivity()
        {
            if (!_isActivityRunning)
            {
                InitActivity();
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
    
    }
}
