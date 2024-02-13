using System.Collections;
using UnityEngine;

namespace XPBD_Activity
{
    public class SelectionSphere: MonoBehaviour
    {
        private static readonly Color NormalColor = new Color(1f, 1f, 1f, 0.3f);
        private static readonly Color ConfirmedColor = new Color(0f, 1f, 0f, 0.3f);
        private static readonly Color ErrorColor = new Color(1f, 1f, 0f, 0.3f);
        private const float AlphaNormalColor = 0.3f;
        private const float AlphaNearColor = 0.1f;
        
        
        private MeshRenderer _selectionSphereMeshRenderer;      //The mesh renderer of the selection sphere
        private bool _isGrabberNearVertex;                      //If the grabber is near the vertex
        private Color _currentColor;                            //The current color of the selection sphere
        private bool _isPlayingErrorColorCoroutine;             //If the error color coroutine is playing

        private void Awake()
        {
            _selectionSphereMeshRenderer = GetComponent<MeshRenderer>();
            SetCurrentSphereColor(NormalColor);
        }
        public void Move(Vector3 newPos)
        {
            gameObject.transform.position = newPos;
        }

        public void SetRadius(float radius)
        {
            gameObject.transform.localScale =  Vector3.one * (radius * 2f);
        }
        public void SetNearVertex(bool isNear)
        {
            var previous = _isGrabberNearVertex;
            _isGrabberNearVertex = isNear;
            
            if (previous != _isGrabberNearVertex)
            {
                SetCurrentSphereAlpha(_isGrabberNearVertex? AlphaNearColor : AlphaNormalColor);
            }
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
        public void SetConfirmed(bool isConfirmed)
        {
            StopError();
            SetCurrentSphereColor(isConfirmed? ConfirmedColor : NormalColor);
        }
        public void SetError(float duration)
        {
            if (!_isPlayingErrorColorCoroutine)
            {
                StartCoroutine(ChangeErrorColorCoroutine(duration));
            }
        }
        private void StopError()
        {
            if (_isPlayingErrorColorCoroutine)
            {
                StopCoroutine(ChangeErrorColorCoroutine(0));
                SetCurrentSphereColor(NormalColor);
                _isPlayingErrorColorCoroutine = false;
            }
        }
        
        
        private IEnumerator ChangeErrorColorCoroutine(float duration)
        {
            _isPlayingErrorColorCoroutine = true;
            SetCurrentSphereColor(ErrorColor);
            yield return new WaitForSeconds(duration);
            SetCurrentSphereColor(NormalColor);
            _isPlayingErrorColorCoroutine = false;
        }
        
        
    }
}