using UnityEngine;

public class EnableComponentCommand : BaseCommand
{
    private float deadzone = 0.2f;
    private bool isBeingPressed = false;

    /// <summary>
    /// Array containing the objects whose components must be enabled/disabled when the command is activated.
    /// </summary>
    public GameObject[] gameObjects;

    public override void Activate()
    {
        foreach (GameObject gameObject in gameObjects)
        {
            gameObject.SetActive(!gameObject.activeSelf);
        }
    }

    public override void Activate(float value)
    {
        if (value > deadzone && !isBeingPressed)
        {
            isBeingPressed = true;
            foreach (GameObject gameObject in gameObjects)
            {
                gameObject.SetActive(!gameObject.activeSelf);
            }
        }
        else if (value <= deadzone)
        {
            isBeingPressed = false;
        }
    }
}
