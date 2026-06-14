using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ARPG/ComboTreeConfig")]
public class ComboTreeConfig : ScriptableObject
{
    public string treeId;
    public List<ComboNode> nodes;

    // --- 运行时查询 ---
    private Dictionary<string, ComboNode> _nodeMap;

    public void Initialize()
    {
        _nodeMap = new Dictionary<string, ComboNode>();
        foreach (var node in nodes)
            _nodeMap[node.nodeId] = node;
    }

    public ComboNode GetNode(string nodeId)
    {
        if (_nodeMap == null) Initialize();
        return _nodeMap.TryGetValue(nodeId, out var node) ? node : null;
    }

    /// <summary>从 Root 查找入口节点（第一次按键时匹配）</summary>
    public ComboNode FindEntryNode(CombatInputType input)
    {
        // 约定：nodes[0] 是虚拟 Root，其 edges 定义入口分支
        if (nodes == null || nodes.Count == 0) return null;
        var root = nodes[0];
        var edge = MatchEdge(root, input);
        return edge != null ? GetNode(edge.targetNodeId) : null;
    }

    public ComboEdge MatchEdge(ComboNode node, CombatInputType input)
    {
        if (node.edges == null) return null;

        ComboEdge best = null;
        foreach (var edge in node.edges)
        {
            if (edge.requiredInput != input) continue;
            if (best == null || edge.priority > best.priority)
                best = edge;
        }
        return best;
    }
}