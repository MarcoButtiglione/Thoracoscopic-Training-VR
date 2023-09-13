using UnityEngine;

public class DisplayUpdateFrequency : MonoBehaviour
{
    private void Awake()
    {
        // float[] freqs = OVRManager.display.displayFrequenciesAvailable;
        // Debug.LogWarning("Available frequencies: " + freqs);
        OVRManager.display.displayFrequency = 90.0f;
    }
}
