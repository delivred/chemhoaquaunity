using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Quản lý trạng thái tổng thể của game: Menu, Playing, Paused, GameOver.
/// Singleton - truy cập bằng GameManager.Instance ở bất kỳ script nào khác.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Menu, Playing, Paused, GameOver }
    public GameState CurrentState { get; private set; } = GameState.Menu;

    public enum GameDifficulty { Easy, Normal, Hard }
    public GameDifficulty CurrentDifficulty { get; private set; } = GameDifficulty.Normal;

    private static GameDifficulty lastDifficulty = GameDifficulty.Normal;
    private static bool autoRestart = false;

    [Header("Cấu hình mạng sống")]
    public int startingLives = 3;
    public int CurrentLives { get; private set; }
    [Tooltip("Nếu bật, để rơi trái cây cũng sẽ bị trừ mạng. Nếu tắt, chỉ chém trúng bom mới bị trừ mạng.")]
    public bool loseLifeOnMissFruit = false;

    [Header("Cấu hình tăng độ khó")]
    public float difficultyIncreaseInterval = 10f; // sau bao nhiêu giây thì tăng độ khó
    public float difficultyMultiplier = 1.15f;      // hệ số nhân tốc độ spawn mỗi lần tăng
    public float currentDifficultyFactor = 1f;

    private float difficultyTimer = 0f;

    void Awake()
    {
        // Thiết lập Singleton, không bị hủy khi chuyển scene (nếu cần)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (autoRestart)
        {
            autoRestart = false;
            StartGame(lastDifficulty);
        }
    }

    void Update()
    {
        if (CurrentState != GameState.Playing) return;

        // Tăng độ khó dần theo thời gian
        difficultyTimer += Time.deltaTime;
        if (difficultyTimer >= difficultyIncreaseInterval)
        {
            difficultyTimer = 0f;
            currentDifficultyFactor *= difficultyMultiplier;
        }
    }

    /// <summary>Bắt đầu game với chế độ được chỉ định: Dễ, Thường hoặc Khó.</summary>
    public void StartGame(GameDifficulty difficulty)
    {
        CurrentDifficulty = difficulty;
        lastDifficulty = difficulty;

        // Cấu hình quy tắc & độ thử thách theo từng chế độ
        switch (difficulty)
        {
            case GameDifficulty.Easy:
                startingLives = 5;              // 5 mạng, thoải mái trải nghiệm
                loseLifeOnMissFruit = false;    // Rơi quả không bị phạt
                difficultyIncreaseInterval = 15f;
                difficultyMultiplier = 1.08f;
                break;

            case GameDifficulty.Normal:
                startingLives = 3;              // 3 mạng chuẩn arcade
                loseLifeOnMissFruit = false;    // Chỉ bom mới trừ mạng
                difficultyIncreaseInterval = 10f;
                difficultyMultiplier = 1.15f;
                break;

            case GameDifficulty.Hard:
                startingLives = 3;              // 3 mạng
                loseLifeOnMissFruit = true;     // RƠI QUẢ CŨNG BỊ TRỪ MẠNG (Chuẩn Fruit Ninja Classic!)
                difficultyIncreaseInterval = 8f;
                difficultyMultiplier = 1.20f;
                break;
        }

        CurrentLives = startingLives;
        currentDifficultyFactor = 1f;
        difficultyTimer = 0f;
        CurrentState = GameState.Playing;
        Time.timeScale = 1f;

        FruitSpawner.Instance?.ApplyDifficulty(difficulty);
        ScoreManager.Instance?.ResetScore();
        UIManager.Instance?.ShowGameplayUI(difficulty);
    }

    /// <summary>Gọi khi bấm Play mặc định (chơi lại chế độ gần nhất hoặc Normal).</summary>
    public void StartGame()
    {
        StartGame(lastDifficulty);
    }

    /// <summary>Người chơi chém trúng bom hoặc để rơi trái cây (trong chế độ Khó).</summary>
    public void LoseLife(bool isBomb = true)
    {
        if (CurrentState != GameState.Playing) return;

        CurrentLives--;
        UIManager.Instance?.UpdateLives(CurrentLives, isBomb);

        if (CurrentLives <= 0)
        {
            EndGame();
        }
    }

    public void EndGame()
    {
        CurrentState = GameState.GameOver;
        UIManager.Instance?.ShowGameOver(ScoreManager.Instance.CurrentScore, CurrentDifficulty);
    }

    public void PauseGame()
    {
        if (CurrentState != GameState.Playing) return;
        CurrentState = GameState.Paused;
        Time.timeScale = 0f;
        UIManager.Instance?.ShowPausePanel(true);
    }

    public void ResumeGame()
    {
        if (CurrentState != GameState.Paused) return;
        CurrentState = GameState.Playing;
        Time.timeScale = 1f;
        UIManager.Instance?.ShowPausePanel(false);
    }

    public void RestartGame()
    {
        autoRestart = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMenu()
    {
        autoRestart = false;
        CurrentState = GameState.Menu;
        Time.timeScale = 1f;
        UIManager.Instance?.ShowMainMenu();
    }

    /// <summary>Dùng cho hiệu ứng chém trúng trái cây đá làm chậm thời gian.</summary>
    public void ActivateSlowMotion(float scale, float duration)
    {
        StopAllCoroutines();
        StartCoroutine(SlowMotionRoutine(scale, duration));
    }

    private System.Collections.IEnumerator SlowMotionRoutine(float scale, float duration)
    {
        Time.timeScale = scale;
        Time.fixedDeltaTime = 0.02f * scale;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }
}
