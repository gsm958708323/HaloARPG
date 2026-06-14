public class ComboComponent : IComponent
{
    // --- 连招树引用 ---
    public ComboTreeConfig Tree;

    // --- 运行时状态 ---
    public ComboNode CurrentNode;          // null = 不在连招中
    public float NodeTimer;                // 当前节点已执行时间
    public float TotalTime;                // 全局计时器（用于 Buffer 过期判断）

    // --- 输入缓冲 ---
    public InputBuffer Buffer;

    // --- 便捷属性 ---
    public bool InCombo => CurrentNode != null;

    public ComboComponent(ComboTreeConfig tree, float bufferWindow = 0.15f)
    {
        Tree = tree;
        Buffer = new InputBuffer(bufferWindow);
    }
}