using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Thêm hiệu ứng nhấn nảy (Juice & Punch Scale) cho nút bấm UI.
/// Sử dụng Time.unscaledDeltaTime để hoạt động mượt mà ngay cả khi Pause (Time.timeScale = 0).
/// </summary>
public class UIButtonJuice : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Scale Settings")]
    public float pressedScale = 0.92f;
    public float hoverScale = 1.04f;
    public float animSpeed = 16f;

    private Vector3 initialScale;
    private Coroutine scaleCoroutine;

    void Awake()
    {
        initialScale = transform.localScale;
    }

    void OnEnable()
    {
        transform.localScale = initialScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        StartScaleAnimation(initialScale * pressedScale);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StartScaleAnimation(initialScale);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
        StartScaleAnimation(initialScale * hoverScale);
#endif
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StartScaleAnimation(initialScale);
    }

    private void StartScaleAnimation(Vector3 targetScale)
    {
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleRoutine(targetScale));
    }

    private IEnumerator ScaleRoutine(Vector3 target)
    {
        Vector3 start = transform.localScale;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * animSpeed;
            transform.localScale = Vector3.Lerp(start, target, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        transform.localScale = target;
    }
}
