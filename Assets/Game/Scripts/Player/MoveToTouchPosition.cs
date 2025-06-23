using UnityEngine;
using UnityEngine.EventSystems; // ✅ Necesario para detectar UI

public class MoveToTouchPosition : Startable
{
    public float moveUnitsPerSecond = 5f;

    private bool _shouldMove = false;

    public override void OnStart()
    {
        _shouldMove = true;
    }

    void Update()
    {
        if (_shouldMove && !IsPointerOverUI())
        {
            Vector3 target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            target.z = transform.position.z;

            transform.position = Vector3.MoveTowards(transform.position, target, moveUnitsPerSecond * Time.deltaTime);
        }
    }

    /// <summary>
    /// Detecta si el mouse o dedo está sobre un elemento de UI.
    /// Compatible con PC y dispositivos táctiles.
    /// </summary>  
    bool IsPointerOverUI()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount > 0)
            return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        else
            return false;
#else
        return EventSystem.current.IsPointerOverGameObject();
#endif
    }
}