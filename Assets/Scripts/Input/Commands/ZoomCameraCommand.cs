using UnityEngine;

public class ZoomCameraCommand : BaseCommand
{
    private float currentZoom;
    [SerializeField] private float deadzone = 0.9f;
    [SerializeField] private float maxFov = 100f;
    [SerializeField] private float minFov = 30f;
    [SerializeField] private float inputMultiplier = 10f;
    public Camera targetCamera;

    private void Awake()
    {
        if (targetCamera == null)
        {
            throw new MissingReferenceException("targetCamera must be assigned.");
        }
    }

    public override void Activate(float value)
    {
        if (Mathf.Abs(value) > deadzone)
        {
            currentZoom = targetCamera.fieldOfView;
            currentZoom -= value * inputMultiplier * Time.deltaTime;  // Increments in the FoV correspond to a zoom out, hence the minus sign.
            currentZoom = Mathf.Clamp(currentZoom, minFov, maxFov);
            targetCamera.fieldOfView = currentZoom;
        }
    }
}
