using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public GameObject playerDamageEffect;
    public int hitPoints = 10;

    private float _currentScale;
    private float _scaleMultiplier;
    private Vector2 _initialScale;
    private Vector2 _minimumScale;
    private bool _isDead;

    private void Start()
    {
        _isDead = false;
        _initialScale = transform.localScale;
        _minimumScale = _initialScale * 0.6f;
    }

    public void ApplyDamage()
    {
        if (!_isDead)
        {
            SpawnDamageEffect();
            ScaleDown();
        }
    }

    private void OnEnable()
    {
        _currentScale = 0f;
        _scaleMultiplier = 1f / hitPoints;
    }

    private void SpawnDamageEffect()
    {
    ///    var deathEffect = ObjectPool.instance.GetObjectForType(playerDamageEffect.name, false);
    ///    deathEffect.transform.position = transform.position;
    ///    deathEffect.SetActive(true);
    }

    private void ScaleDown ()
    {
        _currentScale += _scaleMultiplier;
        transform.localScale = Vector2.Lerp(_initialScale, _minimumScale, _currentScale);

        if(_currentScale >= 1f)
        {
            OnDead();
        }
    }

    private void OnDead()
    {
        _isDead = true;

        GameStateController.instance.OnGameOver();
        Destroy(gameObject);
    }

}
