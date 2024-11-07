using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Sonidos
    public AudioClip playSound;
    public AudioClip tokenClickSound;
    public AudioClip correctPairSound;
    public AudioClip incorrectPairSound;
    public AudioClip newBestScoreSound;
    public AudioClip gameEndSound; // Sonido para el final de la partida sin nuevo récord

    private AudioSource audioSource;

    public GameObject[] tokens;
    public Canvas mainCanvas;
    public Canvas gameCanvas;
    public Canvas winCanvas;
    public Button playButton;
    public Button restartButton;

    public TMP_Text attemptsText;
    public TMP_Text timerText;
    public TMP_Text bestScoreText;
    public TMP_Text finalTimeText;
    public TMP_Text winMessageText;

    private Token firstTokenClicked;
    private Token secondTokenClicked;
    private float checkDelay = 2f;
    private float timer = 0f;
    private bool isChecking = false;
    private bool gameStarted = false;
    private int attempts = 0;
    private float gameTimer = 0f;
    private float bestScore = -1f;

    void Start()
    {
        // Configura el AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (playButton != null) playButton.onClick.AddListener(StartGame);
        if (restartButton != null) restartButton.onClick.AddListener(RestartGame);

        mainCanvas.enabled = true;
        gameCanvas.enabled = false;
        winCanvas.enabled = false;

        LoadBestScore();
        ToggleTokens(false);
    }

    void Update()
    {
        if (!gameStarted) return;

        gameTimer += Time.deltaTime;
        timerText.text = "Time: " + ((int)gameTimer) + " s";

        if (isChecking)
        {
            timer += Time.deltaTime;

            if (timer >= checkDelay)
            {
                CheckTokens();
                ResetCheck();
            }
        }

        if (CheckGameCompleted())
        {
            EndGame();
        }
    }

    public void OnTokenClicked(Token clickedToken)
    {
        if (!gameStarted || isChecking) return;

        // Reproduce el sonido de clic en el token
        PlaySound(tokenClickSound);

        if (firstTokenClicked == null)
        {
            firstTokenClicked = clickedToken;
        }
        else if (secondTokenClicked == null && firstTokenClicked != clickedToken)
        {
            secondTokenClicked = clickedToken;
            isChecking = true;
            timer = 0f;

            attempts++;
            attemptsText.text = "Attempts: " + attempts;
        }
    }

    private void CheckTokens()
    {
        if (firstTokenClicked.CompareToken(secondTokenClicked))
        {
            Destroy(firstTokenClicked.gameObject);
            Destroy(secondTokenClicked.gameObject);

            PlaySound(correctPairSound); // Sonido para pareja correcta

            List<GameObject> validTokens = new List<GameObject>();
            foreach (var token in tokens)
            {
                if (token != null)
                {
                    validTokens.Add(token);
                }
            }
            tokens = validTokens.ToArray();

            if (CheckGameCompleted())
            {
                EndGame();
            }
        }
        else
        {
            firstTokenClicked.HideToken();
            secondTokenClicked.HideToken();

            PlaySound(incorrectPairSound); // Sonido para pareja incorrecta
        }
    }

    private void ResetCheck()
    {
        firstTokenClicked = null;
        secondTokenClicked = null;
        isChecking = false;
        timer = 0f;
    }

    private void StartGame()
    {
        mainCanvas.enabled = false;
        gameCanvas.enabled = true;
        winCanvas.enabled = false;
        gameStarted = true;
        ToggleTokens(true);
        ResetGameStats();

        PlaySound(playSound); // Sonido al comenzar el juego
    }

    private void ToggleTokens(bool state)
    {
        foreach (var token in tokens)
        {
            if (token != null)
            {
                var tokenCollider = token.GetComponentInChildren<BoxCollider>();
                if (tokenCollider != null)
                {
                    tokenCollider.enabled = state;
                }
            }
        }
    }

    private void ResetGameStats()
    {
        attempts = 0;
        attemptsText.text = "Attempts: " + attempts;
        gameTimer = 0f;
        timerText.text = "Time: 0 s";
    }

    private void EndGame()
    {
        gameStarted = false;
        ToggleTokens(false);

        bool newBestScore = bestScore < 0 || gameTimer < bestScore;

        if (newBestScore)
        {
            bestScore = gameTimer;
            bestScoreText.text = "Best Time: " + ((int)bestScore) + " s";
            SaveBestScore();
            winMessageText.text = "Congratulations! New Best Time!";

            PlaySound(newBestScoreSound); // Sonido para nuevo mejor tiempo
        }
        else
        {
            winMessageText.text = "Good Job!";

            PlaySound(gameEndSound); // Sonido para fin de partida sin nuevo récord
        }

        finalTimeText.text = "Final Time: " + ((int)gameTimer) + " s";
        winCanvas.enabled = true;
    }

    private bool CheckGameCompleted()
    {
        foreach (var token in tokens)
        {
            if (token != null) return false;
        }
        return true;
    }

    private void LoadBestScore()
    {
        bestScore = PlayerPrefs.GetFloat("BestTime", -1);
        if (bestScore >= 0)
        {
            bestScoreText.text = "Best Time: " + ((int)bestScore) + " s";
        }
        else
        {
            bestScoreText.text = "Best Time: --";
        }
    }

    private void SaveBestScore()
    {
        PlayerPrefs.SetFloat("BestTime", bestScore);
        PlayerPrefs.Save();
    }

    private void RestartGame()
    {
        winCanvas.enabled = false;
        gameCanvas.enabled = true;
        mainCanvas.enabled = false;

        gameStarted = true;
        ToggleTokens(true);
        ResetGameStats();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
