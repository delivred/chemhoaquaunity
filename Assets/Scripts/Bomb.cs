using UnityEngine;

/// <summary>
/// Gắn vào prefab quả bom. Nếu người chơi chém trúng -> nổ, mất mạng (hoặc thua ngay tùy cấu hình).
/// Nếu bom rơi ra ngoài mà không chạm vào thì không sao (không trừ điểm/mạng).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Bomb : MonoBehaviour
{
    [Header("Hiệu ứng")]
    public GameObject explosionEffectPrefab;
    public AudioClip explosionSound;

    [Header("Cấu hình")]
    [Tooltip("Nếu bật, chém trúng bom sẽ thua ngay lập tức thay vì chỉ trừ 1 mạng")]
    public bool instantGameOver = true;

    private bool isTriggered = false;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Launch(Vector2 force, float torque)
    {
        rb.linearVelocity = force;
        rb.AddTorque(torque, ForceMode2D.Impulse);
    }

    /// <summary>Gọi từ BladeController khi lưỡi dao chạm vào bom.</summary>
    public void Explode(Vector2 hitPoint)
    {
        if (isTriggered) return;
        isTriggered = true;

        if (explosionEffectPrefab != null)
        {
            GameObject fx = Instantiate(explosionEffectPrefab, hitPoint, Quaternion.identity);
            Destroy(fx, 2f);
        }

        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, hitPoint);
        }

        // Rung màn hình nhẹ (yêu cầu Camera có script CameraShake, xem hướng dẫn)
        Camera.main?.SendMessage("Shake", 0.3f, SendMessageOptions.DontRequireReceiver);

        if (instantGameOver)
        {
            GameManager.Instance.EndGame();
        }
        else
        {
            GameManager.Instance.LoseLife();
        }

        gameObject.SetActive(false);
        Destroy(gameObject, 0.05f);
    }

    void OnBecameInvisible()
    {
        // Bom rơi ra ngoài không bị chém -> không phạt gì cả, chỉ hủy
        Destroy(gameObject);
    }
}
