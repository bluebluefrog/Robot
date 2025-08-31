using System;
using System.Collections.Generic;
using UnityEngine;

public class JointController : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public Transform robotRoot;      // 指到模型根（含 Armature）
    public TextAsset jointsJson;     // 指到 Assets/Configs/joints.json

    private readonly Dictionary<string, Transform> jointMap = new();
    private readonly Dictionary<string, JointConfig> cfgMap = new();
    public Pose CurrentPose = new Pose();
    private bool ready;
    
    // 在 JointController 类字段区新增：
    private readonly Dictionary<string, Vector3> _initialLocalEuler = new();


    void Awake()
    {
        if (robotRoot == null || jointsJson == null)
        {
            Debug.LogError("JointController: 请设置 robotRoot 和 jointsJson");
            return;
        }
        var cfg = JsonUtility.FromJson<JointsConfig>(jointsJson.text);
        foreach (var jc in cfg.joints)
        {
            var t = robotRoot.Find(jc.path);
            // 你原来的 foreach (var jc in cfg.joints) { ... } 里，找到到 t 后加：
            if (t != null)
            {
                jointMap[jc.name] = t;
                // 记录初始局部欧拉角（按导入时的姿态）
                if (!_initialLocalEuler.ContainsKey(jc.name))
                    _initialLocalEuler[jc.name] = t.localEulerAngles;
            }
            
            if (t == null) Debug.LogWarning($"找不到关节路径: {jc.path}");
            else jointMap[jc.name] = t;
            cfgMap[jc.name] = jc;
            CurrentPose[jc.name] = new AxisAngles();
            
      

        }
        ready = true;
        
    }
    
    public void ResetAllJoints()
    {
        foreach (var kv in jointMap)
        {
            string name = kv.Key;
            var t = kv.Value;

            // 如果记录了初始角度，用它；否则回零
            if (_initialLocalEuler.TryGetValue(name, out var euler))
                t.localEulerAngles = euler;
            else
                t.localEulerAngles = Vector3.zero;

            // 同步内存中的姿态缓存（这里清空，表示“未设置”）
            if (CurrentPose.ContainsKey(name))
                CurrentPose[name] = new AxisAngles();
            else
                CurrentPose.Add(name, new AxisAngles());
        }
    }


    public bool HasJoint(string name) => ready && jointMap.ContainsKey(name);
    public JointConfig GetConfig(string name) => cfgMap.TryGetValue(name, out var c) ? c : null;

    public void SetJointAngle(string name, float? x, float? y, float? z)
    {
        if (!HasJoint(name)) return;
        var t   = jointMap[name];
        var cfg = cfgMap[name];

        var e = t.localEulerAngles;
        e.x = Norm180(e.x); e.y = Norm180(e.y); e.z = Norm180(e.z);

        if (cfg.x && x.HasValue) e.x = Mathf.Clamp(Norm180(x.Value), cfg.limitX.min, cfg.limitX.max);
        if (cfg.y && y.HasValue) e.y = Mathf.Clamp(Norm180(y.Value), cfg.limitY.min, cfg.limitY.max);
        if (cfg.z && z.HasValue) e.z = Mathf.Clamp(Norm180(z.Value), cfg.limitZ.min, cfg.limitZ.max);

        t.localEulerAngles = e;

        var a = CurrentPose[name] ?? new AxisAngles();
        if (x.HasValue) a.x = Norm180(x.Value);
        if (y.HasValue) a.y = Norm180(y.Value);
        if (z.HasValue) a.z = Norm180(z.Value);
        CurrentPose[name] = a;
    }

    public void ResetJoint(string name)
    {
        if (!HasJoint(name)) return;
        jointMap[name].localEulerAngles = Vector3.zero;
        CurrentPose[name] = new AxisAngles();
    }

    static float Norm180(float d)
    {
        d %= 360f;
        if (d > 180f) d -= 360f;
        if (d < -180f) d += 360f;
        return d;
    }
    
    // JointController.cs 里新增：
    public IReadOnlyDictionary<string, JointConfig> GetAllConfigs()
    {
        // cfgMap 是你已有的 name -> JointConfig 映射（前文版本里有）
        return cfgMap;
    }

}
