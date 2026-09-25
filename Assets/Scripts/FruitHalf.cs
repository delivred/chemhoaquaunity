using UnityEngine;

/// <summary>
/// Gắn vào prefab "nửa trái cây" (sprite đã cắt sẵn nửa quả).
/// Chỉ đơn giản là rơi theo vật lý rồi tự biến mất, không thể chém tiếp.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class FruitHalf : MonoBehaviour
{
    public float fadeAfter = 1.2f;
    private SpriteRenderer sr;
    private float timer;
    private bool fading = false;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= fadeAfter)
        {
            fading = true;
        }

        if (fading && sr != null)
        {
            Color c = sr.color;
            c.a -= Time.deltaTime * 2f;
            sr.color = c;
            if (c.a <= 0f) Destroy(gameObject);
        }
    }
}
