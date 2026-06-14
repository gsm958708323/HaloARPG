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
    /// <summary>抢占式切换（打断），受优先级门槛限制</summary>
    public void RequestChange(TopState target, int priority, string source = null)
    {
        PendingRequests.Add(new StateChangeRequest
        {
            TargetState = target,
            Priority = priority,
            Kind = StateRequestKind.Interrupt,
            Source = source ?? "Unknown"
        });
    }
    /// <summary>当前状态自然退出，无条件回到目标基线状态</summary>
    public void RequestExit(TopState target, string source = null)
    {
        PendingRequests.Add(new StateChangeRequest
        {
            TargetState = target,
            Priority = 0,
            Kind = StateRequestKind.Exit,
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
    Disabled,
    KnockUp
}

public static class StatePriorities
{
    public const int Locomotion = 0;
    public const int InCombo = 10;
    public const int Dodge = 20;
    public const int HitStun = 30;
    public const int KnockUp = 35;
    public const int KnockDown = 40;
    public const int Death = 100;
    public static int Of(TopState state) => state switch
    {
        TopState.Locomotion => Locomotion,
        TopState.InCombo => InCombo,
        TopState.Dodge => Dodge,
        TopState.HitStun => HitStun,
        TopState.KnockUp => KnockUp,
        TopState.KnockDown => KnockDown,
        TopState.Death => Death,
        _ => 0
    };
}
public enum StateRequestKind
{
    Interrupt,   // 抢占当前状态，受优先级门槛限制
    Exit         // 当前状态拥有者宣告完成，无条件回到目标基线状态
}
public class StateChangeRequest
{
    public TopState TargetState;
    public int Priority;
    public StateRequestKind Kind;
    public string Source;
    public override string ToString() =>
        $"Request({TargetState}, {Kind}, pri={Priority}, src={Source})";
}

public class StateChangedEvent : IEvent
{
    public Entity Entity;
    public TopState From;
    public TopState To;
    public string Source;
}