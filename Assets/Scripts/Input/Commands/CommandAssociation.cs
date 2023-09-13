using System;
using UnityEngine;

[Serializable]
public struct CommandAssociation
{
    public Control control;
    public BaseCommand command;

    public void Activate()
    {
        command.Activate();
    }

    public void Activate(float value)
    {
        command.Activate(value);
    }
}

public enum Control
{
    IndexTrigger,
    HandTrigger,
    ThumbstickHorizontal,
    ThumbstickVertical,
    ButtonThumbstick,
    ButtonSouth,
    ButtonNorth
}
