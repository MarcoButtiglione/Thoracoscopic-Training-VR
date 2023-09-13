using System;
using System.Collections.Generic;
using UnityEngine;

public class OperatingBox : MonoBehaviour
{
    // Enable/disable timers for performance purposes.
    [SerializeField] private OperatingErrorsHandler _errorHandler = null;
    [SerializeField] private bool enableBoxErrorTimer = true;
    private OperatingZone[] _operatingZones;

    private void Awake()
    {
        _operatingZones = GetComponentsInChildren<OperatingZone>();
        if (_errorHandler != null)
            _errorHandler.enableTimer = enableBoxErrorTimer;
    }

    public void ResetBox()
    {
        if (_errorHandler != null)
            _errorHandler.ResetHandler();
        foreach (var zone in _operatingZones)
        {
            zone.ResetZone();
        }
    }

    public int GetErrors()
    {
        if (_errorHandler != null)
            return _errorHandler.errors;
        else
            return 0;
    }

    public float GetErrorTime()
    {
        if (_errorHandler != null)
            return _errorHandler.errorTime;
        else
            return 0;
    }

    public List<Tuple<string, float, int, float>> GetPerOperatingZoneUsageTimeAndErrorsAndErrorTime()
    {
        List<Tuple<string, float, int, float>> final = new List<Tuple<string, float, int, float>>();
        foreach (var zone in _operatingZones)
        {
            final.Add(new Tuple<string, float, int, float>(zone.name, zone.usageTime, zone.errors, zone.errorTime));
        }
        return final;
    }

    public Dictionary<string, int> GetErrorsPerObject()
    {
        Dictionary<string, int> final = new Dictionary<string, int>();
        Dictionary<Grabbable, int> errors;

        if (_errorHandler != null)
            errors = _errorHandler.errorsPerGrabbable;
        else
            errors = new Dictionary<Grabbable, int>();

        foreach (var grabbable in errors.Keys)
        {
            final[grabbable.name] = errors[grabbable];
        }
        return final;
    }

    public Dictionary<string, int> GetErrorsPerOperatingZone()
    {
        Dictionary<string, int> final = new Dictionary<string, int>();
        foreach (var zone in _operatingZones)
        {
            final[zone.name] = zone.errors;
        }
        return final;
    }

    public Dictionary<string, int> GetErrorsPerOperatingZoneAndObject()
    {
        Dictionary<string, int> final = new Dictionary<string, int>();
        foreach (var zone in _operatingZones)
        {
            var errors = zone.errorsPerGrabbable;
            foreach (var grabbable in errors.Keys)
            {
                final[zone.name + ": " + grabbable.name] = errors[grabbable];
            }
        }
        return final;
    }

    public Dictionary<string, string> GetOperatingZoneUsageTime()
    {
        Dictionary<string, string> final = new Dictionary<string, string>();
        foreach (var zone in _operatingZones)
        {
            final[zone.name] = Timer.Format(zone.usageTime);
        }
        return final;
    }

    public Dictionary<string, string> GetPerObjectUsageTime()
    {
        Dictionary<Grabbable, float> times = new Dictionary<Grabbable, float>();
        foreach (var zone in _operatingZones)
        {
            var partialTimes = zone.usageTimePerGrabbable;
            foreach (var grabbable in partialTimes.Keys)
            {
                float partial = 0;
                times.TryGetValue(grabbable, out partial);
                times[grabbable] = partial + partialTimes[grabbable];
            }
        }

        Dictionary<string, string> final = new Dictionary<string, string>();
        foreach (var grabbable in times.Keys)
        {
            final[grabbable.name] = Timer.Format(times[grabbable]);
        }
        return final;
    }

    public Dictionary<string, string> GetPerOperatingZoneAndObjectUsageTime()
    {
        Dictionary<string, string> final = new Dictionary<string, string>();

        foreach (var zone in _operatingZones)
        {
            var times = zone.usageTimePerGrabbable;
            foreach (var grabbable in times.Keys)
            {
                final[zone.name + ": " + grabbable.name] = Timer.Format(times[grabbable]);
            }
        }
        return final;
    }

    public Dictionary<string, string> GetOperatingZoneErrorTime()
    {
        Dictionary<string, string> final = new Dictionary<string, string>();
        foreach (var zone in _operatingZones)
        {
            final[zone.name] = Timer.Format(zone.errorTime);
        }
        return final;
    }

    public Dictionary<string, string> GetPerObjectErrorTime()
    {
        Dictionary<Grabbable, float> times = new Dictionary<Grabbable, float>();
        foreach (var zone in _operatingZones)
        {
            var partialTimes = zone.errorTimePerGrabbale;
            foreach (var grabbable in partialTimes.Keys)
            {
                float partial = 0;
                times.TryGetValue(grabbable, out partial);
                times[grabbable] = partial + partialTimes[grabbable];
            }
        }

        Dictionary<string, string> final = new Dictionary<string, string>();
        foreach (var grabbable in times.Keys)
        {
            final[grabbable.name] = Timer.Format(times[grabbable]);
        }
        return final;
    }

    public Dictionary<string, string> GetPerOperatingZoneAndObjectErrorTime()
    {
        Dictionary<string, string> final = new Dictionary<string, string>();

        foreach (var zone in _operatingZones)
        {
            var times = zone.errorTimePerGrabbale;
            foreach (var grabbable in times.Keys)
            {
                final[zone.name + ": " + grabbable.name] = Timer.Format(times[grabbable]);
            }
        }
        return final;
    }

    public Tuple<string, float> GetMostUsedOperatingZone()
    {
        float maxTime = 0;
        string result = "";
        foreach (var zone in _operatingZones)
        {
            if (zone.usageTime > maxTime)
            {
                maxTime = zone.usageTime;
                result = zone.name;
            }
        }
        if (result != "")
            return new Tuple<string, float>(result, maxTime);
        return new Tuple<string, float>("None", 0.0f);
    }

    public Tuple<string, int> GetOperatingZoneWithMostErrors()
    {
        int maxErrors = 0;
        string result = "";
        foreach (var zone in _operatingZones)
        {
            if (zone.errors > maxErrors)
            {
                maxErrors = zone.errors;
                result = zone.name;
            }
        }

        if (_errorHandler != null)
        {
            if (_errorHandler.personalErrors > maxErrors)
            {
                maxErrors = _errorHandler.personalErrors;
                result = "Box margin";
            }
        }

        if (result != "")
            return new Tuple<string, int>(result, maxErrors);
        return new Tuple<string, int>("None", 0);
    }

}
