using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Xử lý việc "chém" sắc bén theo phong cách Katana / Fruit Ninja:
/// - Vệt chém 2 lớp: Lớp hào quang năng lượng xanh sắc sảo + Lõi kiếm thép trắng tinh siêu bén (Razor Core).
/// - Điểm sáng mũi kiếm (Blade Tip Gleam) xoay theo hướng chém.
/// - Nội suy đường cong mượt mà Catmull-Rom Spline loại bỏ gấp khúc khi vung chuột nhanh.
/// - Raycast liên tục để chém chính xác mọi trái cây trên đường cắt.
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

    // Trail Renderer chính (Hào quang lưỡi kiếm)
    private TrailRenderer trail;

    // Trail Renderer phụ (Lõi lưỡi kiếm trắng tinh siêu sắc bén)
    private TrailRenderer coreTrail;

    // Điểm sáng mũi kiếm (Gleam star)
    private GameObject tipGleamObj;
    private SpriteRenderer tipGleamSr;

    // Danh sách lưu tọa độ gần nhất để nội suy đường cong Catmull-Rom Spline
    private List<Vector2> strokePoints = new List<Vector2>();

    // Tránh chém trúng 1 quả nhiều lần trong cùng 1 stroke
    private HashSet<EntityId> hitThisStroke = new HashSet<EntityId>();

    void Awake()
    {
        mainCamera = Camera.main;
        trail = GetComponent<TrailRenderer>();
        if (trail != null) trail.emitting = false;

        SetupInnerRazorTrail();
        SetupBladeTipGleam();
    }

    /// <summary>Khởi tạo vệt lõi kiếm trắng siêu mỏng tạo cảm giác sắc bén như dao cạo.</summary>
    private void SetupInnerRazorTrail()
    {
        Transform existingCore = transform.Find("BladeCore");
        GameObject coreObj = existingCore != null ? existingCore.gameObject : new GameObject("BladeCore");
        coreObj.transform.SetParent(transform, false);

        coreTrail = coreObj.GetComponent<TrailRenderer>();
        if (coreTrail == null) coreTrail = coreObj.AddComponent<TrailRenderer>();

        if (trail != null && trail.sharedMaterial != null)
        {
            coreTrail.sharedMaterial = trail.sharedMaterial;
        }

        coreTrail.time = 0.10f; // Tan biến nhanh hơn lớp ngoài tạo đuôi nhọn hoắt
        coreTrail.minVertexDistance = 0.015f;
        coreTrail.numCapVertices = 0;
        coreTrail.numCornerVertices = 8;
        coreTrail.sortingOrder = (trail != null ? trail.sortingOrder : 10) + 2;

        // Đường cong độ dày siêu mảnh sắc bén (lưỡi thép)
        AnimationCurve coreCurve = new AnimationCurve();
        coreCurve.AddKey(new Keyframe(0f, 0.015f));      // Mũi kim
        coreCurve.AddKey(new Keyframe(0.06f, 0.08f));    // Thân lưỡi mỏng
        coreCurve.AddKey(new Keyframe(0.35f, 0.045f));   // Vuốt thon
        coreCurve.AddKey(new Keyframe(0.70f, 0.02f));    // Đuôi mảnh
        coreCurve.AddKey(new Keyframe(1f, 0f));          // Chóp đuôi biến mất
        coreTrail.widthCurve = coreCurve;

        // Màu trắng tinh khiết phát sáng
        Gradient coreGrad = new Gradient();
        coreGrad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(new Color(0.92f, 0.98f, 1f), 0.6f),
                new GradientColorKey(Color.white, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.9f, 0.35f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        coreTrail.colorGradient = coreGrad;
        coreTrail.emitting = false;
    }

    /// <summary>Khởi tạo điểm sáng lóe lên ở đầu mũi kiếm.</summary>
    private void SetupBladeTipGleam()
    {
        Transform existingGleam = transform.Find("BladeTipGleam");
        tipGleamObj = existingGleam != null ? existingGleam.gameObject : new GameObject("BladeTipGleam");
        tipGleamObj.transform.SetParent(transform, false);

        tipGleamSr = tipGleamObj.GetComponent<SpriteRenderer>();
        if (tipGleamSr == null) tipGleamSr = tipGleamObj.AddComponent<SpriteRenderer>();

        Sprite gleamSprite = Resources.Load<Sprite>("blade_gleam");
        if (gleamSprite == null)
        {
            gleamSprite = Resources.Load<Sprite>("Sprites/blade_gleam");
        }
        if (gleamSprite != null) tipGleamSr.sprite = gleamSprite;

        tipGleamSr.color = new Color(1f, 1f, 1f, 0.95f);
        tipGleamSr.sortingOrder = 16;
        tipGleamObj.transform.localScale = new Vector3(0.28f, 0.28f, 1f);
        tipGleamObj.SetActive(false);
    }

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
            SetEmitting(false);
            return;
        }

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
        HandleMouseInput();
#else
        HandleTouchInput();
#endif
    }

    private void SetEmitting(bool emit)
    {
        if (trail != null) trail.emitting = emit;
        if (coreTrail != null) coreTrail.emitting = emit;
        if (tipGleamObj != null && !emit) tipGleamObj.SetActive(false);
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

        strokePoints.Clear();
        strokePoints.Add(previousWorldPos);

        if (trail != null)
        {
            trail.Clear();
            trail.emitting = true;
        }
        if (coreTrail != null)
        {
            coreTrail.Clear();
            coreTrail.emitting = true;
        }
        if (tipGleamObj != null)
        {
            tipGleamObj.SetActive(true);
            tipGleamObj.transform.localScale = new Vector3(0.24f, 0.24f, 1f);
        }
    }

    private void ContinueStroke(Vector2 screenPos)
    {
        currentWorldPos = GetWorldPosition(screenPos);
        float distance = Vector2.Distance(previousWorldPos, currentWorldPos);
        if (distance < 0.01f) return;

        float dt = Mathf.Max(0.001f, Time.deltaTime);
        float velocity = distance / dt;

        strokePoints.Add(currentWorldPos);
        if (strokePoints.Count > 10) strokePoints.RemoveAt(0);

        // Tính 4 điểm mốc để nội suy đường cong Catmull-Rom
        int count = strokePoints.Count;
        Vector2 p1 = (count >= 2) ? strokePoints[count - 2] : previousWorldPos;
        Vector2 p2 = currentWorldPos;
        Vector2 p0 = (count >= 3) ? strokePoints[count - 3] : (p1 - (p2 - p1));
        Vector2 p3 = p2 + (p2 - p1);

        // Chia nhỏ bước nội suy để vệt chém cong mượt như đường kiếm Katana
        int steps = Mathf.Clamp(Mathf.CeilToInt(distance / 0.08f), 1, 16);
        Vector2 stepFrom = p1;

        for (int i = 1; i <= steps; i++)
        {
            float t = (float)i / steps;
            Vector2 stepTo = EvaluateCatmullRom(p0, p1, p2, p3, t);
            transform.position = new Vector3(stepTo.x, stepTo.y, 0f);

            if (velocity >= minSliceVelocity)
            {
                CheckSliceAlongPath(stepFrom, stepTo);
            }
            stepFrom = stepTo;
        }

        // Cập nhật hướng và hiệu ứng lấp lánh của mũi kiếm
        if (tipGleamObj != null)
        {
            Vector2 moveDir = currentWorldPos - previousWorldPos;
            if (moveDir.sqrMagnitude > 0.0001f)
            {
                float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
                tipGleamObj.transform.rotation = Quaternion.Euler(0, 0, angle);

                // Khi vung nhanh, mũi kiếm kéo dãn tạo cảm giác xé gió sắc bén
                float stretch = Mathf.Clamp(1f + velocity * 0.05f, 1f, 2.2f);
                tipGleamObj.transform.localScale = new Vector3(0.32f * stretch, 0.16f, 1f);
                tipGleamObj.SetActive(velocity >= minSliceVelocity);
            }
        }

        previousWorldPos = currentWorldPos;
    }

    private void EndStroke()
    {
        isSlicing = false;
        strokePoints.Clear();
        SetEmitting(false);
    }

    /// <summary>
    /// Nội suy đường cong tự nhiên mượt mà Catmull-Rom Spline giữa 4 điểm P0, P1, P2, P3.
    /// Giúp đường chém không bị góc cạnh khi người chơi xoay cổ tay hoặc vẽ vòng cung.
    /// </summary>
    private Vector2 EvaluateCatmullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;
        return 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3
        );
    }

    /// <summary>
    /// Bắn raycast dọc theo đoạn di chuyển của lưỡi dao để không bỏ sót trái cây
    /// khi rê chuột/ngón tay nhanh qua nhiều đối tượng cùng lúc.
    /// </summary>
    private void CheckSliceAlongPath(Vector2 from, Vector2 to)
    {
        Vector2 direction = to - from;
        float distance = direction.magnitude;
        if (distance <= 0.0001f) return;

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
