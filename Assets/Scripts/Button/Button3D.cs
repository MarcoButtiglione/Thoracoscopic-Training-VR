using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(BoxCollider))]
public abstract class Button3D : MonoBehaviour
{
    [SerializeField] protected bool _continuous = false;
    [SerializeField] protected UnityEvent command;

    [SerializeField] protected AudioClip _pressClip;
    [SerializeField] protected AudioClip _releaseClip;

    protected AudioSource _audioSource;
    protected int _enteredIndexesNumber = 0;
    protected bool _canBePressed = true;
    protected bool _triggered = false;


    public virtual void PressBehaviour()
    {
        if (_canBePressed)
        {
            _triggered = true;
            if (_pressClip != null)
                _audioSource.PlayOneShot(_pressClip);
        }
    }

    public virtual void ReleaseBehaviour()
    {
        _triggered = false;
        if (_releaseClip != null)
            _audioSource.PlayOneShot(_releaseClip);
    }

    protected virtual void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        var collider = GetComponent<BoxCollider>();
        if (!collider.isTrigger)
        {
            collider.isTrigger = true;
            Debug.LogWarning("Collider in 3D Button " + gameObject.name + " was not trigger. Forced to trigger.");
        }
    }
    protected virtual void FixedUpdate()
    {
        if (_triggered)
        {
            command.Invoke();
            if (!_continuous)
            {
                _triggered = false;
                _canBePressed = false;
            }
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Index"))
        {
            _enteredIndexesNumber++;
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Index"))
        {
            _enteredIndexesNumber--;
            if (_enteredIndexesNumber == 0)
            {
                _canBePressed = true;
            }
        }
    }
}
