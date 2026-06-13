public class StateSystem : ISystem
{

    public override void Tick(float deltaTime)
    {
        world.Query<StateComponent>((state) =>
        {
            state.StateTimer += deltaTime;

            // 本步暂无状态切换逻辑
            // 后续步骤会在这里加入 HitStun 超时恢复、Death 判定等
        });
    }
}