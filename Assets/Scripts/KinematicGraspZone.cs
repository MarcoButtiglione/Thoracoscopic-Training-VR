using UnityEngine;
using MinigameSystem;

[RequireComponent(typeof(Collider))]
public class KinematicGraspZone : MonoBehaviour
{
    private Graspable _touchedGraspable = null;
    private bool _enabled = false;
    public Graspable touchedGraspable
    {
        get { return _touchedGraspable; }
    }

    private void Awake()
    {
        Collider collider = GetComponent<Collider>();
        if (!collider.isTrigger)
        {
            collider.isTrigger = true;
            Debug.LogWarning("Collider in KinematicGrasp [" + gameObject.name + "] was not trigger. Forced to trigger.");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (_enabled)
        {
            Graspable graspable = other.GetComponentInParent<Graspable>();
            if (graspable != null)
            {
                if (_touchedGraspable == null) _touchedGraspable = graspable;
            }
        }
    }

    public void EnableGrasp()
    {
        _enabled = true;
    }

    public void DisableGrasp()
    {
        _enabled = false;
        _touchedGraspable = null;
    }
}
