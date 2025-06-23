using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Responsible for updating game state.
/// </summary>
public class GameStateController : Singleton<GameStateController>
{
    [Header("Tutorial")]
    public GameObject tutorial;
    [Header("Main Menu")]
    public GameObject playUI;
    public Text mainMenuHighScoreText;
    public float delayBeforeGameStarts = 1f;

    [Header("In Game")]
    public GameObject inGameUI;

    [Header("Pause")]
    public GameObject pauseMenuUI;
    public Text pauseScoreText;
    public Text pauseHighscoreText;

    [Header("Game Over")]
    public float gameOverDelay = 0.5f;
    public GameObject gameOverUI;
    public Text gameOverScoreText;
    public Text gameOverHighscoreText;

    public bool isGameOver { get; private set; }

    private Startable[] _startables;
    private Startable _playerInput;
    private GameObject _straightToPlay;
    private string _straightToPlayTag = "StraightToGame";
    private bool _hasShownTips = false;
    private MusicManager musicManager;

    protected override void Awake()
    {
        base.Awake();

        _startables = GameObject.FindObjectsByType<Startable>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var s in _startables)
        {
            if (s is MoveToTouchPosition)
            {
                _playerInput = s;
                break;
            }
        }

        _straightToPlay = GameObject.FindGameObjectWithTag(_straightToPlayTag);

        musicManager = FindFirstObjectByType<MusicManager>();
    }

    void Start()
    {
        if (_straightToPlay != null)
        {
            GoStraightIntoGame();
        }
        else
        {
            OnMainMenu();
        }
    }

    public void OnPlay()
    {
        if (SFXManager.Instance != null)
            SFXManager.Instance.PlayClick();

        if (musicManager != null)
            musicManager.PlayRandomGameTrack();

        HidePlayMenu();

        if (!_hasShownTips)
        {
            ShowTipsUI();
            _hasShownTips = true;
        }
        else
        {
            ShowInGameUI();
            StartGame();
        }
    }

    public void OnGameOver()
    {
        isGameOver = true;

        if (musicManager != null)
            musicManager.PlayGameOverMusic();

        StartCoroutine(DoGameOver());
    }

    public void OnRestart()
    {
        if (SFXManager.Instance != null)
            SFXManager.Instance.PlayClick();

        if (_straightToPlay == null)
        {
            _straightToPlay = new GameObject("Straight To Play");
            _straightToPlay.AddComponent<PersistentObject>();
            _straightToPlay.tag = _straightToPlayTag;
        }

        if (musicManager != null)
        {
            musicManager.StopMusic();
            musicManager.PlayRandomGameTrack();
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnPause()
    {
        if (SFXManager.Instance != null)
            SFXManager.Instance.PlayClick();

        StopTime();
        HideInGameUI();
        SetPauseText();
        ShowPauseMenu();
    }

    public void OnResume()
    {
        if (SFXManager.Instance != null)
            SFXManager.Instance.PlayClick();

        ShowInGameUI();
        ResumeTime();
        HidePauseMenu();
    }

    public void OnContinueAfterTutorial()
    {
        if (SFXManager.Instance != null)
            SFXManager.Instance.PlayClick(); // ✅ Sonido del botón

        tutorial.SetActive(false);
        ShowInGameUI();
        StartGame();
    }
    private void OnMainMenu()
    {
        if (musicManager != null)
            musicManager.PlayMenuMusic();

        mainMenuHighScoreText.text = Score.instance.highScore.ToString("d2");
        HideGameOverUI();
        HidePauseMenu();
        HideInGameUI();
        ShowPlayMenu();
    }

    private void GoStraightIntoGame()
    {
        ResumeTime();

        // ⚠ No mostrar tips al reiniciar
        _hasShownTips = true;

        ShowInGameUI();
        StartGame();
    }

    private IEnumerator DoGameOver()
    {
        yield return new WaitForSeconds(gameOverDelay);

        HideInGameUI();
        StopTime();

        Score.instance.CalculateHighScore();

        SetGameOverText();
        ShowGameOverUI();
    }

    private void StartGame()
    {
        StartCoroutine(DoStartAfterDelay());
    }

    private IEnumerator DoStartAfterDelay()
    {
        _playerInput.OnStart();

        yield return new WaitForSeconds(delayBeforeGameStarts);

        foreach (var s in _startables)
        {
            s.OnStart();
        }
    }

    private void StopTime()
    {
        Time.timeScale = 0f;
    }

    private void ResumeTime()
    {
        Time.timeScale = 1f;
    }

    private void SetGameOverText()
    {
        gameOverScoreText.text = Score.instance.currentScore.ToString("d2");
        gameOverHighscoreText.text = Score.instance.highScore.ToString("d2");
    }

    private void SetPauseText()
    {
        pauseScoreText.text = Score.instance.currentScore.ToString("d2");
        pauseHighscoreText.text = Score.instance.highScore.ToString("d2");
    }

    private void ShowGameOverUI()
    {
        gameOverUI.SetActive(true);
    }

    private void HideGameOverUI()
    {
        gameOverUI.SetActive(false);
    }

    private void HideInGameUI()
    {
        inGameUI.SetActive(false);
    }

    private void ShowInGameUI()
    {
        inGameUI.SetActive(true);
    }

    private void HidePauseMenu()
    {
        pauseMenuUI.SetActive(false);
    }

    private void ShowPauseMenu()
    {
        pauseMenuUI.SetActive(true);
    }

    private void HidePlayMenu()
    {
        playUI.SetActive(false);
    }

    private void ShowPlayMenu()
    {
        playUI.SetActive(true);
    }
    private void ShowTipsUI()
    {
        tutorial.SetActive(true);
    }
}