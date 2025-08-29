using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable] public class JointLimit { public float min; public float max; }
[Serializable] public class JointConfig {
    public string name;
    public string path;
    public bool x, y, z;
    public JointLimit limitX, limitY, limitZ;
}
[Serializable] public class JointsConfig { public JointConfig[] joints; }

[Serializable] public class AxisAngles { public float? x, y, z; }
[Serializable] public class Pose : Dictionary<string, AxisAngles> { }