public class StateSystem : ISystem
{
    public override void Tick(float deltaTime)
    {
        world.Query<StateComponent>((entity, state) =>
        {
            state.StateTimer += deltaTime;
            if (state.PendingRequests.Count == 0) return;
            // 分离 Interrupt 和 Exit
            StateChangeRequest bestInterrupt = null;
            StateChangeRequest exit = null;
            foreach (var req in state.PendingRequests)
            {
                if (req.Kind == StateRequestKind.Exit)
                {
                    exit = req;  // 后到的 Exit 覆盖前者
                }
                else
                {
                    if (bestInterrupt == null || req.Priority > bestInterrupt.Priority)
                        bestInterrupt = req;
                }
            }
            // 1. Interrupt 优先：priority 够高才能抢占
            if (bestInterrupt != null
                && bestInterrupt.TargetState != state.Current
                && bestInterrupt.Priority >= state.CurrentPriority)
            {
                ExecuteTransition(entity, state,
                    bestInterrupt.TargetState, bestInterrupt.Source);
            }
            // 2. 否则处理 Exit：无条件允许（拥有者宣告完成）
            else if (exit != null && exit.TargetState != state.Current)
            {
                ExecuteTransition(entity, state, exit.TargetState, exit.Source);
            }
#if UNITY_EDITOR
            else if (bestInterrupt != null && bestInterrupt.TargetState != state.Current)
            {
                UnityEngine.Debug.Log(
                    $"[State] {entity} REJECTED interrupt {bestInterrupt} " +
                    $"(current={state.Current}, pri={state.CurrentPriority})");
            }
#endif
            state.PendingRequests.Clear();
        });
    }
    private void ExecuteTransition(Entity entity, StateComponent state,
        TopState target, string source)
    {
        var from = state.Current;
        state.Previous = from;
        state.Current = target;
        state.StateTimer = 0f;
        state.CurrentPriority = StatePriorities.Of(target);  // 用规范优先级
        UnityEngine.Debug.Log(
            $"[State] {entity} TRANSITION {from} -> {target} by {source}");
        world.Events.Publish(new StateChangedEvent
        {
            Entity = entity,
            From = from,
            To = target,
            Source = source
        });
    }
}