using System.Collections.Generic;

public class StateComponent : IComponent
{
    public TopState Current;
    public TopState Previous;
    public float StateTimer;
    public int CurrentPriority;

    public readonly List<StateChangeRequest> PendingRequests = new(4);

    public StateComponent()
    {
        Current = TopState.Locomotion;
        Previous = TopState.Locomotion;
        CurrentPriority = StatePriorities.Locomotion;
    }

    public void RequestChange(TopState target, int priority, string source = null)
    {
        PendingRequests.Add(new StateChangeRequest
        {
            TargetState = target,
            Priority = priority,
            Source = source ?? "Unknown"
        });
    }

    public bool IsState(TopState state) => Current == state;
}

public enum TopState
{
    Locomotion,
    InCombo,
    HitStun,
    KnockDown,
    Dodge,
    Death,
    Disabled
}

public static class StatePriorities
{
    public const int Locomotion = 0;
    public const int InCombo    = 10;
    public const int Dodge      = 20;
    public const int HitStun    = 30;
    public const int KnockUp    = 35;
    public const int KnockDown  = 40;
    public const int Death      = 100;
}

public class StateChangeRequest
{
    public TopState TargetState;
    public int Priority;
    public string Source;

    public override string ToString() =>
        $"Request({TargetState}, pri={Priority}, src={Source})";
}

public class StateChangedEvent : IEvent
{
    public Entity Entity;
    public TopState From;
    public TopState To;
    public string Source;
}