using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Timer
{
    private MonoBehaviour _caller;
    private IEnumerator _timer;
    private float _value;
    private bool _paused;
    private bool _hasCoroutineStarted;
    private List<SubTimer> _subs = new List<SubTimer>();

    public float time
    {
        get { return _value; }
    }

    public static string Format(float value)
    {
        var milliseconds = Timer.GetMilliseconds(value);
        var seconds = Timer.GetSeconds(value);
        var minutes = Timer.GetMinutes(value);
        var hours = Timer.GetHours(value);

        if (hours > 0)
            return hours + ":" + (minutes - hours * 60) + ":" + (seconds - minutes * 60) + " h";
        else if (minutes > 0)
            return minutes + ":" + (seconds - minutes * 60) + " m";
        else if (seconds > 0)
            return seconds + " s";
        else
            return milliseconds + " ms";
    }

    public static int GetMilliseconds(float value)
    {
        return (int)(1000 * value);
    }

    public static int GetSeconds(float value)
    {
        return (int)value;
    }

    public static int GetMinutes(float value)
    {
        return (int)(value / 60);
    }

    public static int GetHours(float value)
    {
        return (int)(value / 3600);
    }

    public Timer(MonoBehaviour caller)
    {
        _caller = caller;
        _timer = TimerCoroutine();
        _value = .0f;
        _paused = true;
    }

    public void Start()
    {
        Reset();
        _paused = false;

        if (_hasCoroutineStarted)
            _caller.StopCoroutine(_timer);  // Clean coroutine status.

        _caller.StartCoroutine(_timer);
        _hasCoroutineStarted = true;
    }

    public void InitPaused()
    {
        Reset();
        _paused = true;

        if (_hasCoroutineStarted)
            _caller.StopCoroutine(_timer);  // Clean coroutine status.

        _caller.StartCoroutine(_timer);
        _hasCoroutineStarted = true;
    }

    public void Pause()
    {
        _paused = true;
    }

    public void Play()
    {
        _paused = false;
    }

    public void SwitchPauseState()
    {
        if (_paused)
            Play();
        else
            Pause();
    }

    public void Stop()
    {
        _caller.StopCoroutine(_timer);
        _hasCoroutineStarted = false;
    }

    public void Reset(bool resetSubTimers = true)
    {
        _value = .0f;
        if (resetSubTimers)
        {
            foreach (var sub in _subs)
            {
                sub.Reset();
            }
        }
    }

    public int RegisterSubTimer()
    {
        _subs.Add(new SubTimer(this));
        return _subs.Count - 1;
    }

    public void PauseSubTimer(int index)
    {
        if (index < _subs.Count)
            _subs[index].Pause();
        else
            throw new IndexOutOfRangeException("Sub timer of index " + index + " does not exist.");
    }

    public void PlaySubTimer(int index)
    {
        if (index < _subs.Count)
            _subs[index].Play();
        else
            throw new IndexOutOfRangeException("Sub timer of index " + index + " does not exist.");
    }

    public float GetSubTimer(int index)
    {
        if (index < _subs.Count)
            return _subs[index].time;
        else
            throw new IndexOutOfRangeException("Sub timer of index " + index + " does not exist.");
    }

    private IEnumerator TimerCoroutine()
    {
        _value = .0f;
        while (true)
        {
            if (!_paused)
                _value += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    private class SubTimer
    {
        Timer _original;
        float _initDelta;
        float _value;
        bool _paused;

        public float time
        {
            get { return _value; }
        }

        public SubTimer(Timer originalTimer)
        {
            _original = originalTimer;
            _value = 0f;
            _paused = true;
        }

        public void Play()
        {
            if (_paused)
            {
                _initDelta = _original.time;
                _paused = false;
            }
        }

        public void Pause()
        {
            if (!_paused)
            {
                _value += _original.time - _initDelta;
                _paused = true;
            }
        }

        public void Reset()
        {
            _initDelta = 0;
            _value = 0;
        }
    }
}

