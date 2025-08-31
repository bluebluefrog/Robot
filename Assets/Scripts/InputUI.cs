using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class InputUI : MonoBehaviour
{
    public JointController controller;
    [Tooltip("与 joints.json 的 name 一致")]
    public string jointName;

    public InputField xInput;
    public InputField yInput;
    public InputField zInput;

    public Text errorText;

    private JointConfig cfg;

    void Start()
    {
        if (controller == null || !controller.HasJoint(jointName))
        {
            Debug.LogError($"JointInputUI: 未找到关节 {jointName}");
            enabled = false; return;
        }

        cfg = controller.GetConfig(jointName);

        // 不可控轴直接隐藏对应输入框
        if (xInput && !cfg.x) xInput.gameObject.SetActive(false);
        if (yInput && !cfg.y) yInput.gameObject.SetActive(false);
        if (zInput && !cfg.z) zInput.gameObject.SetActive(false);

        if (xInput) xInput.onEndEdit.AddListener(_ => ValidateAndApply());
        if (yInput) yInput.onEndEdit.AddListener(_ => ValidateAndApply());
        if (zInput) zInput.onEndEdit.AddListener(_ => ValidateAndApply());

        if (errorText) errorText.text = "";
        
        RefreshUI();
    }

    void ValidateAndApply()
    {
        var errors = new List<string>();
        float? xv = ReadAxis(xInput, cfg.x, cfg.limitX, "X", errors);
        float? yv = ReadAxis(yInput, cfg.y, cfg.limitY, "Y", errors);
        float? zv = ReadAxis(zInput, cfg.z, cfg.limitZ, "Z", errors);

        if (errors.Count > 0)
        {
            if (errorText) errorText.text = string.Join("；", errors);
            return;
        }

        if (errorText) errorText.text = "";
        controller.SetJointAngle(jointName, xv, yv, zv);
    }

    float? ReadAxis(InputField inp, bool enabledAxis, JointLimit lim, string axisLabel, List<string> errors)
    {
        if (!enabledAxis || inp == null || !inp.gameObject.activeSelf) return null;
        if (string.IsNullOrWhiteSpace(inp.text)) return null;

        string s = NormalizeNumberString(inp.text);

        if (!float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out float v))
        {
            errors.Add($"{axisLabel} 请输入数字");
            return null;
        }
        if (v < lim.min || v > lim.max)
        {
            errors.Add($"{axisLabel} 超出范围 ({lim.min} ~ {lim.max})");
            return null;
        }
        return v;
    }

    string NormalizeNumberString(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        s = s.Trim().Replace("°", "").Replace("，", ".").Replace("。", ".");
        var sb = new StringBuilder(s.Length);
        foreach (var ch in s)
        {
            if (ch >= '０' && ch <= '９') sb.Append((char)('0' + (ch - '０')));
            else if (ch == '－') sb.Append('-');
            else if (ch == '．') sb.Append('.');
            else sb.Append(ch);
        }
        return sb.ToString();
    }
    
    // InputUI.cs 中新增：
    public void SetJointName(string newName)
    {
        if (string.IsNullOrEmpty(newName) || controller == null || !controller.HasJoint(newName)) return;
        jointName = newName;
        cfg = controller.GetConfig(jointName);
        RefreshUI();
    }

// InputUI.cs 中新增（用于根据 cfg 显示/隐藏 X/Y/Z 输入框、清空错误）
    public void RefreshUI()
    {
        if (cfg == null) return;
        if (xInput) xInput.gameObject.SetActive(cfg.x);
        if (yInput) yInput.gameObject.SetActive(cfg.y);
        if (zInput) zInput.gameObject.SetActive(cfg.z);
        if (errorText) errorText.text = "";
    }
    public void ResetAllJointsButton()
    {
        if (controller == null) return;

        controller.ResetAllJoints();

        // 清空当前面板的输入与错误提示（可选）
        if (xInput) xInput.text = "X Input";
        if (yInput) yInput.text = "Y Input";
        if (zInput) zInput.text = "Z Input";
        if (errorText) errorText.text = "";
    }

    
}
