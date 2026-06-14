using UnityEngine;

public class DodgeSystem : ISystem
{
    private const float DodgeDuration = 0.5f;
    private const float DodgeSpeed = 12f;

    public override void Tick(float deltaTime)
    {
        world.Query<StateComponent, TransformComponent, InputComponent>(
            (entity, state, transform, input) =>
        {
            if (state.Current != TopState.Dodge) return;

            // 闪避方向：有输入用输入方向，无输入用面朝方向
            Vector3 dir;
            if (input.HasMoveInput)
            {
                dir = input.MoveDirection;
            }
            else
            {
                dir = transform.Rotation * Vector3.forward;
            }

            transform.Position += dir * DodgeSpeed * deltaTime;

            // 闪避结束
            if (state.StateTimer >= DodgeDuration)
            {
                state.RequestExit(TopState.Locomotion, "Dodge:End");
            }
        });
    }
}