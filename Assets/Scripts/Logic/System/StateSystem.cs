public class StateSystem : ISystem
{
    public override void Tick(float deltaTime)
    {
        world.Query<StateComponent>((entity, state) =>
        {
            state.StateTimer += deltaTime;

            if (state.PendingRequests.Count == 0) return;

            // 找最高优先级
            StateChangeRequest winner = null;
            foreach (var req in state.PendingRequests)
            {
                if (winner == null || req.Priority > winner.Priority)
                    winner = req;
            }

            // 裁决
            if (winner != null && CanTransition(state, winner))
            {
                ExecuteTransition(entity, state, winner);
            }
            #if UNITY_EDITOR
            else if (winner != null)
            {
                UnityEngine.Debug.Log(
                    $"[State] {entity} REJECTED {winner} " +
                    $"(current={state.Current}, pri={state.CurrentPriority})");
            }
            #endif

            state.PendingRequests.Clear();
        });
    }

    private bool CanTransition(StateComponent state, StateChangeRequest req)
    {
        if (req.TargetState == state.Current)
            return false;   // 不重复切到相同状态
        return req.Priority >= state.CurrentPriority;
    }

    private void ExecuteTransition(Entity entity, StateComponent state, StateChangeRequest req)
    {
        var from = state.Current;
        state.Previous = from;
        state.Current = req.TargetState;
        state.StateTimer = 0f;
        state.CurrentPriority = req.Priority;

        world.Events.Publish(new StateChangedEvent
        {
            Entity = entity,
            From = from,
            To = req.TargetState,
            Source = req.Source
        });
    }
}