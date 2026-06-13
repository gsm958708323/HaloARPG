public class StateComponent : IComponent
{
    public TopState Current;
    public TopState Previous;
    public float StateTimer;        // 当前状态已持续时间
    public bool IsState(TopState state) => Current == state;
    public void ChangeState(TopState newState)
    {
        Previous = Current;
        Current = newState;
        StateTimer = 0f;
    }
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