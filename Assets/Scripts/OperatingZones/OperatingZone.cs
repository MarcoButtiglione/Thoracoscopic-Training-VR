using System.Collections.Generic;
using UnityEngine;

public class OperatingZone : MonoBehaviour, IZoneInsertable
{
    // Enable/disable timers for performance purposes.
    [SerializeField] private bool _enableTimers = true;

    // The error handler of this operating zone
    private OperatingErrorsHandler _errorHandler;
    private Timer _operatingZoneTimer; // Usage timer of this operating zone
    private Dictionary<Grabbable, int> _insertedTimers = new Dictionary<Grabbable, int>(); // Grabbable specific usage timers 
    protected List<Grabbable> _insertedGrabbables = new List<Grabbable>(); // Currently inserted grabbables
    private int _inserted = 0; // Inserted grabbables counter

    /// <value>Errors count</value>
    public int errors
    {
        get
        {
            if (_errorHandler != null)
                return _errorHandler.errors;
            else return 0;
        }
    }

    /// <value>Usage time of this operating zone</value>
    public float usageTime
    {
        get { return _operatingZoneTimer.time; }
    }

    /// <value>Grabbable specific usage time for this operating zone</value>
    public Dictionary<Grabbable, float> usageTimePerGrabbable
    {
        get
        {
            Dictionary<Grabbable, float> times = new Dictionary<Grabbable, float>();
            foreach (var grabbable in _insertedTimers.Keys)
            {
                times[grabbable] = _operatingZoneTimer.GetSubTimer(_insertedTimers[grabbable]);
            }
            return times;
        }
    }

    /// <value>Grabbable specific error time for this operating zone</value>
    public Dictionary<Grabbable, float> errorTimePerGrabbale
    {
        get
        {
            if (_errorHandler != null)
                return _errorHandler.errorTimePerGrabbale;
            else
                return new Dictionary<Grabbable, float>();
        }
    }

    /// <value>Error time for this operating zone</value>
    public float errorTime
    {
        get
        {
            if (_errorHandler != null)
                return _errorHandler.errorTime;
            else
                return 0;
        }
    }

    /// <value>Grabbable specific errors for this operating zone</value>
    public Dictionary<Grabbable, int> errorsPerGrabbable
    {
        get
        {
            if (_errorHandler != null)
                return _errorHandler.errorsPerGrabbable;
            else
                return new Dictionary<Grabbable, int>();
        }
    }

    protected virtual void Awake()
    {
        _operatingZoneTimer = new Timer(this);
        _operatingZoneTimer.InitPaused();

        _errorHandler = GetComponentInChildren<OperatingErrorsHandler>();
        if (_errorHandler != null)
            _errorHandler.enableTimer = _enableTimers;
    }

    /// <summary>
    /// Reset this operating zone to init specs
    /// </summary>
    public void ResetZone()
    {
        _operatingZoneTimer.Reset();
        if (_errorHandler != null)
            _errorHandler.ResetHandler();
    }

    public virtual void Insert(Grabbable grabbable)
    {
        _insertedGrabbables.Add(grabbable); // Add the grabbable to the current inserted list
        _inserted++; // Update counter
        if (_enableTimers)
            _operatingZoneTimer.Play();
        if (_insertedTimers.ContainsKey(grabbable))
        {
            if (_enableTimers)
                _operatingZoneTimer.PlaySubTimer(_insertedTimers[grabbable]);
        }
        else
        {
            _insertedTimers[grabbable] = _operatingZoneTimer.RegisterSubTimer(); // Register the virtual sub timer
            if (_enableTimers)
                _operatingZoneTimer.PlaySubTimer(_insertedTimers[grabbable]);
        }
    }

    public virtual void Remove(Grabbable grabbable)
    {
        _insertedGrabbables.Remove(grabbable); // Remove the grabbable form the list
        _inserted--;
        if (_inserted == 0)
            _operatingZoneTimer.Pause();
        if (_insertedTimers.ContainsKey(grabbable))
        {
            _operatingZoneTimer.PauseSubTimer(_insertedTimers[grabbable]);
        }
    }

}
