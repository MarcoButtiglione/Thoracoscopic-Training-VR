using UnityEngine;

public class BaseCommand : MonoBehaviour
{
    public virtual void Activate() { }
    public virtual void Activate(float value) { }
}
