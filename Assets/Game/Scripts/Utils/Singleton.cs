using UnityEngine;
using System.Collections;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static bool IsDestroyed { get { return _instance == null; } }
    private static bool _applicationIsQuitting = false;

    private static readonly bool LAZY_INIT = false;
    private static T _instance = null;

    public static T instance
    {
        get
        {
            if (_applicationIsQuitting)
            {
                return _instance;
            }

            if (!_instance)
            {
                _instance = GameObject.FindAnyObjectByType<T>();

                if (!_instance)
                {
                    _instance = new GameObject().AddComponent<T>();
                    _instance.gameObject.name = _instance.GetType().Name;
                }
            }
            return _instance;
        }
    }

    protected virtual void Awake ()
    {
        if(!LAZY_INIT)
        {
            _instance = GameObject.FindFirstObjectByType<T>();
        }
        _applicationIsQuitting = false;
    }

    void OnDisable()
    {
        _applicationIsQuitting = true;
    }

    protected virtual void OnApplicationQuit()
    {
        _instance = null;
        _applicationIsQuitting = true;
    }

}
