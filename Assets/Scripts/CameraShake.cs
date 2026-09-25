using UnityEngine;

/// <summary>
/// Gắn vào Main Camera. Gây hiệu ứng rung nhẹ khi nổ bom hoặc combo lớn.
/// Được gọi qua SendMessage("Shake", duration) từ Bomb.cs.
/// </summary>
public class CameraShake : MonoBehaviour
{
    private Vector3 originalPos;
    private float shakeTimer = 0f;
    public float shakeMagnitude = 0.15f;

    void Awake()
    {
        originalPos = transform.localPosition;
    }

    void Update()
    {
        if (shakeTimer > 0)
        {
            transform.localPosition = originalPos + (Vector3)Random.insideUnitCircle * shakeMagnitude;
            shakeTimer -= Time.unscaledDeltaTime;

            if (shakeTimer <= 0)
            {
                transform.localPosition = originalPos;
            }
        }
    }

    /// <summary>Gọi hàm này để bắt đầu rung camera trong "duration" giây.</summary>
    public void Shake(float duration)
    {
        shakeTimer = duration;
    }
}
