using UnityEngine;

public class IKPivot : MonoBehaviour
{
    private static float ORIGIN_THRESHOLD = 0.035f; // Under .03f of magnitude unity Vector3.normalize seems to not work properly.
    [SerializeField] private IKPivotTarget _target = null; // Anchor of the inserted object.
    private PivotInsertable _inserted; // The inserted object.
    private bool _previousGrabbingState; // Util to reset the grabbing state.

    [SerializeField] private bool _singleSide = false; // If true insertion only allowed in front.
    [SerializeField] private bool _coneConstraints = true; // If true cone used for constraints instead of pyramid. (Much better result)
    //Constraints
    [Range(0, 180)]
    [SerializeField] private float _angleLimit = 90; // The cone/pyramid angle.
    [SerializeField] private float _insertionLimit = 0.05f; // The limit of forward insertion.

    [Range(0f, .5f)]
    [SerializeField] private float _exceedLimit = 0.15f; // The limit distance out of constraints bounds.

    private Vector3 _feasiblePosition; // The feasible position inside the bounds.
    private Quaternion _feasibleOrientation; // The current feasible lookAtPivot orientation.

    private bool _dirty; // Dirty check to restore the _target position.
    private bool _frontFacing = false; // Util to remeber if insertable was inserted frontly or not.

    private float _scaledInsertionLimit;
    private float _scaledExceedLimit;

    private IZoneInsertable _insertableZone;

    /// <value>The inserted object in the pivot.</value>
    public PivotInsertable inserted
    {
        get { return _inserted; }
    }

    private void Awake()
    {
        _insertableZone = GetComponentInParent(typeof(IZoneInsertable)) as IZoneInsertable;
    }

    // Start is called before the first frame update
    void Start()
    {
        _angleLimit = Mathf.Clamp(_angleLimit, 0, 180); // Clamp the angle jsut to be safe.
        _dirty = false; // init to clear.
        _scaledInsertionLimit = WorldScaler.Instance.GetScaledValue(_insertionLimit);
        _scaledExceedLimit = WorldScaler.Instance.GetScaledValue(_exceedLimit);
    }

    void FixedUpdate()
    {
        if (_inserted != null) // If something inserted
        {
            if (_target.grabbable.isGrabbed) // If user currently interacting with inserted object.
            {
                if (!_dirty)
                {
                    _inserted.grabbable.OverrideGrabber(_target.grabbedBy);
                }
                _dirty = true; // Object is likely moved at this point, set it to dirty.

                // Adjust postion ad orientation of inserted object to Pivot. If something is wrong false is returned.
                bool clamped = false;
                bool adjusted = AdjustInsertedToPivot(out _feasiblePosition, out _feasibleOrientation, out clamped);
                if (!adjusted) // If inserted adjustment went wrong force the object release (it will be resetted) and block the procedure
                {
                    _target.grabbedBy.ForceRelease();
                    return;
                }

                float sqrDistace = (_target.transform.position - _feasiblePosition).sqrMagnitude;

                if (clamped)
                {
                    float vibrationLevel = sqrDistace / (_scaledExceedLimit * _scaledExceedLimit);
                    _target.grabbable.vibrationFeedback = vibrationLevel;
                }
                else
                {
                    _target.grabbable.vibrationFeedback = 0f;
                }

                // Force release if the _target exceed the maximum allowed distance from the feasiblePosition.
                if (sqrDistace > _scaledExceedLimit * _scaledExceedLimit)
                {
                    _target.grabbable.vibrationFeedback = 0f;
                    _target.grabbedBy.ForceRelease();
                }
            }
            else // If user not interacting with inserted object.
            {
                if (_dirty)
                {
                    _target.transform.position = _feasiblePosition; // Reset _target to last safe position (the same position of the insertable).
                    _target.AdjustGrabPoints(_inserted.grabbable.transform); // Ajust the grabPoints to match again the inserted object
                    VibrationHandler.Instance.DisableVibration(_target.grabbable);
                    _inserted.grabbable.OverrideGrabber(null); // Nothing is currently grabbing the inserted object
                    _dirty = false;
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Debug.Log("enter in: " + transform.name + " staff " + other.name);

        PivotInsertable insertable = other.GetComponent<PivotInsertable>();
        if (insertable == null)
            return;

        insertable.EnterPivot(this);
    }

    private void OnTriggerExit(Collider other)
    {
        // Debug.Log("exit in: " + transform.name + " staff " + other.name);

        PivotInsertable insertable = other.GetComponent<PivotInsertable>();
        if (insertable == null)
            return;

        insertable.ExitPivot(this);
    }


    /// <summary>
    /// Check if insertable can be currently inserted in the pivot.
    /// </summary>
    /// <param name="insertable">The insertable</param>
    /// <returns></returns>
    public bool CanBeInserted(PivotInsertable insertable)
    {
        if (_inserted != null || insertable.grabbable.grabbedBy == null) // if there was something in the pivot, or the object triggering the pivot is not grabbed by anything
            return false;

        // Set feasible position to start of insertable staff.
        Vector3 feasiblePositionTemp = insertable.transform.position;

        // Set the frontFacing to true if the isnertable is inserted in front of the pivot just to check.
        bool frontFacingTemp = false;
        if (transform.InverseTransformPoint(insertable.transform.position).z > 0)
            frontFacingTemp = true;

        // Check the feasible position.
        bool outOfBounds = _coneConstraints ?
            ClampPositionCone(feasiblePositionTemp, frontFacingTemp, out feasiblePositionTemp) :
            ClampPositionPyramid(feasiblePositionTemp, frontFacingTemp, out feasiblePositionTemp);

        // If near to origin can generate wierd result due to floating point.
        if (NearToOrigin(feasiblePositionTemp))
        {
            outOfBounds = true;
            return false;
        }

        // Clamp insertion
        _dirty = ClampInsertion(feasiblePositionTemp, out feasiblePositionTemp) || _dirty;

        // Once feasiblity is checked retrieve position of the grabbable
        feasiblePositionTemp = feasiblePositionTemp + insertable.offset;

        // If the feasible position distance from the current insertable grabbable position exceed in distance the limit (plus a safe guard), the insertable cannot be inserted.
        if ((insertable.grabbable.transform.position - feasiblePositionTemp).sqrMagnitude > (_scaledExceedLimit * _scaledExceedLimit) - .01f)
            return false;

        // If insertable can be inserted only by the front side and it is currenly facing the back one.
        if (_singleSide && transform.InverseTransformPoint(insertable.transform.position).z < 0)
            return false;

        // Precompute position
        _feasiblePosition = feasiblePositionTemp;

        return true;
    }

    /// <summary>
    /// Insert an insertable in the pivot.
    /// </summary>
    /// <param name="insertable">The isnertable to insert.</param>
    public void Insert(PivotInsertable insertable)
    {
        if (_inserted != null || insertable.grabbable.grabbedBy == null) // if there was something in the pivot, or the object triggering the pivot is not grabbed by anything
            return;

        // Save the insertable and force the release (now the pivot will move it in the fixedUpdate)
        _inserted = insertable;
        Grabber currentGrabber = _inserted.grabbable.grabbedBy;
        currentGrabber.ForceRelease();

        // Force the target grab and set its state to make it interactable.
        _target.grabbable.currentSnapAdapter.OverrideSnaps(_inserted.grabbable.currentSnapAdapter);
        currentGrabber.ForceGrab(_target.grabbable);
        _target.grabbable.enableGrab = true;
        _target.grabbable.allowedGrabbinghand = _inserted.grabbable.allowedGrabbinghand;

        // Give to the _target same imputs of the insertable.
        InputDispatcher dispatcher = _inserted.grabbable.gameObject.GetComponent<InputDispatcher>();
        if (dispatcher != null)
            _target.OverrideInputs(dispatcher);
        _target.OverrideGrabPoints(_inserted.grabbable.grabPoints, _feasibleOrientation);
        _inserted.grabbable.OverrideGrabber(_target.grabbedBy);

        // Save inserted original state and disable its grab
        _previousGrabbingState = _inserted.grabbable.enableGrab;
        _inserted.grabbable.enableGrab = false;

        // Set the frontFacing to true if the isnertable is inserted in front of the pivot.
        _frontFacing = false;
        if (transform.InverseTransformPoint(_inserted.transform.position).z > 0)
            _frontFacing = true;

        if (_insertableZone != null)
            _insertableZone.Insert(insertable.grabbable);

    }

    public void Remove()
    {
        // Force target release and reset its state
        Grabber currentGrabber = _target.grabbedBy;
        currentGrabber.ForceRelease();
        _target.grabbable.currentSnapAdapter.ResetSnaps();
        _target.grabbable.enableGrab = false;
        _target.grabbable.allowedGrabbinghand = HandType.None;
        _target.ResetInputs(); // Clear the insertable inputs from _target.
        _target.ResetGrabPoints();

        // Reset the inserted grabbable to its original state and put it back in the hand.
        _inserted.grabbable.enableGrab = _previousGrabbingState;
        currentGrabber.ForceGrab(_inserted.grabbable);

        if (_insertableZone != null)
            _insertableZone.Remove(_inserted.grabbable);

        // Reset the saved inserted.
        _inserted = null;
        _dirty = false; // Reset

    }

    // Adjust inserted object to pivot (ensure _inserted is not null before inserting it)
    private bool AdjustInsertedToPivot(out Vector3 clampedPosition, out Quaternion lookAtOrientation, out bool clamped)
    {
        // Set initial ouputs to "last-frame" legit values.
        clampedPosition = _feasiblePosition;
        lookAtOrientation = _feasibleOrientation;
        clamped = false;

        // Position clamping

        Vector3 delta = _target.transform.position - _inserted.grabbable.transform.position; // Calcualte delta movement from last-frame position to the next one.
        Vector3 insertableOrigin = _inserted.origin + delta; // This is a prediction. We need it since the true _inserted origin is not jet updated and is referring last frame position. With this prediction we can prevent the object to jitter around the pivot while moving.

        // Clamp _insreted.origin inside the pivot boundaries
        if (_coneConstraints)
            clamped = ClampPositionCone(insertableOrigin, out insertableOrigin);
        else
            clamped = ClampPositionPyramid(insertableOrigin, out insertableOrigin);

        // Now on if the clamped _inserted.origin is to much near to the pivot position we can have serious problems with directions due to float precision. Block the adjustement and return false.
        if (NearToOrigin(insertableOrigin))
            return false; // Nope

        // Clamp insertion of _inserted.origin
        clamped = ClampInsertion(insertableOrigin, out insertableOrigin) || clamped;

        // Rotation Clamping
        Vector3 lookAtDir = (transform.position - insertableOrigin).normalized; // Look at Pivot direction the insertable need to reach
        Vector3 insertLocalDir = _inserted.grabbable.transform.InverseTransformDirection(_inserted.direction); // The direction of the inserted local to its grabbable parent
        Vector3 predictedDir = _target.transform.rotation * insertLocalDir; // The direction of the inserted remapped in the _target space (oriented as it)
        Quaternion finalOrientation = Quaternion.FromToRotation(predictedDir, lookAtDir) * _target.transform.rotation; // final oreintation = _target.rotation rotated by the necessary rotation to reach the lookAtDir.

        // Adjust the insertable

        clampedPosition = insertableOrigin + _inserted.offset; // Position of the inserted grabbable = position of insertable origin and offset between insertable origin and grabbable origin
        lookAtOrientation = finalOrientation; // Set orientation as calculated 

        // Move the _inserted
        _inserted.grabbable.grabbedRigidbody.MovePosition(clampedPosition);
        _inserted.grabbable.grabbedRigidbody.MoveRotation(lookAtOrientation);
        return true; // Everything OK
    }

    private bool ClampInsertion(Vector3 targetPosition, out Vector3 clampedPosition)
    {
        // EVERYTHING IS IN PIVOT LOCAL SPACE
        bool clamped = false;
        Vector3 targetLocal = transform.InverseTransformPoint(targetPosition); // Bring target position in local pivot space

        // Clamp isnertion
        float originDelta = Mathf.Max(_scaledInsertionLimit, ORIGIN_THRESHOLD) - targetLocal.magnitude;
        if (originDelta > 0)
        {
            clamped = true;
            targetLocal = targetLocal + (targetLocal.normalized * originDelta);
        }

        // Transform back in world-space and give it to out.
        clampedPosition = transform.TransformPoint(targetLocal);
        return clamped;
    }

    // Geometric clamp of position inside a pyramid.
    private bool ClampPositionPyramid(Vector3 targetPosition, out Vector3 clampedPosition)
    {
        // EVERYTHING IS IN PIVOT LOCAL SPACE
        bool clamped = false;
        float halfAngle = _angleLimit / 2;
        Vector3 targetLocal = transform.InverseTransformPoint(targetPosition); // Bring target position in local pivot space

        Vector3 xzProjPoint = Vector3.ProjectOnPlane(targetLocal, Vector3.up); // Project target on xz plane;
        Vector3 planePoint_x = Vector3.Project(xzProjPoint, Vector3.forward); // Project projected point onto forward dir to find third point needed to define the plane.
        Plane plane_x = new Plane(Vector3.zero, planePoint_x, xzProjPoint); // Generate the plane (it is xz, but doing so it can be flipped depending on xzPorjPoint position. In this way we have not to care about sign -> Better)
        float angle_x = Vector3.Angle(Vector3.forward, xzProjPoint.normalized); // Find the projected angle of _target from forward dir.
        angle_x = (_frontFacing || _singleSide) ? angle_x : 180 - angle_x; // Remap angles if frontfacing or singleSided-Pivot.
        if (halfAngle < 90 && angle_x > halfAngle) // if angle exceed limit ( if halfAngle == 90 it can rotate by 360 around pivot. halfAngle cannot be > 90 since angle is clamped to 180).
        {
            clamped = true;
            Vector3 limitDir = Quaternion.AngleAxis(halfAngle, plane_x.normal) * Vector3.forward; // Find the direction of the limit bound.
            Vector3 intersection; // Define intersection.
            if (halfAngle < 45) // This is not due to geometrical behaviours, but just for a better user experience.
            {
                // If there is and intersection -> clamp x (NB: in theory we will have and intersection for sure since prior if statments ensure it. But just to be sure.)
                if (LinesIntersection(Vector3.zero, limitDir, xzProjPoint, Vector3.right, out intersection))
                    targetLocal.x = intersection.x;
            }
            else
            {
                // Clamp x to intersection
                intersection = Vector3.Project(xzProjPoint, limitDir);
                targetLocal.x = intersection.x;
            }
        }

        // Repeat for y projecting on YZPlane.
        Vector3 yzProjPoint = Vector3.ProjectOnPlane(targetLocal, Vector3.right);
        Vector3 planePoint_y = Vector3.Project(yzProjPoint, Vector3.forward);
        Plane plane_y = new Plane(Vector3.zero, planePoint_y, yzProjPoint);
        float angle_y = Vector3.Angle(Vector3.forward, yzProjPoint.normalized);
        angle_y = (_frontFacing || _singleSide) ? angle_y : 180 - angle_y;
        if (halfAngle < 90 && angle_y > halfAngle)
        {
            clamped = true;
            Vector3 limitDir = Quaternion.AngleAxis(halfAngle, plane_y.normal) * Vector3.forward;
            Vector3 intersection;
            if (halfAngle < 45)
            {
                if (LinesIntersection(Vector3.zero, limitDir, yzProjPoint, Vector3.up, out intersection))
                    targetLocal.y = intersection.y;
            }
            else
            {
                intersection = Vector3.Project(yzProjPoint, limitDir);
                targetLocal.y = intersection.y;
            }
        }

        // Transform back in world-space and give it to out.
        clampedPosition = transform.TransformPoint(targetLocal);
        return clamped;
    }

    // Geometric clamp of position inside a pyramid.
    private bool ClampPositionPyramid(Vector3 targetPosition, bool frontFacing, out Vector3 clampedPosition)
    {
        // EVERYTHING IS IN PIVOT LOCAL SPACE
        bool clamped = false;
        float halfAngle = _angleLimit / 2;
        Vector3 targetLocal = transform.InverseTransformPoint(targetPosition); // Bring target position in local pivot space

        Vector3 xzProjPoint = Vector3.ProjectOnPlane(targetLocal, Vector3.up); // Project target on xz plane;
        Vector3 planePoint_x = Vector3.Project(xzProjPoint, Vector3.forward); // Project projected point onto forward dir to find third point needed to define the plane.
        Plane plane_x = new Plane(Vector3.zero, planePoint_x, xzProjPoint); // Generate the plane (it is xz, but doing so it can be flipped depending on xzPorjPoint position. In this way we have not to care about sign -> Better)
        float angle_x = Vector3.Angle(Vector3.forward, xzProjPoint.normalized); // Find the projected angle of _target from forward dir.
        angle_x = (frontFacing || _singleSide) ? angle_x : 180 - angle_x; // Remap angles if frontfacing or singleSided-Pivot.
        if (halfAngle < 90 && angle_x > halfAngle) // if angle exceed limit ( if halfAngle == 90 it can rotate by 360 around pivot. halfAngle cannot be > 90 since angle is clamped to 180).
        {
            clamped = true;
            Vector3 limitDir = Quaternion.AngleAxis(halfAngle, plane_x.normal) * Vector3.forward; // Find the direction of the limit bound.
            Vector3 intersection; // Define intersection.
            if (halfAngle < 45) // This is not due to geometrical behaviours, but just for a better user experience.
            {
                // If there is and intersection -> clamp x (NB: in theory we will have and intersection for sure since prior if statments ensure it. But just to be sure.)
                if (LinesIntersection(Vector3.zero, limitDir, xzProjPoint, Vector3.right, out intersection))
                    targetLocal.x = intersection.x;
            }
            else
            {
                // Clamp x to intersection
                intersection = Vector3.Project(xzProjPoint, limitDir);
                targetLocal.x = intersection.x;
            }
        }

        // Repeat for y projecting on YZPlane.
        Vector3 yzProjPoint = Vector3.ProjectOnPlane(targetLocal, Vector3.right);
        Vector3 planePoint_y = Vector3.Project(yzProjPoint, Vector3.forward);
        Plane plane_y = new Plane(Vector3.zero, planePoint_y, yzProjPoint);
        float angle_y = Vector3.Angle(Vector3.forward, yzProjPoint.normalized);
        angle_y = (frontFacing || _singleSide) ? angle_y : 180 - angle_y;
        if (halfAngle < 90 && angle_y > halfAngle)
        {
            clamped = true;
            Vector3 limitDir = Quaternion.AngleAxis(halfAngle, plane_y.normal) * Vector3.forward;
            Vector3 intersection;
            if (halfAngle < 45)
            {
                if (LinesIntersection(Vector3.zero, limitDir, yzProjPoint, Vector3.up, out intersection))
                    targetLocal.y = intersection.y;
            }
            else
            {
                intersection = Vector3.Project(yzProjPoint, limitDir);
                targetLocal.y = intersection.y;
            }
        }

        // Transform back in world-space and give it to out.
        clampedPosition = transform.TransformPoint(targetLocal);
        return clamped;
    }

    // Geometric clamp of position inside a cone.
    private bool ClampPositionCone(Vector3 targetPosition, out Vector3 clampedPosition)
    {
        // EVERYTHING IS IN PIVOT LOCAL SPACE
        bool clamped = false;
        float halfAngle = _angleLimit / 2;

        Vector3 targetLocal = transform.InverseTransformPoint(targetPosition); // Bring target position in local pivot space
        Vector3 planePoint = Vector3.Project(targetLocal, Vector3.forward); // Project projected point onto forward dir to find third point needed to define the plane.
        Plane plane = new Plane(Vector3.zero, planePoint, targetLocal); // Generate the plane.
        float angle = Vector3.Angle(Vector3.forward, targetLocal.normalized); // Find the projected angle of _target from forward dir.
        angle = (_frontFacing || _singleSide) ? angle : 180 - angle; // Remap angles if frontfacing or singleSided-Pivot.
        if (halfAngle < 90 && angle > halfAngle) // if angle exceed limit ( if halfAngle == 90 it can rotate by 360 around pivot. halfAngle cannot be > 90 since angle is clamped to 180).
        {
            clamped = true;
            Vector3 limitDir = Quaternion.AngleAxis(halfAngle, plane.normal) * Vector3.forward; // Find the direction of the limit bound.
            Vector3 intersection; // Define intersection.
            if (halfAngle < 45) // This is not due to geometrical behaviours, but just for a better user experience.
            {
                // If there is and intersection -> clamp (NB: in theory we will have an intersection for sure since prior if statments ensure it. But just to be sure.)
                if (LinesIntersection(Vector3.zero, limitDir, targetLocal, (planePoint - targetLocal).normalized, out intersection))
                    targetLocal = intersection;
            }
            else
            {
                // Clamp to intersection
                intersection = Vector3.Project(targetLocal, limitDir);
                targetLocal = intersection;
            }
        }

        // Transform back in world-space and give it to out.
        clampedPosition = transform.TransformPoint(targetLocal);
        return clamped;
    }

    // Geometric clamp of position inside a cone.
    private bool ClampPositionCone(Vector3 targetPosition, bool frontFacing, out Vector3 clampedPosition)
    {
        // EVERYTHING IS IN PIVOT LOCAL SPACE
        bool clamped = false;
        float halfAngle = _angleLimit / 2;

        Vector3 targetLocal = transform.InverseTransformPoint(targetPosition); // Bring target position in local pivot space
        Vector3 planePoint = Vector3.Project(targetLocal, Vector3.forward); // Project projected point onto forward dir to find third point needed to define the plane.
        Plane plane = new Plane(Vector3.zero, planePoint, targetLocal); // Generate the plane.
        float angle = Vector3.Angle(Vector3.forward, targetLocal.normalized); // Find the projected angle of _target from forward dir.
        angle = (frontFacing || _singleSide) ? angle : 180 - angle; // Remap angles if frontfacing or singleSided-Pivot.
        if (halfAngle < 90 && angle > halfAngle) // if angle exceed limit ( if halfAngle == 90 it can rotate by 360 around pivot. halfAngle cannot be > 90 since angle is clamped to 180).
        {
            clamped = true;
            Vector3 limitDir = Quaternion.AngleAxis(halfAngle, plane.normal) * Vector3.forward; // Find the direction of the limit bound.
            Vector3 intersection; // Define intersection.
            if (halfAngle < 45) // This is not due to geometrical behaviours, but just for a better user experience.
            {
                // If there is and intersection -> clamp (NB: in theory we will have an intersection for sure since prior if statments ensure it. But just to be sure.)
                if (LinesIntersection(Vector3.zero, limitDir, targetLocal, (planePoint - targetLocal).normalized, out intersection))
                    targetLocal = intersection;
            }
            else
            {
                // Clamp to intersection
                intersection = Vector3.Project(targetLocal, limitDir);
                targetLocal = intersection;
            }
        }

        // Transform back in world-space and give it to out.
        clampedPosition = transform.TransformPoint(targetLocal);
        return clamped;
    }

    // Check if a position is to near to pivot origin
    private bool NearToOrigin(Vector3 targetPosition)
    {
        Vector3 localTarget = transform.InverseTransformPoint(targetPosition);
        if (localTarget.magnitude < ORIGIN_THRESHOLD)
            return true;
        return false;
    }


    /// <summary>
    /// Find the intersection between two infinite lines.
    /// </summary>
    /// <param name="point_a">Point from which first line pass.</param>
    /// <param name="dir_a">Direction of first line.</param>
    /// <param name="point_b">Point from which second line pass.</param>
    /// <param name="dir_b">Direction of second line.</param>
    /// <param name="intersection">The output intersection</param>
    /// <returns></returns>
    private bool LinesIntersection(Vector3 point_a, Vector3 dir_a, Vector3 point_b, Vector3 dir_b, out Vector3 intersection)
    {
        Vector3 dir_c = point_b - point_a;
        Vector3 cross_ab = Vector3.Cross(dir_a, dir_b);
        Vector3 cross_cb = Vector3.Cross(dir_c, dir_b);
        float planarFactor = Vector3.Dot(dir_c, cross_ab);

        // if coplanar and not parallel
        if (Mathf.Abs(planarFactor) < 0.0001f && cross_ab.sqrMagnitude > 0.0001f)
        {
            float s = Vector3.Dot(cross_cb, cross_ab) / cross_ab.sqrMagnitude;
            intersection = point_a + (dir_a * s);
            return true;
        }

        intersection = Vector3.zero;
        return false;
    }
}
