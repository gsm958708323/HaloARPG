public class ComboSystem : ISystem
{
    public override void Tick(float deltaTime)
    {
        world.Query<InputComponent, ComboComponent, StateComponent>(
            (entity, input, combo, state) =>
        {
            combo.TotalTime += deltaTime;
            RecordInputs(input, combo);

            // ====== 帧头一致性检查 ======
            if (combo.InCombo && state.Current != TopState.InCombo)
            {
                // 被外部打断（受击/死亡/闪避等），清理连招数据
                // DeactivateHitbox(entity);
                CleanupCombo(combo);
                return;
            }

            if (!combo.InCombo && state.Current == TopState.InCombo)
            {
                // 异常防御：状态显示在连招但没有节点数据
                state.RequestExit(TopState.Locomotion, "Combo:Fallback");
                return;
            }

            // ====== 主逻辑 ======
            if (!combo.InCombo)
                HandleIdle(entity, combo, state);
            else
                HandleInCombo(entity, combo, state, deltaTime);
        });
    }

    // ------ 输入记录 ------
    private void RecordInputs(InputComponent input, ComboComponent combo)
    {
        if (input.LightAttackPressed)
            combo.Buffer.Record(CombatInputType.LightAttack, combo.TotalTime);
        if (input.HeavyAttackPressed)
            combo.Buffer.Record(CombatInputType.HeavyAttack, combo.TotalTime);
        if (input.DodgePressed)
            combo.Buffer.Record(CombatInputType.Dodge, combo.TotalTime);
    }

    // ------ 不在连招中 ------
    private void HandleIdle(Entity entity, ComboComponent combo, StateComponent state)
    {
        if (state.Current != TopState.Locomotion) return;

        var buffered = combo.Buffer.Peek(combo.TotalTime);
        if (buffered == null) return;

        // 闲置时闪避
        if (buffered.Input == CombatInputType.Dodge)
        {
            combo.Buffer.Consume();
            state.RequestChange(TopState.Dodge,
                StatePriorities.Dodge, "Combo:IdleDodge");
            return;
        }

        // 进入连招
        var entryNode = combo.Tree.FindEntryNode(buffered.Input);
        if (entryNode != null)
        {
            combo.Buffer.Consume();
            EnterNode(entity, combo, state, entryNode);
        }
    }

    // ------ 在连招中 ------
    private void HandleInCombo(Entity entity, ComboComponent combo,
        StateComponent state, float deltaTime)
    {
        combo.NodeTimer += deltaTime;
        var node = combo.CurrentNode;
        float t = combo.NodeTimer;

        //   // 驱动 Hitbox
        //     UpdateHitbox(entity, node, t);
        //     // 驱动定时触发效果
        //     ProcessTimedActions(entity, node, t);

        // 1. 取消窗口 → 闪避
        if (t >= node.cancelWindowStart && t <= node.cancelWindowEnd)
        {
            var buffered = combo.Buffer.Peek(combo.TotalTime);
            if (buffered != null && buffered.Input == CombatInputType.Dodge)
            {
                combo.Buffer.Consume();
                state.RequestChange(TopState.Dodge,
                    StatePriorities.Dodge, "Combo:CancelDodge");
                // 不清理连招数据 → 下帧帧头检测处理
                return;
            }
        }

        // 2. 输入窗口 → 接招
        if (t >= node.inputWindowStart && t <= node.inputWindowEnd)
        {
            var buffered = combo.Buffer.Peek(combo.TotalTime);
            if (buffered != null && buffered.Input != CombatInputType.Dodge)
            {
                var edge = combo.Tree.MatchEdge(node, buffered.Input);
                if (edge != null)
                {
                    var nextNode = combo.Tree.GetNode(edge.targetNodeId);
                    if (nextNode != null)
                    {
                        combo.Buffer.Consume();
                        EnterNode(entity, combo, state, nextNode);
                        return;
                    }
                }
            }
        }

        // 3. 超时：节点播完
        float totalDuration = node.isFinisher
            ? node.duration + node.recoveryLockTime
            : node.duration;
        if (t >= totalDuration)
        {
            // DeactivateHitbox(entity);
            CleanupCombo(combo);
            state.RequestExit(TopState.Locomotion, "Combo:Timeout");
        }
    }

    // ------ 辅助 ------
    private void EnterNode(Entity entity, ComboComponent combo,
        StateComponent state, ComboNode node)
    {
        combo.CurrentNode = node;
        combo.NodeTimer = 0f;
        if (state.Current != TopState.InCombo)
        {
            state.RequestChange(TopState.InCombo,
                StatePriorities.InCombo, "Combo:Enter");
        }
        // // 重置定时触发标记
        // if (node.timedActions != null)
        // {
        //     foreach (var action in node.timedActions)
        //         action.triggered = false;
        // }
        world.Events.Publish(new ComboNodeChangedEvent
        {
            Entity = entity,
            Node = node
        });
    }

    private void CleanupCombo(ComboComponent combo)
    {
        combo.CurrentNode = null;
        combo.NodeTimer = 0f;
        combo.Buffer.Clear();
    }
}

public class ComboNodeChangedEvent : IEvent
{
    public Entity Entity { get; set; }
    public ComboNode Node { get; set; }
}