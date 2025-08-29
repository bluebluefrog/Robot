using UnityEngine;
using UnityEngine.UI;

public class JointSliderUI : MonoBehaviour
{
    public JointController controller;
    [Tooltip("与 joints.json 的 name 一致，如 neck / left_shoulder")]
    public string jointName;

    public Slider xSlider; public Text xLabel;
    public Slider ySlider; public Text yLabel;
    public Slider zSlider; public Text zLabel;

    JointConfig cfg;

    void Start()
    {
        if (controller == null || !controller.HasJoint(jointName))
        {
            Debug.LogError($"JointSliderUI: 未找到关节 {jointName}");
            enabled = false; return;
        }
        cfg = controller.GetConfig(jointName);

        SetupAxis(xSlider, xLabel, cfg.x, cfg.limitX, "X");
        SetupAxis(ySlider, yLabel, cfg.y, cfg.limitY, "Y");
        SetupAxis(zSlider, zLabel, cfg.z, cfg.limitZ, "Z");

        // 初始应用一次
        ApplyFromUI();
        xSlider.onValueChanged.AddListener(_ => ApplyFromUI());
        ySlider.onValueChanged.AddListener(_ => ApplyFromUI());
        zSlider.onValueChanged.AddListener(_ => ApplyFromUI());
    }

    void SetupAxis(Slider s, Text lab, bool enabledAxis, JointLimit lim, string axisName)
    {
        if (s == null) return;
        s.gameObject.SetActive(enabledAxis);
        if (!enabledAxis) { if (lab) lab.text = $"{axisName}: (off)"; return; }
        s.minValue = lim.min; s.maxValue = lim.max;
        s.value = 0f;
        if (lab) lab.text = $"{axisName}: {s.value:0}";
        s.onValueChanged.AddListener(v => { if (lab) lab.text = $"{axisName}: {v:0}"; });
    }

    public void ResetAngles()
    {
        if (xSlider.gameObject.activeSelf) xSlider.value = 0f;
        if (ySlider.gameObject.activeSelf) ySlider.value = 0f;
        if (zSlider.gameObject.activeSelf) zSlider.value = 0f;
        ApplyFromUI();
    }

    void ApplyFromUI()
    {
        float? xv = xSlider.gameObject.activeSelf ? xSlider.value : (float?)null;
        float? yv = ySlider.gameObject.activeSelf ? ySlider.value : (float?)null;
        float? zv = zSlider.gameObject.activeSelf ? zSlider.value : (float?)null;
        controller.SetJointAngle(jointName, xv, yv, zv);
    }
}
