using UnityEngine;
using System.Collections;

/// <summary>
/// Returns GameObject to pool when collision occurs with object that has specified tag.
/// </summary>
public class PoolWhenHitObjectWithTag : MonoBehaviour
{
    public GameObject onHitEffect;
    public AudioClip[] onHitAudioClips;
    public GameObject objectToPool;
    public string hitTag = "Projectile";

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(hitTag))
        {
            OnDeath();
        }
    }

    private void OnDeath()
    {
        // 🎵 Solo reproducir sonido si NO es un Collectable
        if (!CompareTag("Collectable") && SFXManager.Instance != null)
        {
            SFXManager.Instance.PlayEnemyDeath();
        }

        if (onHitAudioClips != null && onHitAudioClips.Length > 0)
        {
            AudioClip clip = onHitAudioClips[Random.Range(0, onHitAudioClips.Length)];
            AudioSource.PlayClipAtPoint(clip, transform.position);
        }

        if (onHitEffect != null)
        {
            SpawnDamageEffect();
        }

        ObjectPool.instance.PoolObject(objectToPool);
    }

    private void SpawnDamageEffect()
    {
        var deathEffect = ObjectPool.instance.GetObjectForType(onHitEffect.name, false);
        deathEffect.transform.position = transform.position;
        deathEffect.SetActive(true);
    }
}