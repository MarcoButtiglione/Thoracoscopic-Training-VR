using System;
using UnityEngine;

public class InputDispatcher : MonoBehaviour
{
    /// <summary>
    /// 
    /// </summary>
    public CommandAssociation[] commands;


    /// <summary>
    /// Dispatch an input event triggering the associated command if any.
    /// </summary>
    /// <param name="control">Input control to dispatch</param>
    public void DispatchInput(Control control)
    {
        // FindAll returns an empty array if it doesn't find an element matching the predicate.
        CommandAssociation[] matchingCommands = Array.FindAll(commands, element => element.control == control);
        if (matchingCommands.Length > 0)
        {
            foreach (CommandAssociation command in matchingCommands)
            {
                command.Activate();
            }
        }
    }

    /// <summary>
    /// Dispatch an input value triggering the associated command if any.
    /// </summary>
    /// <param name="control">Input control to dispatch</param>
    /// <param name="value">Input value to dispatch</param>
    public void DispatchInputValue(Control control, float value)
    {
        // FindAll returns an empty array if it doesn't find an element matching the predicate.
        CommandAssociation[] matchingCommands = Array.FindAll(commands, element => element.control == control);
        if (matchingCommands.Length > 0)
        {
            foreach (CommandAssociation command in matchingCommands)
            {
                command.Activate(value);
            }
        }
    }

}
