using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Quản lý các panel UI: Menu chính, Gameplay HUD, Pause, Game Over.
/// Gắn vào 1 GameObject "UIManager" trong Canvas, kéo thả các panel/text vào Inspector.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject gameplayHUD;
    public GameObject pausePanel;
    public GameObject gameOverPanel;

    [Header("HUD Elements")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI comboText;
    public Transform livesContainer;      // chứa các icon trái tim
    public GameObject heartIconPrefab;    // prefab icon 1 mạng

    [Header("Game Over Elements")]
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI newRecordBadge; // huy hiệu "KỶ LỤC MỚI!" (tùy chọn)

    [Header("Main Menu Elements (Tùy chọn)")]
    public TextMeshProUGUI menuHighScoreText;

    [Header("Floating score popup (tùy chọn)")]
    public GameObject floatingScorePrefab; // prefab TextMeshPro world space
    public Transform floatingScoreSpawnPoint;

    private GameObject[] heartIcons;
    private Coroutine comboAnimCoroutine;
    private Coroutine scoreCountCoroutine;
    private Vector3 initialScoreScale = Vector3.one;
    private Vector3 initialComboScale = Vector3.one;

    private const string HIGH_SCORE_KEY = "FruitNinja_HighScore";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (scoreText != null) initialScoreScale = scoreText.transform.localScale;
        if (comboText != null) initialComboScale = comboText.transform.localScale;
    }

    void Start()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        SetActiveSafe(mainMenuPanel, true);
        SetActiveSafe(gameplayHUD, false);
        SetActiveSafe(pausePanel, false);
        SetActiveSafe(gameOverPanel, false);

        int highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
        if (menuHighScoreText != null)
        {
            menuHighScoreText.text = $"★ BEST: {highScore}";
        }
    }

    public void ShowGameplayUI()
    {
        SetActiveSafe(mainMenuPanel, false);
        SetActiveSafe(gameplayHUD, true);
        SetActiveSafe(pausePanel, false);
        SetActiveSafe(gameOverPanel, false);

        SetupLivesUI(GameManager.Instance != null ? GameManager.Instance.startingLives : 3);
        UpdateScore(0);
        UpdateCombo(0);
    }

    public void ShowPausePanel(bool show)
    {
        SetActiveSafe(pausePanel, show);
    }

    public void ShowGameOver(int finalScore)
    {
        SetActiveSafe(gameplayHUD, false);
        SetActiveSafe(gameOverPanel, true);

        int oldHighScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
        bool isNewRecord = finalScore > oldHighScore;
        int displayHighScore = oldHighScore;

        if (isNewRecord)
        {
            displayHighScore = finalScore;
            PlayerPrefs.SetInt(HIGH_SCORE_KEY, finalScore);
            PlayerPrefs.Save();
        }

        if (newRecordBadge != null)
        {
            newRecordBadge.gameObject.SetActive(isNewRecord && finalScore > 0);
        }

        if (highScoreText != null)
        {
            highScoreText.text = isNewRecord && finalScore > 0 ? $"★ {displayHighScore} ★" : $"{displayHighScore}";
        }

        // Hiệu ứng tăng điểm từ 0 -> finalScore
        if (scoreCountCoroutine != null) StopCoroutine(scoreCountCoroutine);
        scoreCountCoroutine = StartCoroutine(AnimateScoreCount(finalScore));
    }

    private IEnumerator AnimateScoreCount(int targetScore)
    {
        if (finalScoreText == null) yield break;

        float duration = Mathf.Min(1.0f, 0.3f + targetScore * 0.02f);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // Ease out cubic
            float ease = 1f - Mathf.Pow(1f - t, 3f);
            int current = Mathf.RoundToInt(targetScore * ease);
            finalScoreText.text = $"{current}";
            yield return null;
        }

        finalScoreText.text = $"{targetScore}";

        // Nháy to nhẹ khi hoàn tất
        Vector3 orig = finalScoreText.transform.localScale;
        finalScoreText.transform.localScale = orig * 1.15f;
        float punchTime = 0.2f;
        float pe = 0f;
        while (pe < punchTime)
        {
            pe += Time.unscaledDeltaTime;
            finalScoreText.transform.localScale = Vector3.Lerp(orig * 1.15f, orig, pe / punchTime);
            yield return null;
        }
        finalScoreText.transform.localScale = orig;
    }

    public void UpdateScore(int score)
    {
        if (scoreText == null) return;
        scoreText.text = score.ToString();

        // Subtle punch scale
        if (gameObject.activeInHierarchy && score > 0)
        {
            StartCoroutine(PunchScaleRoutine(scoreText.transform, initialScoreScale, 1.15f, 0.15f));
        }
    }

    public void UpdateCombo(int combo)
    {
        if (comboText == null) return;

        if (combo > 1)
        {
            comboText.gameObject.SetActive(true);
            comboText.text = $"COMBO x{combo}!";

            if (comboAnimCoroutine != null) StopCoroutine(comboAnimCoroutine);
            comboAnimCoroutine = StartCoroutine(ComboPunchRoutine());
        }
        else
        {
            comboText.gameObject.SetActive(false);
        }
    }

    private IEnumerator ComboPunchRoutine()
    {
        if (comboText == null) yield break;
        Transform t = comboText.transform;
        t.localScale = initialComboScale * 1.4f;

        float dur = 0.25f;
        float e = 0f;
        while (e < dur)
        {
            e += Time.deltaTime;
            t.localScale = Vector3.Lerp(initialComboScale * 1.4f, initialComboScale, e / dur);
            yield return null;
        }
        t.localScale = initialComboScale;
    }

    private IEnumerator PunchScaleRoutine(Transform target, Vector3 normalScale, float punchFactor, float duration)
    {
        if (target == null) yield break;
        target.localScale = normalScale * punchFactor;
        float e = 0f;
        while (e < duration)
        {
            e += Time.deltaTime;
            target.localScale = Vector3.Lerp(normalScale * punchFactor, normalScale, e / duration);
            yield return null;
        }
        target.localScale = normalScale;
    }

    private void SetupLivesUI(int lives)
    {
        if (livesContainer == null) return;

        foreach (Transform child in livesContainer) Destroy(child.gameObject);

        // Nạp prefab nếu chưa gán
        if (heartIconPrefab == null)
        {
            heartIconPrefab = Resources.Load<GameObject>("Prefabs/HeartIcon");
        }

        heartIcons = new GameObject[lives];
        for (int i = 0; i < lives; i++)
        {
            if (heartIconPrefab != null)
            {
                heartIcons[i] = Instantiate(heartIconPrefab, livesContainer);
            }
            else
            {
                // Fallback tạo nhanh GameObject Image hình trái tim
                GameObject h = new GameObject($"Heart_{i}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                h.transform.SetParent(livesContainer, false);
                RectTransform rt = h.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(64, 64);
                heartIcons[i] = h;
            }
        }
    }

    public void UpdateLives(int lives)
    {
        if (heartIcons == null) return;

        for (int i = 0; i < heartIcons.Length; i++)
        {
            if (heartIcons[i] != null)
            {
                // Khi mất mạng, làm mờ đi hoặc ẩn
                Image img = heartIcons[i].GetComponent<Image>();
                if (img != null)
                {
                    img.color = i < lives ? Color.white : new Color(0.3f, 0.3f, 0.3f, 0.4f);
                }
                else
                {
                    heartIcons[i].SetActive(i < lives);
                }
            }
        }
    }

    public void ShowFloatingScore(int amount)
    {
        if (floatingScorePrefab == null || floatingScoreSpawnPoint == null) return;

        GameObject popup = Instantiate(floatingScorePrefab, floatingScoreSpawnPoint.position, Quaternion.identity);
        TextMeshPro tmp = popup.GetComponent<TextMeshPro>();
        if (tmp != null) tmp.text = $"+{amount}";
        Destroy(popup, 1f);
    }

    private void SetActiveSafe(GameObject obj, bool value)
    {
        if (obj != null) obj.SetActive(value);
    }

    // --- Các hàm gọi từ Button OnClick() trong Inspector ---
    public void OnClickPlay() => GameManager.Instance.StartGame();
    public void OnClickPause() => GameManager.Instance.PauseGame();
    public void OnClickResume() => GameManager.Instance.ResumeGame();
    public void OnClickRestart() => GameManager.Instance.RestartGame();
    public void OnClickMenu() => GameManager.Instance.GoToMenu();
    public void OnClickQuit() => Application.Quit();
}
