using UnityEngine;
using TMPro;

namespace MinigameSystem
{
    public class Degree : MonoBehaviour
    {
        private Grabbable _grabbable = null;
        private bool isGrabbable = false;
        private bool hasBeenGrabbed = false;


        [SerializeField] private Animator _animator = null;
        [SerializeField] private TextMeshPro _sessionId = null;
        [SerializeField] private TextMeshPro _timeSpent = null;
        [SerializeField] private TextMeshPro _minigameErrors = null;
        [SerializeField] private TextMeshPro _operatingErrors = null;
        [SerializeField] private TextMeshPro _operatingErrorTime = null;
        [SerializeField] private TextMeshPro _mostUsedOperatingZone = null;
        [SerializeField] private TextMeshPro _mostErrorZone = null;
        [SerializeField] private TextMeshPro _mostErrorTool = null;

        public string sessionId
        {
            set { _sessionId.SetText(value); }
        }

        public string timeSpent
        {
            set { _timeSpent.SetText(value); }
        }

        public string minigameErrors
        {
            set { _minigameErrors.SetText(value); }
        }

        public string operatingErrors
        {
            set { _operatingErrors.SetText(value); }
        }

        public string mostUsedOperatingZone
        {
            set { _mostUsedOperatingZone.SetText(value); }
        }

        public string operatingErrorTime
        {
            set { _operatingErrorTime.SetText(value); }
        }

        public string mostErrorZone
        {
            set { _mostErrorZone.SetText(value); }
        }

        public string mostErrorTool
        {
            set { _mostErrorTool.SetText(value); }
        }

        public void SetUp()
        {
            _grabbable = GetComponent<Grabbable>();
            if (_grabbable != null)
                isGrabbable = true;
        }

        public void ResetState()
        {
            hasBeenGrabbed = false;
            _animator.enabled = true;
            _grabbable.grabbedRigidbody.isKinematic = true;

            timeSpent = "";
            minigameErrors = "";
            operatingErrors = "";
            mostUsedOperatingZone = "";
            operatingErrorTime = "";
            mostErrorZone = "";
            mostErrorTool = "";
        }

        // Update is called once per frame
        void Update()
        {
            if (isGrabbable && !hasBeenGrabbed)
            {
                if (_grabbable.isGrabbed)
                {
                    hasBeenGrabbed = true;
                    _grabbable.defaultKinematic = false;
                    _animator.enabled = false;
                }
            }
        }
    }

}