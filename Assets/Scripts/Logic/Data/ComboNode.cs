using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ComboNode
{
    public string nodeId;              // "light_1", "heavy_1", "launcher"
    public string animationClip;       // Animator 中的 State 名称
    public float duration;             // 节点总时长（秒）

    [Header("Input Window")]
    public float inputWindowStart;     // 可接招的开始时间
    public float inputWindowEnd;       // 可接招的结束时间

    [Header("Cancel Window")]
    public float cancelWindowStart;    // 可闪避取消的开始时间
    public float cancelWindowEnd;      // 可闪避取消的结束时间

    [Header("Edges")]
    public List<ComboEdge> edges;      // 分支列表

    [Header("Finisher")]
    public bool isFinisher;            // 是否终结招（到达后强制播完）
    public float recoveryLockTime;     // 终结招额外硬直
}