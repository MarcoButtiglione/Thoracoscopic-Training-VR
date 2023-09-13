using UnityEngine;
using System;

[RequireComponent(typeof(CustomHand))]
public class InteractableFinger : MonoBehaviour
{
    [SerializeField] private GameObject _colliderPrefab = null;
    [SerializeField] private Transform _targetFinger = null;
    [SerializeField] private Vector3 _positionOffset = new Vector3();
    [SerializeField] private Vector3 _rotationOffset = new Vector3();
    private GameObject _fingerColliderIstance;
    private Rigidbody _fingerColliderRb;
    private Collider _fingerCollider;
    private CustomHand _myHand;


    private void Awake()
    {
        if (_colliderPrefab == null)
            throw new ArgumentException("Collider prefab in Interactable Finger not valid.");

        if (_targetFinger == null)
            throw new ArgumentException("Target Finger in Interactable Finger not valid.");

        _fingerColliderIstance = GameObject.Instantiate<GameObject>(_colliderPrefab);
        _fingerColliderRb = _fingerColliderIstance.GetComponent<Rigidbody>();
        _fingerCollider = _fingerColliderIstance.GetComponent<Collider>();

        if (_fingerColliderRb == null)
            throw new ArgumentException("Collider prefab in Interactable Finger needs a Rigidbody.");

        _myHand = GetComponent<CustomHand>();
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (_myHand.isPointing)
            _fingerCollider.enabled = true;
        else
            _fingerCollider.enabled = false;

        _fingerColliderRb.MovePosition(_targetFinger.TransformPoint(_positionOffset));
        _fingerColliderRb.MoveRotation(_targetFinger.rotation * Quaternion.Euler(_rotationOffset));
    }

}
