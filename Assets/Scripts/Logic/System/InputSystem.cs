using UnityEngine;

public class InputSystem : ISystem
{
    public override void Tick(float delteTime)
    {
        base.Tick(delteTime);
        world.Query<InputComponent>((input) =>
        {
            input.Clear();
            // 移动
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            var camForward = Camera.main.transform.forward;
            var camRight = Camera.main.transform.right;
            camForward.y = 0; camForward.Normalize();
            camRight.y = 0; camRight.Normalize();
            var dir = camForward * v + camRight * h;
            if (dir.sqrMagnitude > 0.01f)
            {
                input.MoveDirection = dir.normalized;
                input.HasMoveInput = true;
            }
            // 攻击（本步暂不处理，留接口）
            input.LightAttackPressed = Input.GetMouseButtonDown(0);
            input.HeavyAttackPressed = Input.GetMouseButtonDown(1);
            input.DodgePressed = Input.GetKeyDown(KeyCode.LeftShift);
        });
    }
}