using UnityEngine;

/// <summary>
/// Gắn script này vào mỗi prefab trái cây.
/// Xử lý: loại trái cây, điểm số, hiệu ứng khi bị chém, và khi rơi ra ngoài màn hình (mất mạng).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Fruit : MonoBehaviour
{
    public enum FruitType { Normal, Golden, Freeze }

    [Header("Loại & điểm")]
    public FruitType fruitType = FruitType.Normal;
    public int basePoints = 10;

    [Header("Hiệu ứng khi chém")]
    public GameObject sliceEffectPrefab;       // particle bắn nước ép
    public GameObject slicedHalfPrefab;        // prefab 2 nửa trái cây chung (fallback)
    public GameObject slicedHalfPrefabLeft;    // nửa trái riêng biệt
    public GameObject slicedHalfPrefabRight;   // nửa phải riêng biệt
    public AudioClip sliceSound;

    [Header("Trạng thái")]
    public bool countsAsMissIfMissed = true; // rơi ra ngoài mà không chém trúng thì có bị trừ mạng không (bom thì false)

    private bool isSliced = false;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    /// <summary>Gọi từ FruitSpawner để "bắn" quả bay lên với lực ban đầu.</summary>
    public void Launch(Vector2 force, float torque)
    {
        rb.linearVelocity = force; // Unity 6 dùng linearVelocity; nếu bản cũ hơn dùng rb.velocity
        rb.AddTorque(torque, ForceMode2D.Impulse);
    }

    /// <summary>Được gọi bởi BladeController khi lưỡi dao chạm vào quả này.</summary>
    public void Slice(Vector2 sliceDirection, Vector2 hitPoint)
    {
        if (isSliced) return; // tránh tính điểm 2 lần cho 1 nhát chém
        isSliced = true;

        // Cộng điểm theo loại trái cây
        int points = basePoints;
        if (fruitType == FruitType.Golden)
        {
            points *= 2;
            UIManager.Instance?.ShowSpecialNotice("★ GOLDEN BONUS! ★", new Color(1f, 0.85f, 0.1f));
            Camera.main?.SendMessage("Shake", 0.16f, SendMessageOptions.DontRequireReceiver);
        }
        else if (fruitType == FruitType.Freeze)
        {
            GameManager.Instance?.ActivateSlowMotion(0.3f, 2.5f);
            UIManager.Instance?.ShowSpecialNotice("❄️ FREEZE SLOW MOTION! ❄️", new Color(0.35f, 0.9f, 1f));
            Camera.main?.SendMessage("Shake", 0.12f, SendMessageOptions.DontRequireReceiver);
        }

        ScoreManager.Instance?.AddSliceScore(points);

        // Spawn particle nước ép tại điểm chém
        if (sliceEffectPrefab != null)
        {
            GameObject fx = Instantiate(sliceEffectPrefab, hitPoint, Quaternion.identity);
            Destroy(fx, 1.5f);
        }

        // Phát âm thanh
        if (sliceSound != null)
        {
            AudioSource.PlayClipAtPoint(sliceSound, hitPoint);
        }

        // Tạo 2 nửa trái cây văng ra theo hướng chém
        if (slicedHalfPrefab != null || slicedHalfPrefabLeft != null || slicedHalfPrefabRight != null)
        {
            SpawnHalf(sliceDirection, -1);
            SpawnHalf(sliceDirection, 1);
        }

        // Ẩn/hủy quả gốc
        gameObject.SetActive(false);
        Destroy(gameObject, 0.05f);
    }

    private void SpawnHalf(Vector2 sliceDirection, int side)
    {
        GameObject prefabToUse = slicedHalfPrefab;
        if (side < 0 && slicedHalfPrefabLeft != null) prefabToUse = slicedHalfPrefabLeft;
        else if (side > 0 && slicedHalfPrefabRight != null) prefabToUse = slicedHalfPrefabRight;

        if (prefabToUse == null) return;

        GameObject half = Instantiate(prefabToUse, transform.position, transform.rotation);
        Rigidbody2D halfRb = half.GetComponent<Rigidbody2D>();

        // Hướng văng ra vuông góc với hướng chém
        Vector2 perpendicular = new Vector2(-sliceDirection.y, sliceDirection.x).normalized;
        Vector2 launchDir = (perpendicular * side + Vector2.up * 0.5f).normalized;

        if (halfRb != null)
        {
            halfRb.linearVelocity = rb.linearVelocity * 0.5f + launchDir * 2f;
            halfRb.AddTorque(side * 5f, ForceMode2D.Impulse);
        }

        Destroy(half, 2f);
    }

    /// <summary>Được gọi khi quả rơi ra khỏi vùng chơi (dưới đáy màn hình) mà chưa bị chém.</summary>
    void OnBecameInvisible()
    {
        if (isSliced) return;
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;

        // Chỉ trừ mạng nếu quả rơi xuống dưới (không tính khi bay ra 2 bên lúc mới spawn)
        if (transform.position.y < -6f && countsAsMissIfMissed)
        {
            GameManager.Instance.LoseLife();
        }

        Destroy(gameObject);
    }
}
