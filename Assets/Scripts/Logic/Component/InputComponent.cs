using UnityEngine;

public class InputComponent : IComponent
{
    // 移动（归一化方向）
    public Vector3 MoveDirection;    // 世界空间方向，长度 0~1
    public bool HasMoveInput;

    // 攻击（后续步骤使用）
    public bool LightAttackPressed;
    public bool HeavyAttackPressed;
    public bool DodgePressed;

    public void Clear()
    {
        MoveDirection = Vector3.zero;
        HasMoveInput = false;
        LightAttackPressed = false;
        HeavyAttackPressed = false;
        DodgePressed = false;
    }
}