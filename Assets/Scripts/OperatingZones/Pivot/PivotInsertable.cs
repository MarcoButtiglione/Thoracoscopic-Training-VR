using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(CapsuleCollider))]
public class PivotInsertable : MonoBehaviour
{
    private Grabbable _myGrabbable; // The grabbable of the insertable (is requirend ( at leas in parent ))
    private Vector3 _offset; // The offset between the origin of insertable and the grabbable center of mass.
    private IEnumerator insertingCorutine = null; // The corutin used to insert procedure.


    public Vector3 origin
    {
        get { return transform.position; }
    }

    public Vector3 direction
    {
        get { return transform.forward; }
    }

    // Grabbable of the isnertable
    public Grabbable grabbable
    {
        get { return _myGrabbable; }
    }

    public Vector3 offset
    {
        get { return _myGrabbable.transform.position - transform.position; }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Get the grabbable and throw if not found.
        _myGrabbable = GetComponentInParent<Grabbable>();
        if (_myGrabbable == null)
            throw new ArgumentException("PivotInsertable needs to be parented to a grabbable.");

        // get the collider (is required, no need to check)
        CapsuleCollider collider = GetComponent<CapsuleCollider>();
        if (!collider.isTrigger)
        {
            // Froce the collider to trigger if it is not.
            collider.isTrigger = true;
            Debug.LogWarning("Collider in PivotInsertable was not trigger. Forced to trigger.");
        }

        // Save the offset.
        _offset = _myGrabbable.transform.InverseTransformPoint(transform.position);
    }

    // private void OnTriggerEnter(Collider other)
    // {
    //     // Debug.Log("Staff enter: " + transform.name + " into " + other.name);

    //     // Get the pivot if any.
    //     IKPivot pivot = other.GetComponent<IKPivot>();
    //     if (pivot == null)
    //         return;

    //     // If no corutine are currently active start it.
    //     if (insertingCorutine == null)
    //     {
    //         insertingCorutine = TryToInsert(pivot);
    //         StartCoroutine(insertingCorutine);
    //     }
    // }

    // private void OnTriggerExit(Collider other)
    // {
    //     // Debug.Log("Staff exit: " + transform.name + " from " + other.name);

    //     // Get the pivot if any
    //     IKPivot pivot = other.GetComponent<IKPivot>();
    //     if (pivot == null)
    //         return;

    //     // Sto and reset the inserting corutine
    //     StopCoroutine(insertingCorutine);
    //     insertingCorutine = null;

    //     // If this insertable is the one inserted int he pivot remove it.
    //     if (pivot.inserted == this)
    //         pivot.Remove();
    // }

    // Corutine that insert the object inside the pivot as soon as possible.
    private IEnumerator TryToInsert(IKPivot pivot)
    {
        bool canInsert = false;
        while (!canInsert)
        {
            // Debug.Log("try to insert");
            canInsert = pivot.CanBeInserted(this);
            yield return null;
        }

        pivot.Insert(this);
        // Debug.Log("inserted");
    }

    public void EnterPivot(IKPivot pivot)
    {
        // If no corutine are currently active start it.
        if (insertingCorutine == null)
        {
            insertingCorutine = TryToInsert(pivot);
            StartCoroutine(insertingCorutine);
        }
    }

    public void ExitPivot(IKPivot pivot)
    {
        // Stop and reset the inserting corutine
        StopCoroutine(insertingCorutine);
        insertingCorutine = null;

        // If this insertable is the one inserted in the pivot remove it.
        if (pivot.inserted == this)
            pivot.Remove();
    }

}
