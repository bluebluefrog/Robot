using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class ViewPreset
{
    public string name = "Preset";
    public float yaw = 0f;              // 水平角（度）
    public float pitch = 20f;           // 俯仰角（度）
    public float distance = 5f;         // 距离
    public Vector3 focusOffset = Vector3.zero; // 观察点偏移（例如胸口上移）
}

public class CameraLook : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    public Vector2 pitchLimits = new Vector2(-30f, 80f);

    [Header("Mouse / Touch")]
    public float rotateSensitivity = 0.2f;
    public float zoomSensitivity   = 2.0f;
    public float pinchSensitivity  = 0.02f;

    [Header("Presets (Q/W/E/R)")]
    public ViewPreset presetQ = new ViewPreset { name = "Front", yaw = 0f,   pitch = 20f, distance = 5f, focusOffset = new Vector3(0, 0.6f, 0) };
    public ViewPreset presetW = new ViewPreset { name = "Left",  yaw = -90f, pitch = 20f, distance = 5.5f, focusOffset = new Vector3(0, 0.6f, 0) };
    public ViewPreset presetE = new ViewPreset { name = "Right", yaw = 90f,  pitch = 20f, distance = 5.5f, focusOffset = new Vector3(0, 0.6f, 0) };
    public ViewPreset presetR = new ViewPreset { name = "Back",  yaw = 180f, pitch = 20f, distance = 6f, focusOffset = new Vector3(0, 0.6f, 0) };

    [Header("Preset Transition")]
    public float presetBlendTime = 0.25f;  // 预设切换平滑时间（秒）

    // 当前相机参数
    float yaw, pitch, distance;
    Vector3 focusOffset = Vector3.zero;

    // 过渡控制
    bool blending = false;
    float blendT = 0f;
    float fromYaw, fromPitch, fromDist;
    Vector3 fromOffset;
    float toYaw, toPitch, toDist;
    Vector3 toOffset;

    void Start()
    {
        if (target == null) { Debug.LogWarning("OrbitCameraWithPresets: 请指定 target"); return; }

        // 初始使用 Q 预设
        ApplyPresetImmediate(presetQ);
    }

    void LateUpdate()
    {
        if (target == null) return;

        // --- 键盘切换预设（新输入系统） ---
        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.qKey.wasPressedThisFrame) StartBlendTo(presetQ);
            if (kb.wKey.wasPressedThisFrame) StartBlendTo(presetW);
            if (kb.eKey.wasPressedThisFrame) StartBlendTo(presetE);
            if (kb.rKey.wasPressedThisFrame) StartBlendTo(presetR);
        }

        // --- 鼠标/触摸交互（与之前一致） ---
        HandlePointerInput();

        // --- 限制俯仰 ---
        pitch = Mathf.Clamp(pitch, pitchLimits.x, pitchLimits.y);

        // --- 计算并放置相机 ---
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 focus = target.position + focusOffset;
        Vector3 dir = rot * Vector3.back;
        Vector3 pos = focus + dir * distance;

        transform.position = pos;
        transform.LookAt(focus);
    }

    void HandlePointerInput()
    {
        // 进行预设平滑时，也允许鼠标打断继续控制
        if (Mouse.current != null)
        {
            if (Mouse.current.leftButton.isPressed)
            {
                Vector2 delta = Mouse.current.delta.ReadValue();
                yaw   += delta.x * rotateSensitivity;
                pitch -= delta.y * rotateSensitivity;
                blending = false; // 手动操作打断预设过渡
            }

            float scroll = Mouse.current.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                distance = Mathf.Clamp(distance - scroll * 0.01f * zoomSensitivity, 1.5f, 15f);
                blending = false;
            }
        }

        var ts = Touchscreen.current;
        if (ts != null && ts.touches.Count > 0)
        {
            if (ts.touches.Count == 1 && ts.touches[0].isInProgress)
            {
                Vector2 delta = ts.touches[0].delta.ReadValue();
                yaw   += delta.x * rotateSensitivity * 0.1f;
                pitch -= delta.y * rotateSensitivity * 0.1f;
                blending = false;
            }
            else if (ts.touches.Count >= 2 &&
                     ts.touches[0].isInProgress && ts.touches[1].isInProgress)
            {
                Vector2 p0 = ts.touches[0].position.ReadValue();
                Vector2 p1 = ts.touches[1].position.ReadValue();
                Vector2 pp0 = p0 - ts.touches[0].delta.ReadValue();
                Vector2 pp1 = p1 - ts.touches[1].delta.ReadValue();

                float prevDist = (pp0 - pp1).magnitude;
                float currDist = (p0 - p1).magnitude;
                float pinch = currDist - prevDist;

                distance = Mathf.Clamp(distance - pinch * pinchSensitivity, 1.5f, 15f);
                blending = false;
            }
        }

        // 若在预设过渡中，推进插值
        if (blending)
        {
            blendT += Time.deltaTime / Mathf.Max(0.0001f, presetBlendTime);
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(blendT));
            // 注意角度插值用 Mathf.Lerp 足够；如需跨 180°优化可自行封装
            yaw        = Mathf.Lerp(fromYaw,   toYaw,   t);
            pitch      = Mathf.Lerp(fromPitch, toPitch, t);
            distance   = Mathf.Lerp(fromDist,  toDist,  t);
            focusOffset= Vector3.Lerp(fromOffset, toOffset, t);
            if (t >= 1f) blending = false;
        }
    }

    void StartBlendTo(ViewPreset p)
    {
        fromYaw = yaw; fromPitch = pitch; fromDist = distance; fromOffset = focusOffset;
        toYaw   = p.yaw; toPitch = p.pitch; toDist = p.distance; toOffset = p.focusOffset;
        blendT  = 0f; blending = true;
    }

    void ApplyPresetImmediate(ViewPreset p)
    {
        yaw = p.yaw; pitch = p.pitch; distance = p.distance; focusOffset = p.focusOffset;
        blending = false; blendT = 0f;
    }
}
