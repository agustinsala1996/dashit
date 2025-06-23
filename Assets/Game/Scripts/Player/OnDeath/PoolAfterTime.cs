using UnityEngine;
using System.Collections;

public class PoolAfterTime : MonoBehaviour
{
    public float timeInSeconds;

    private float _currentTime;

    private void OnEnable()
    {
        _currentTime = timeInSeconds;
    }

    // Update is called once per frame
    void Update()
    {
        _currentTime -= Time.deltaTime;
        if(_currentTime <= 0f)
        {
         ///   ObjectPool.instance.PoolObject(gameObject);
        }
    }
}
