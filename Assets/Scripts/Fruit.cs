using System.Collections.Generic;
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
        else
        {
            SliceJuiceFX.Spawn(hitPoint, sliceDirection, GetJuiceColor());
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

    private Color GetJuiceColor()
    {
        if (fruitType == FruitType.Golden) return new Color(1f, 0.85f, 0.15f);
        if (fruitType == FruitType.Freeze) return new Color(0.25f, 0.9f, 1f);

        string n = gameObject.name.ToLower();
        if (n.Contains("cam")) return new Color(1f, 0.55f, 0.05f);
        if (n.Contains("chuoi")) return new Color(1f, 0.88f, 0.15f);
        if (n.Contains("dautay")) return new Color(1f, 0.15f, 0.35f);
        if (n.Contains("dua") || n.Contains("hau")) return new Color(0.95f, 0.2f, 0.3f);
        return new Color(0.95f, 0.15f, 0.2f); // Mặc định: đỏ tươi (táo)
    }

    /// <summary>Được gọi khi quả rơi ra khỏi vùng chơi (dưới đáy màn hình) mà chưa bị chém.</summary>
    void OnBecameInvisible()
    {
        if (isSliced) return;
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;

        // Reset chuỗi combo khi để rơi quả
        ScoreManager.Instance?.ResetCombo();

        // Chỉ trừ mạng nếu được bật cấu hình và quả rơi xuống dưới đáy
        if (GameManager.Instance.loseLifeOnMissFruit && transform.position.y < -6f && countsAsMissIfMissed)
        {
            GameManager.Instance.LoseLife();
        }

        Destroy(gameObject);
    }
}

/// <summary>
/// Hiệu ứng tóe nước ép trái cây và tia lửa chém (Juice Splash & Slice Flash).
/// Tự động sinh ra các hạt nước ép bay tỏa ra xung quanh điểm chém và mờ dần.
/// </summary>
public class SliceJuiceFX : MonoBehaviour
{
    private static Sprite dropSprite;

    public static void Spawn(Vector2 hitPoint, Vector2 sliceDir, Color juiceColor)
    {
        GameObject fxObj = new GameObject("SliceJuiceFX");
        fxObj.transform.position = new Vector3(hitPoint.x, hitPoint.y, 0f);
        SliceJuiceFX fx = fxObj.AddComponent<SliceJuiceFX>();
        fx.Initialize(sliceDir, juiceColor);
    }

    private void Initialize(Vector2 sliceDir, Color juiceColor)
    {
        if (dropSprite == null)
        {
            dropSprite = Resources.Load<Sprite>("juice_drop");
            if (dropSprite == null)
            {
                dropSprite = Sprite.Create(
                    Texture2D.whiteTexture,
                    new Rect(0, 0, 4, 4),
                    new Vector2(0.5f, 0.5f),
                    100f
                );
            }
        }

        StartCoroutine(AnimateSplash(sliceDir, juiceColor));
    }

    private System.Collections.IEnumerator AnimateSplash(Vector2 sliceDir, Color juiceColor)
    {
        // 1. Tạo vệt chém lóe sáng tức thì (Slash Flash)
        GameObject flashObj = new GameObject("SlashFlash");
        flashObj.transform.SetParent(transform, false);
        flashObj.transform.localPosition = Vector3.zero;

        float angle = Mathf.Atan2(sliceDir.y, sliceDir.x) * Mathf.Rad2Deg;
        flashObj.transform.localRotation = Quaternion.Euler(0, 0, angle);

        SpriteRenderer flashSr = flashObj.AddComponent<SpriteRenderer>();
        flashSr.sprite = dropSprite;
        flashSr.color = new Color(1f, 1f, 1f, 0.9f);
        flashSr.sortingOrder = 15;
        flashObj.transform.localScale = new Vector3(1.4f, 0.12f, 1f);

        // 2. Tạo 8-12 hạt nước ép bắn ra xung quanh
        int dropCount = Random.Range(8, 13);
        List<Transform> drops = new List<Transform>();
        List<Vector2> dropVelocities = new List<Vector2>();
        List<SpriteRenderer> dropRenderers = new List<SpriteRenderer>();
        List<float> initialScales = new List<float>();

        Vector2 perp = new Vector2(-sliceDir.y, sliceDir.x).normalized;

        for (int i = 0; i < dropCount; i++)
        {
            GameObject drop = new GameObject($"Drop_{i}");
            drop.transform.SetParent(transform, false);
            drop.transform.localPosition = (Vector3)(Random.insideUnitCircle * 0.15f);

            SpriteRenderer sr = drop.AddComponent<SpriteRenderer>();
            sr.sprite = dropSprite;
            Color c = juiceColor;
            c.r = Mathf.Clamp01(c.r + Random.Range(-0.08f, 0.08f));
            c.g = Mathf.Clamp01(c.g + Random.Range(-0.08f, 0.08f));
            c.b = Mathf.Clamp01(c.b + Random.Range(-0.08f, 0.08f));
            sr.color = c;
            sr.sortingOrder = 12;

            float scale = Random.Range(0.08f, 0.18f);
            drop.transform.localScale = Vector3.one * scale;

            float side = (i % 2 == 0) ? 1f : -1f;
            Vector2 launchDir = (perp * side * Random.Range(0.6f, 1.2f) + Random.insideUnitCircle * 0.5f).normalized;
            float speed = Random.Range(3.5f, 7.5f);

            drops.Add(drop.transform);
            dropVelocities.Add(launchDir * speed);
            dropRenderers.Add(sr);
            initialScales.Add(scale);
        }

        // 3. Di chuyển và thu nhỏ/mờ dần
        float duration = 0.38f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            if (flashSr != null)
            {
                float flashT = Mathf.Clamp01(elapsed / 0.12f);
                Color fc = flashSr.color;
                fc.a = Mathf.Lerp(0.9f, 0f, flashT);
                flashSr.color = fc;
                flashObj.transform.localScale = Vector3.Lerp(new Vector3(1.4f, 0.12f, 1f), new Vector3(2.2f, 0.02f, 1f), flashT);
            }

            for (int i = 0; i < drops.Count; i++)
            {
                if (drops[i] == null) continue;

                Vector2 vel = dropVelocities[i];
                vel.y -= 12f * Time.deltaTime;
                dropVelocities[i] = vel;

                drops[i].localPosition += (Vector3)(vel * Time.deltaTime);

                float s = Mathf.Lerp(initialScales[i], 0f, t * t);
                drops[i].localScale = Vector3.one * s;

                Color dc = dropRenderers[i].color;
                dc.a = Mathf.Lerp(1f, 0f, t);
                dropRenderers[i].color = dc;
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}
