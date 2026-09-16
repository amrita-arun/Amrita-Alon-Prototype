using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Central game state owner:
///   - Runs the 3-2-1 countdown on scene load and locks players until it finishes.
///   - Owns the win/loss condition (a player hitting minimum size loses).
///   - Shows the game-over overlay with the winner.
///   - Handles scene restart (merged in from the old SceneRestarter/ResetScene script).
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Countdown, Playing, GameOver }
    public GameState State { get; private set; } = GameState.Countdown;

    /// <summary>Convenience flag other scripts (movement, eating) can check.</summary>
    public bool IsPlaying => State == GameState.Playing;

    [Header("Player Tags")]
    [Tooltip("Must match the tag used on the circle player object.")]
    public string circleTag = "Circle";
    [Tooltip("Must match the tag used on the triangle player object.")]
    public string triangleTag = "Triangle";

    [Header("Countdown UI")]
    public TextMeshProUGUI countdownText;
    public float countdownStart = 3f;
    public string goText = "GO!";
    public float goDisplayTime = 0.5f;

    [Header("Game Over UI")]
    [Tooltip("Allows the death animation to play before showing the winner.")]
public float gameOverDisplayDelay = 0.45f;  
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverText;
    public Button restartButton;

    [Header("Restart")]
    public KeyCode restartKey = KeyCode.R;

    private GameObject circlePlayer;
    private GameObject trianglePlayer;
    private PlayerMovement circleMovement;
    private PlayerMovement triangleMovement;

    

    [Header("Stalemate Detection")]
    [Tooltip("How long large players must remain touching before the match ends.")]
    public float stalemateDuration = 3f;

    [Tooltip("Prevents ordinary early-game collisions from causing a stalemate.")]
    public float minimumCombinedSizeForStalemate = 10f;

    [Tooltip("If their sizes are this close, the result is a draw.")]
    public float tieSizeTolerance = 0.05f;

    private float stalemateTimer;
    private Vector3 previousCirclePosition;
    private Vector3 previousTrianglePosition;
    void Awake()
    {
        // Scene-local singleton. Intentionally NOT DontDestroyOnLoad — on restart we
        // want a brand new GameManager with fresh state, not a carried-over instance.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        circlePlayer = GameObject.FindGameObjectWithTag(circleTag);
        trianglePlayer = GameObject.FindGameObjectWithTag(triangleTag);

        if (circlePlayer != null) circleMovement = circlePlayer.GetComponent<PlayerMovement>();
        if (trianglePlayer != null) triangleMovement = trianglePlayer.GetComponent<PlayerMovement>();

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (restartButton != null) restartButton.onClick.AddListener(RestartLevel);

        SetPlayersFrozen(true);
        StartCoroutine(CountdownRoutine());

        if (circlePlayer != null)
        {
            previousCirclePosition = circlePlayer.transform.position;
        }

        if (trianglePlayer != null)
        {
            previousTrianglePosition = trianglePlayer.transform.position;
        }
    }
    void Update()
    {
        if (Input.GetKeyDown(restartKey))
        {
            RestartLevel();
            return;
        }

        CheckForStalemate();
    }

    private IEnumerator CountdownRoutine()
    {
        State = GameState.Countdown;

        float count = countdownStart;
        while (count > 0f)
        {
            if (countdownText != null)
            {
                countdownText.gameObject.SetActive(true);
                countdownText.text = Mathf.Ceil(count).ToString();
                if(GameSFX.Instance != null) GameSFX.Instance.PlayCountdown();
            }
            yield return new WaitForSeconds(1f);
            count -= 1f;
        }

        if (countdownText != null)
        {
            countdownText.text = goText;
            if(GameSFX.Instance != null) GameSFX.Instance.PlayGO();
        }
        yield return new WaitForSeconds(goDisplayTime);

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }

        SetPlayersFrozen(false);
        State = GameState.Playing;
    }

    private void SetPlayersFrozen(bool frozen)
    {
        // Disabling PlayerMovement stops input from being read and, via its own
        // OnDisable(), zeroes the Rigidbody's velocity — no movement script edits needed.
        if (circleMovement != null) circleMovement.enabled = !frozen;
        if (triangleMovement != null) triangleMovement.enabled = !frozen;
    }


   
    /// <summary>
    /// Called by PlayerEat when a player's size hits its minimum. GameManager decides
    /// what that means for the match (the OTHER tag wins).
    /// </summary>
    public void PlayerReachedMinimumSize(string losingTag)
    {
        if (State != GameState.Playing) return; // ignore duplicate/late reports

        string winningTag = losingTag == circleTag ? triangleTag : circleTag;
        EndGame(winningTag);
    }

    private void EndGame(string winningTag)
    {
        State = GameState.GameOver;
        SetPlayersFrozen(true);

        StartCoroutine(
            ShowGameOverAfterDelay(winningTag)
        );
    }

    /// <summary>Reloads the current scene (merged in from ResetScene.cs).</summary>
    public void RestartLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }

    private void CheckForStalemate()
    {
        if (State != GameState.Playing)
        {
            stalemateTimer = 0f;
            return;
        }

        if (circlePlayer == null || trianglePlayer == null)
        {
            stalemateTimer = 0f;
            return;
        }

        float circleSize = circlePlayer.transform.localScale.x;
        float triangleSize = trianglePlayer.transform.localScale.x;
        float combinedSize = circleSize + triangleSize;

        bool playersAreLargeEnough =
            combinedSize >= minimumCombinedSizeForStalemate;

        bool stalemateIsHappening =
            playersAreLargeEnough &&
            PlayerCollisionJuice.PlayersAreTouching;

        if (!stalemateIsHappening)
        {
            stalemateTimer = 0f;
            return;
        }

        stalemateTimer += Time.deltaTime;

        if (stalemateTimer < stalemateDuration)
            return;

        if (Mathf.Abs(circleSize - triangleSize) <= tieSizeTolerance)
        {
            TriggerStalemateEnd("", null);
        }
        else if (circleSize > triangleSize)
        {
            TriggerStalemateEnd(circleTag, trianglePlayer);
        }
        else
        {
            TriggerStalemateEnd(triangleTag, circlePlayer);
        }
    }

    private void TriggerStalemateEnd(
    string winningTag,
    GameObject losingPlayer
    )
    {
        EndGame(winningTag);

        if (losingPlayer == null)
            return;

        PlayerDeathJuice deathJuice =
            losingPlayer.GetComponent<PlayerDeathJuice>();

        if (deathJuice != null)
        {
            deathJuice.BeginDeath();
        }
        else
        {
            Destroy(losingPlayer);
        }
    }

    private IEnumerator ShowGameOverAfterDelay(
        string winningTag
    )
    {
        yield return new WaitForSeconds(
            gameOverDisplayDelay
        );

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (gameOverText != null)
        {
            gameOverText.text =
                string.IsNullOrEmpty(winningTag)
                    ? "Draw!"
                    : $"{winningTag} Wins!";
        }
    }
    
}

