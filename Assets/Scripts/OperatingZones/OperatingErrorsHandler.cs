using System.Collections.Generic;
using UnityEngine;

public class OperatingErrorsHandler : MonoBehaviour
{
    // The parent error handler, if any.
    [SerializeField] private OperatingErrorsHandler _parent = null;
    [SerializeField] private bool _parentSync = true;
    [SerializeField] private List<GameObject> _visualFeedbackObjects = new List<GameObject>();
    private bool _hasParent = false;
    private bool _timerEnabled = true;

    private Timer _errorTimer; // The timer keeping track of time spent in error state.
    private Dictionary<Grabbable, int> _grabbablesErrorVirtualTimers = new Dictionary<Grabbable, int>(); // Virtual timers build upon _errorTimer to keep track of errortime in a grabbable specific way.
    private int _errorsCount = 0;
    private int _personalErrorCount = 0; // Error count that do not consider errors from children handler (if any, if not is equal to _errorsCount)
    private List<OperatingZoneLimit> _operatingZoneLimits = new List<OperatingZoneLimit>(); // Set of limits of this handler
    private Dictionary<Grabbable, int> _grabbableCollisions = new Dictionary<Grabbable, int>(); // Current collisison of each grabbable (missing reference means 0)
    private Dictionary<Grabbable, int> _grabbableErrors = new Dictionary<Grabbable, int>(); // Errors count per grabbable


    public bool enableTimer
    {
        get { return _timerEnabled; }
        set { _timerEnabled = value; }
    }

    /// <value> Current number of mistake made so far.</value>
    public int errors
    {
        get { return _errorsCount; }
    }

    /// <value>Current number of mistake made so far by this specific handler (not considering child handler if any).</value>
    public int personalErrors
    {
        get { return _personalErrorCount; }
    }

    /// <value>Time spent in error state.</value>
    public float errorTime
    {
        get { return _errorTimer.time; }
    }

    /// <value>Number of errors for each grabbable.</value>
    public Dictionary<Grabbable, int> errorsPerGrabbable
    {
        get { return _grabbableErrors; }
    }

    /// <value>Time spent in error for each grabbable.</value>
    public Dictionary<Grabbable, float> errorTimePerGrabbale
    {
        get
        {
            Dictionary<Grabbable, float> times = new Dictionary<Grabbable, float>();
            foreach (var grabbable in _grabbablesErrorVirtualTimers.Keys)
            {
                times[grabbable] = _errorTimer.GetSubTimer(_grabbablesErrorVirtualTimers[grabbable]);
            }
            return times;
        }
    }

    private void Awake()
    {
        // Init the timer paused
        _errorTimer = new Timer(this);
        _errorTimer.InitPaused();

        // If parent exist
        if (_parent != null)
            _hasParent = true;
    }

    /// <summary>
    /// Register a limit to this error handler.
    /// </summary>
    /// <param name="limit">The limit to register.</param>
    public void RegisterLimit(OperatingZoneLimit limit)
    {
        _operatingZoneLimits.Add(limit);
    }

    /// <summary>
    /// Reset the handler to init state.
    /// </summary>
    public void ResetHandler()
    {
        _errorsCount = 0;
        _personalErrorCount = 0;
        _errorTimer.Reset();
        _grabbableErrors.Clear();
        _grabbableCollisions.Clear();
    }

    /// <summary>
    /// Notify a grabbable enter a limit.
    /// </summary>
    /// <param name="grabbable">The grabbable entering the limit.</param>
    /// <param name="fromChildErrorHandler">True if this call comes from a child handler. False by default.</param>
    public void NotifyGrabbableLimitEnter(Grabbable grabbable, bool fromChildErrorHandler = false)
    {
        int limitCount = 0; // Count of the current limit that are simultaneously triggered.
        _grabbableCollisions.TryGetValue(grabbable, out limitCount);
        _grabbableCollisions[grabbable] = limitCount + 1;

        bool sharedErrorState = false; // Used to sync errors between different handler child of same parent (false as default to pass the if statement)

        // If has parent notify also the parent
        if (_hasParent)
        {
            if (_parentSync)
                sharedErrorState = _parent.GrabbableInErrorState(grabbable); // Check if grabbable was already in error in the parent
            _parent.NotifyGrabbableLimitEnter(grabbable, true); // Then update with notify
        }

        // grabbable.vibrationFeedback = true; // Should move on box level
        VibrationHandler.Instance.EnableVibration(grabbable, 1f);
        EnableVisualFeedback();

        // If the grabbable is entering the limit from a non-error state (tha count was 0 before this enter notification)
        if (_grabbableCollisions[grabbable] == 1)
        {
            if (!sharedErrorState)
            {
                // Update the error count.
                _errorsCount++;

                // Is is not coming from child handler update also personal counter.
                if (!fromChildErrorHandler)
                    _personalErrorCount++;

                // Update grabbable specific error counter
                int grabbableErrors = 0;
                _grabbableErrors.TryGetValue(grabbable, out grabbableErrors);
                _grabbableErrors[grabbable] = grabbableErrors + 1;

                if (_timerEnabled)
                    _errorTimer.Play(); // Play the error timer

                // Play the subtimer if exit, otherwise create and play it.
                if (_grabbablesErrorVirtualTimers.ContainsKey(grabbable))
                {
                    if (_timerEnabled)
                        _errorTimer.PlaySubTimer(_grabbablesErrorVirtualTimers[grabbable]);
                }
                else
                {
                    _grabbablesErrorVirtualTimers[grabbable] = _errorTimer.RegisterSubTimer();
                    if (_timerEnabled)
                        _errorTimer.PlaySubTimer(_grabbablesErrorVirtualTimers[grabbable]);
                }
            }
        }
    }

    /// <summary>
    /// Notify a grabbable exiting a limit.
    /// </summary>
    /// <param name="grabbable">The grabbable exiting the limit</param>
    public void NotifyGrabbableLimitExit(Grabbable grabbable)
    {
        int limitCount = 0; // Count of the current limit that are simultaneously triggered.
        _grabbableCollisions.TryGetValue(grabbable, out limitCount);

        // If has parent notify also the parent
        if (_hasParent)
        {
            _parent.NotifyGrabbableLimitExit(grabbable);
        }

        if (limitCount == 0) // Should not happen (just for safety reasons)
        {
            // grabbable.vibrationFeedback = false;
            VibrationHandler.Instance.DisableVibration(grabbable);
            if (!SomethingStillInError())
            {
                DisableVisualFeedback();
            }
        }
        else
        {
            _grabbableCollisions[grabbable] = limitCount - 1; // Update grabbable limit counter
            if (_grabbableCollisions[grabbable] == 0) // if not limit collisions detected, set error state to false
            {
                // grabbable.vibrationFeedback = false;
                if (_timerEnabled && _grabbablesErrorVirtualTimers.ContainsKey(grabbable))
                    _errorTimer.PauseSubTimer(_grabbablesErrorVirtualTimers[grabbable]); // Puase the grabbble specific error timer

                // If nothing is still in error pause also the global error timer
                if (!SomethingStillInError())
                {
                    DisableVisualFeedback();
                    if (_timerEnabled) _errorTimer.Pause();
                }
            }
            VibrationHandler.Instance.DisableVibration(grabbable);
        }
    }

    public bool GrabbableInErrorState(Grabbable grabbable)
    {
        if (_hasParent)
            return _parent.GrabbableInErrorState(grabbable);

        int currentCollisions = 0;
        _grabbableCollisions.TryGetValue(grabbable, out currentCollisions);
        if (currentCollisions > 0)
            return true;
        return false;
    }

    /// <summary>
    /// Check if something is currently in error state.
    /// </summary>
    /// <returns>True if something is currently in error state, False otherwise.</returns>
    private bool SomethingStillInError()
    {
        foreach (var grabbable in _grabbableCollisions.Keys)
        {
            if (_grabbableCollisions[grabbable] != 0)
                return true;
        }
        return false;
    }

    private void EnableVisualFeedback()
    {
        foreach (GameObject visualFeedback in _visualFeedbackObjects)
        {
            visualFeedback.SetActive(true);
        }
    }

    private void DisableVisualFeedback()
    {
        foreach (GameObject visualFeedback in _visualFeedbackObjects)
        {
            visualFeedback.SetActive(false);
        }
    }
}
