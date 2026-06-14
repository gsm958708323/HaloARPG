using UnityEngine;

public class MovementSystem : ISystem
{
    public override void Tick(float deltaTime)
    {
        world.Query<InputComponent, TransformComponent, StateComponent>(
            (entity, input, transform, state) =>
        {
            // 只有 Locomotion 允许自由移动
            if (state.Current != TopState.Locomotion) return;
            if (!input.HasMoveInput) return;

            transform.Position += input.MoveDirection * transform.MoveSpeed * deltaTime;

            var targetRot = Quaternion.LookRotation(input.MoveDirection);
            transform.Rotation = Quaternion.RotateTowards(
                transform.Rotation,
                targetRot,
                transform.RotationSpeed * deltaTime
            );
        });
    }
}