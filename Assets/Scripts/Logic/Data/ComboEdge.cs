[System.Serializable]
public class ComboEdge
{
    public CombatInputType requiredInput;
    public string targetNodeId;        // 跳转目标节点的 nodeId
    public int priority;               // 多条同时满足时取最高
}