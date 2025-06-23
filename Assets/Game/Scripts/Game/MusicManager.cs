using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [Header("Menu Music")]
    [SerializeField] private AudioClip menuMusic;

    [Header("Game Music")]
    [SerializeField] private AudioClip[] gameTracks;

    [Header("Game Over Music")]
    [SerializeField] private AudioClip gameOverMusic;

    private AudioSource audioSource;
    private int lastTrackIndex = -1;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true; // Loop por defecto
    }

    public void PlayMenuMusic()
    {
        if (menuMusic == null)
        {
            Debug.LogWarning("No se asignó una música de menú.");
            return;
        }

        audioSource.Stop();
        audioSource.loop = true;
        audioSource.clip = menuMusic;
        audioSource.Play();
    }

    public void PlayRandomGameTrack()
    {
        if (gameTracks == null || gameTracks.Length == 0)
        {
            Debug.LogWarning("No se asignaron pistas de juego.");
            return;
        }

        int newIndex = Random.Range(0, gameTracks.Length);

        if (gameTracks.Length > 1)
        {
            while (newIndex == lastTrackIndex)
            {
                newIndex = Random.Range(0, gameTracks.Length);
            }
        }

        lastTrackIndex = newIndex;

        audioSource.Stop();
        audioSource.loop = true;
        audioSource.clip = gameTracks[newIndex];
        audioSource.Play();
    }

    public void PlayGameOverMusic()
    {
        if (gameOverMusic == null)
        {
            Debug.LogWarning("No se asignó una música de Game Over.");
            return;
        }

        audioSource.Stop();
        audioSource.loop = false; // ❌ No repetir
        audioSource.clip = gameOverMusic;
        audioSource.Play();
    }

    public void StopMusic()
    {
        if (audioSource.isPlaying)
            audioSource.Stop();
    }
}