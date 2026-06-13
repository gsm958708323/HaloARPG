using UnityEngine;

public class MovementSystem : ISystem
{
    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);
        world.Query<InputComponent, TransformComponent, StateComponent>(
             (input, transform, state) =>
         {
             // 只有 Locomotion 状态才允许自由移动
             if (!state.IsState(TopState.Locomotion))
                 return;
             if (!input.HasMoveInput)
                 return;
             // 位置更新
             transform.Position += input.MoveDirection * transform.MoveSpeed * deltaTime;
             // 旋转：面朝移动方向
             var targetRot = Quaternion.LookRotation(input.MoveDirection);
             transform.Rotation = Quaternion.RotateTowards(
                 transform.Rotation,
                 targetRot,
                 transform.RotationSpeed * deltaTime
             );
         });
    }
}