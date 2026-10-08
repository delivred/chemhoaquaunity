using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gắn vào prefab quả bom.
/// - Ngòi nổ có ngọn lửa lập lòe (Fuse Spark) nhấp nháy liên tục khi bay.
/// - Khi bị chém trúng: Kích hoạt hiệu ứng nổ rực lửa BombExplosionFX, rung màn hình mạnh, trừ 1 tim (hoặc Game Over).
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
    public bool instantGameOver = false;

    private bool isTriggered = false;
    private Rigidbody2D rb;
    private GameObject fuseSparkObj;
    private SpriteRenderer fuseSparkSr;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        // Tạo đóm lửa ngòi nổ lập lòe sinh động
        fuseSparkObj = new GameObject("FuseSpark");
        fuseSparkObj.transform.SetParent(transform, false);
        // Vị trí đầu ngòi nổ (góc trên bên phải quả bom)
        fuseSparkObj.transform.localPosition = new Vector3(0.68f, 1.12f, 0f);

        fuseSparkSr = fuseSparkObj.AddComponent<SpriteRenderer>();
        fuseSparkSr.sprite = Resources.Load<Sprite>("blade_gleam") ?? Resources.Load<Sprite>("Sprites/blade_gleam");
        fuseSparkSr.color = new Color(1f, 0.85f, 0.2f, 1f);
        fuseSparkSr.sortingOrder = 10;
        fuseSparkObj.transform.localScale = Vector3.one * 0.35f;
    }

    void Update()
    {
        if (fuseSparkObj != null)
        {
            // Hiệu ứng ngọn lửa ngòi nổ cháy xèo xèo lập lòe
            float pulse = Mathf.PingPong(Time.time * 14f, 1f);
            fuseSparkObj.transform.localScale = Vector3.one * Mathf.Lerp(0.25f, 0.48f, pulse);
            fuseSparkSr.color = Color.Lerp(new Color(1f, 0.45f, 0.1f, 1f), new Color(1f, 0.95f, 0.35f, 1f), pulse);
        }
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

        // 1. Tạo hiệu ứng nổ rực lửa hoành tráng
        BombExplosionFX.Spawn(hitPoint);

        if (explosionEffectPrefab != null)
        {
            GameObject fx = Instantiate(explosionEffectPrefab, hitPoint, Quaternion.identity);
            Destroy(fx, 2f);
        }

        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, hitPoint);
        }

        // Rung màn hình mạnh báo hiệu nổ bom
        Camera.main?.SendMessage("Shake", 0.45f, SendMessageOptions.DontRequireReceiver);

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
        // Bom rơi ra ngoài không bị chém -> an toàn, không phạt
        Destroy(gameObject);
    }
}

/// <summary>
/// Hiệu ứng nổ bom rực lửa hoành tráng (Explosion Flash, Fire Ring & Shrapnel Sparks).
/// Tự động sinh ra quầng lửa bùng phát, vòng sóng xung kích và tia lửa bắn tung tóe.
/// </summary>
public class BombExplosionFX : MonoBehaviour
{
    public static void Spawn(Vector2 hitPoint)
    {
        GameObject fxObj = new GameObject("BombExplosionFX");
        fxObj.transform.position = new Vector3(hitPoint.x, hitPoint.y, 0f);
        BombExplosionFX fx = fxObj.AddComponent<BombExplosionFX>();
        fx.StartCoroutine(fx.AnimateExplosion());
    }

    private System.Collections.IEnumerator AnimateExplosion()
    {
        Sprite gleamSprite = Resources.Load<Sprite>("blade_gleam") ?? Resources.Load<Sprite>("Sprites/blade_gleam");
        Sprite dropSprite = Resources.Load<Sprite>("juice_drop") ?? Resources.Load<Sprite>("Sprites/juice_drop");

        // 1. Chớp sáng chói lòa toàn màn hình (Blinding Flash)
        GameObject flashObj = new GameObject("ExplosionFlash");
        flashObj.transform.SetParent(transform, false);
        SpriteRenderer flashSr = flashObj.AddComponent<SpriteRenderer>();
        flashSr.sprite = gleamSprite ?? dropSprite;
        flashSr.color = new Color(1f, 0.95f, 0.8f, 1f);
        flashSr.sortingOrder = 30;
        flashObj.transform.localScale = Vector3.one * 3.5f;

        // 2. Vòng sóng xung kích lửa (Shockwave Ring)
        GameObject ringObj = new GameObject("ShockwaveRing");
        ringObj.transform.SetParent(transform, false);
        SpriteRenderer ringSr = ringObj.AddComponent<SpriteRenderer>();
        ringSr.sprite = dropSprite ?? gleamSprite;
        ringSr.color = new Color(1f, 0.45f, 0.1f, 0.9f);
        ringSr.sortingOrder = 28;

        // 3. Tia lửa và mảnh vỡ bay tỏa ra mọi hướng
        int sparkCount = 18;
        List<Transform> sparks = new List<Transform>();
        List<Vector2> sparkVels = new List<Vector2>();
        List<SpriteRenderer> sparkRenderers = new List<SpriteRenderer>();

        for (int i = 0; i < sparkCount; i++)
        {
            GameObject sp = new GameObject($"Spark_{i}");
            sp.transform.SetParent(transform, false);
            SpriteRenderer sr = sp.AddComponent<SpriteRenderer>();
            sr.sprite = gleamSprite ?? dropSprite;
            // Màu tia lửa: cam đỏ rực rỡ đến vàng chói
            sr.color = Color.Lerp(new Color(1f, 0.25f, 0.05f), new Color(1f, 0.9f, 0.2f), Random.value);
            sr.sortingOrder = 29;

            float angle = (i / (float)sparkCount) * 360f + Random.Range(-15f, 15f);
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;
            float speed = Random.Range(6f, 13f);

            sp.transform.localScale = Vector3.one * Random.Range(0.2f, 0.45f);
            sparks.Add(sp.transform);
            sparkVels.Add(dir * speed);
            sparkRenderers.Add(sr);
        }

        float duration = 0.55f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Flash thu nhỏ và mờ
            if (flashObj != null)
            {
                float flashT = Mathf.Clamp01(elapsed / 0.15f);
                flashSr.color = new Color(1f, 0.9f, 0.7f, 1f - flashT);
                flashObj.transform.localScale = Vector3.Lerp(Vector3.one * 4.5f, Vector3.one * 1f, flashT);
            }

            // Sóng xung kích nở to
            if (ringObj != null)
            {
                float ringScale = Mathf.Lerp(0.5f, 6.0f, t);
                ringObj.transform.localScale = new Vector3(ringScale, ringScale, 1f);
                ringSr.color = new Color(1f, 0.4f, 0.1f, (1f - t) * 0.85f);
            }

            // Di chuyển các tia lửa
            for (int i = 0; i < sparks.Count; i++)
            {
                if (sparks[i] != null)
                {
                    sparks[i].position += (Vector3)(sparkVels[i] * Time.deltaTime);
                    sparkVels[i] *= 0.94f; // giảm tốc do lực cản
                    Color c = sparkRenderers[i].color;
                    c.a = 1f - t;
                    sparkRenderers[i].color = c;
                }
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}
