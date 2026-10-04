using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Xử lý việc "chém" bằng cách theo dõi đường di chuyển của chuột/ngón tay,
/// dùng Raycast giữa các điểm liên tiếp để phát hiện va chạm với trái cây/bom.
/// Gắn script này vào 1 GameObject rỗng (ví dụ "Blade") trong scene.
/// Yêu cầu: có TrailRenderer trên cùng GameObject để hiển thị vệt chém (tùy chọn).
/// </summary>
public class BladeController : MonoBehaviour
{
    [Header("Cấu hình chém")]
    [Tooltip("Vận tốc tối thiểu (đơn vị/giây) để tính là 1 nhát chém hợp lệ, tránh chém khi chỉ rê chuột nhẹ")]
    public float minSliceVelocity = 0.5f;

    public LayerMask sliceableLayer; // đặt layer riêng cho Fruit + Bomb, gán trong Inspector

    private Camera mainCamera;
    private Vector2 previousWorldPos;
    private Vector2 currentWorldPos;
    private bool isSlicing = false;
    private TrailRenderer trail;

    // Tránh chém trúng 1 quả nhiều lần trong cùng 1 frame kéo dài
    private HashSet<EntityId> hitThisStroke = new HashSet<EntityId>();

    void Awake()
    {
        mainCamera = Camera.main;
        trail = GetComponent<TrailRenderer>();
        if (trail != null) trail.emitting = false;
    }

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
            if (trail != null) trail.emitting = false;
            return;
        }

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
        HandleMouseInput();
#else
        HandleTouchInput();
#endif
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartStroke(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0))
        {
            ContinueStroke(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            EndStroke();
        }
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount == 0)
        {
            if (isSlicing) EndStroke();
            return;
        }

        Touch touch = Input.GetTouch(0);
        switch (touch.phase)
        {
            case TouchPhase.Began:
                StartStroke(touch.position);
                break;
            case TouchPhase.Moved:
            case TouchPhase.Stationary:
                ContinueStroke(touch.position);
                break;
            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                EndStroke();
                break;
        }
    }

    private Vector2 GetWorldPosition(Vector2 screenPos)
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) return screenPos;
        // Đảm bảo lấy đúng tọa độ trên mặt phẳng Z = 0
        float distanceToPlane = -mainCamera.transform.position.z;
        Vector3 wp = mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, distanceToPlane));
        return new Vector2(wp.x, wp.y);
    }

    private void StartStroke(Vector2 screenPos)
    {
        isSlicing = true;
        hitThisStroke.Clear();
        previousWorldPos = GetWorldPosition(screenPos);
        transform.position = new Vector3(previousWorldPos.x, previousWorldPos.y, 0f);
        if (trail != null)
        {
            trail.Clear();
            trail.emitting = true;
        }
    }

    private void ContinueStroke(Vector2 screenPos)
    {
        currentWorldPos = GetWorldPosition(screenPos);

        float distance = Vector2.Distance(previousWorldPos, currentWorldPos);
        float dt = Mathf.Max(0.001f, Time.deltaTime);
        float velocity = distance / dt;

        // Nội suy mượt (sub-stepping) để vệt chém uốn lượn mềm mại khi vung chuột nhanh
        int steps = Mathf.Max(1, Mathf.CeilToInt(distance / 0.15f));
        Vector2 stepFrom = previousWorldPos;

        for (int i = 1; i <= steps; i++)
        {
            float t = (float)i / steps;
            Vector2 stepTo = Vector2.Lerp(previousWorldPos, currentWorldPos, t);
            transform.position = new Vector3(stepTo.x, stepTo.y, 0f);

            if (velocity >= minSliceVelocity)
            {
                CheckSliceAlongPath(stepFrom, stepTo);
            }
            stepFrom = stepTo;
        }

        previousWorldPos = currentWorldPos;
    }

    private void EndStroke()
    {
        isSlicing = false;
        if (trail != null) trail.emitting = false;
    }

    /// <summary>
    /// Bắn nhiều raycast dọc theo đoạn di chuyển của lưỡi dao để không bỏ sót trái cây
    /// khi rê chuột/ngón tay nhanh qua nhiều đối tượng cùng lúc.
    /// </summary>
    private void CheckSliceAlongPath(Vector2 from, Vector2 to)
    {
        Vector2 direction = to - from;
        float distance = direction.magnitude;

        RaycastHit2D[] hits = Physics2D.RaycastAll(from, direction.normalized, distance, sliceableLayer);

        foreach (RaycastHit2D hit in hits)
        {
            EntityId id = hit.collider.gameObject.GetEntityId();
            if (hitThisStroke.Contains(id)) continue;
            hitThisStroke.Add(id);

            Fruit fruit = hit.collider.GetComponent<Fruit>();
            if (fruit != null)
            {
                fruit.Slice(direction.normalized, hit.point);
                continue;
            }

            Bomb bomb = hit.collider.GetComponent<Bomb>();
            if (bomb != null)
            {
                bomb.Explode(hit.point);
            }
        }
    }
}
