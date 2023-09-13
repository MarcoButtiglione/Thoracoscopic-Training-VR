using UnityEngine;

/// <summary>
/// Inherit from this base class to create a singleton.
/// E.g.: <code>public class MyClassName : Singleton<MyClassName> {}</code>.
/// </summary>
/// <typeparam name="T">Type of the singleton.</typeparam>
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    #region  Fields
    private static T _instance;
    private static readonly object _lock = new object();

    // Used to check if the singleton is about to be destroyed.
    private static bool _quitting = false;

    [SerializeField]
    protected bool _persistent = false;
    #endregion

    #region  Properties
    public static T Instance
    {
        get
        {
            if (_quitting)
            {
                Debug.LogWarning($"[{nameof(MonoBehaviour)}<{typeof(T)}>] Instance will not be returned because the application is quitting.");
                return null;
            }
            lock (_lock)
            {
                if (_instance != null)
                    return _instance;
                var instances = FindObjectsOfType<T>();
                var count = instances.Length;
                if (count > 0)
                {
                    if (count == 1)
                        return _instance = instances[0];
                    Debug.LogWarning($"[{nameof(MonoBehaviour)}<{typeof(T)}>] There should never be more than one {nameof(MonoBehaviour)} of type {typeof(T)} in the scene, but {count} were found. The first instance found will be used, and all others will be destroyed.");
                    for (var i = 1; i < instances.Length; i++)
                        Destroy(instances[i]);
                    return _instance = instances[0];
                }

                Debug.Log($"[{nameof(MonoBehaviour)}<{typeof(T)}>] An instance is needed in the scene and no existing instances were found, so a new instance will be created.");
                return _instance = new GameObject($"({nameof(MonoBehaviour)}){typeof(T)}")
                           .AddComponent<T>();
            }
        }
    }
    #endregion

    #region  Methods
    private void Awake()
    {
        if (_persistent)
            DontDestroyOnLoad(gameObject);
        OnAwake();
    }

    private void OnApplicationQuit()
    {
        _quitting = true;
    }

    private void OnDestroy()
    {
        _quitting = true;
    }

    protected virtual void OnAwake() { }
    #endregion
}
