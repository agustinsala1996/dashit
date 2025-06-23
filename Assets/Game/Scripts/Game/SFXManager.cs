using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;

    public AudioClip clickSound;
    public AudioClip dashSound;
    public AudioClip collectSound;
    public AudioClip enemyDeathSound;
    public AudioClip enemyShootSound; // 🔊 Nuevo sonido


    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void PlayClick()
    {
        if (clickSound != null)
            audioSource.PlayOneShot(clickSound);
    }

    public void PlayDash()
    {
        if (dashSound != null)
            audioSource.PlayOneShot(dashSound);
    }

    public void PlayCollect()
    {
        if (collectSound != null)
            audioSource.PlayOneShot(collectSound);
    }

    public void PlayEnemyDeath()
    {
        if (enemyDeathSound != null)
            audioSource.PlayOneShot(enemyDeathSound);
    }

    public void PlayEnemyShoot()
    {
        if (enemyShootSound != null)
            audioSource.PlayOneShot(enemyShootSound);
    }
}