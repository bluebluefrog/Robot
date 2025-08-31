using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DropBoxUI : MonoBehaviour
{
    public JointController controller; // 拖场景里的 JointController
    public Dropdown dropdown;          // 拖 UI Dropdown
    public InputUI inputUI;            // 拖你的 InputUI 面板脚本

    void Start()
    {
        if (controller == null || dropdown == null || inputUI == null)
        {
            Debug.LogError("JointDropdownUI: 请绑定 controller / dropdown / inputUI");
            enabled = false; return;
        }

        // 取全部关节名并排序
        var names = controller.GetAllConfigs().Keys.ToList();
        names.Sort(); // 可按需自定义分组/顺序

        // 填充下拉选项
        dropdown.ClearOptions();
        dropdown.AddOptions(names);

        // 默认选第一个（或你想要的初始关节）
        if (names.Count > 0)
        {
            dropdown.value = 0;
            dropdown.RefreshShownValue();
            inputUI.SetJointName(names[0]);
        }

        // 监听选择变化
        dropdown.onValueChanged.AddListener(idx =>
        {
            if (idx >= 0 && idx < names.Count)
            {
                inputUI.SetJointName(names[idx]);
            }
        });
    }
}