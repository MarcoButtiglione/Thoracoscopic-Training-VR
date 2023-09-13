using System.Collections.Generic;

public class VibrationHandler : Singleton<VibrationHandler>
{
    private Dictionary<Grabbable, int> _grabbablesDetected = new Dictionary<Grabbable, int>();

    /// <summary>
    /// Method used to enable the vibration of the controller that is grabbing the specified <see cref="Grabbable" />.
    /// </summary>
    /// <param name="grabbable"><see cref="Grabbable" /> that is being holded and whose hand needs to vibrate.</param>
    public void EnableVibration(Grabbable grabbable, float vibrationValue)
    {
        int vibrationCount = 0;
        _grabbablesDetected.TryGetValue(grabbable, out vibrationCount);
        _grabbablesDetected[grabbable] = vibrationCount + 1;
        grabbable.vibrationFeedback = vibrationValue;
    }

    /// <summary>
    /// Method used to disable the vibration of the controller that is grabbing the specified <see cref="Grabbable" />.
    /// </summary>
    /// <param name="grabbable"><see cref="Grabbable" /> that is being holded and whose hand needs stop vibrating.</param>
    public void DisableVibration(Grabbable grabbable)
    {
        int vibrationCount = 0;
        _grabbablesDetected.TryGetValue(grabbable, out vibrationCount);
        _grabbablesDetected[grabbable] = vibrationCount - 1;
        if (_grabbablesDetected[grabbable] == 0)
            grabbable.vibrationFeedback = 0f;
    }
}
