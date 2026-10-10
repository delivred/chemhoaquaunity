using UnityEngine;

/// <summary>
/// Quản lý điểm số và hệ thống combo (chém nhiều trái cây liên tiếp trong thời gian ngắn).
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int CurrentScore { get; private set; }
    public int ComboCount { get; private set; }

    [Header("Cấu hình Combo")]
    [Tooltip("Thời gian tối đa (giây) giữa 2 lần chém để vẫn được tính combo")]
    public float comboResetTime = 0.6f;

    private float lastSliceTime = -999f;

    [Header("Điểm thưởng combo")]
    public int comboBonusPerExtra = 5; // mỗi trái thêm trong combo được cộng thêm điểm

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void ResetScore()
    {
        CurrentScore = 0;
        ComboCount = 0;
        lastSliceTime = -999f;
        UIManager.Instance.UpdateScore(CurrentScore);
        UIManager.Instance.UpdateCombo(0);
    }

    /// <summary>Đặt lại chuỗi combo về 0 khi người chơi để rơi quả hoặc hết thời gian.</summary>
    public void ResetCombo()
    {
        ComboCount = 0;
        lastSliceTime = -999f;
        UIManager.Instance?.UpdateCombo(0);
    }

    /// <summary>Gọi mỗi khi một quả bị chém trúng.</summary>
    public void AddSliceScore(int basePoints)
    {
        // Kiểm tra combo: nếu chém trong khoảng thời gian ngắn thì combo tăng
        if (Time.time - lastSliceTime <= comboResetTime)
        {
            ComboCount++;
        }
        else
        {
            ComboCount = 1;
        }
        lastSliceTime = Time.time;

        int bonus = (ComboCount - 1) * comboBonusPerExtra;
        float diffMult = 1f;
        if (GameManager.Instance != null && GameManager.Instance.CurrentDifficulty == GameManager.GameDifficulty.Hard)
        {
            diffMult = 1.5f; // Chế độ Khó được thưởng x1.5 điểm
        }
        int totalPoints = Mathf.RoundToInt((basePoints + bonus) * diffMult);

        CurrentScore += totalPoints;

        UIManager.Instance.UpdateScore(CurrentScore);
        UIManager.Instance.UpdateCombo(ComboCount);
        UIManager.Instance.ShowFloatingScore(totalPoints);
    }
}
