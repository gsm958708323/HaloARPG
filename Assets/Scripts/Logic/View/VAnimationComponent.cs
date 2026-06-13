using UnityEngine;

public class VAnimationComponent : IViewComponent
{
    private Animator _animator;

    // Animator 参数名（避免字符串散落）
    private static readonly int Speed = Animator.StringToHash("Speed");

    public override void OnAttach(ViewEntity viewEntity, World world)
    {
        base.OnAttach(viewEntity, world);
        _animator = viewEntity.GameObject.GetComponent<Animator>();
    }

    public override void OnUpdate(float deltaTime)
    {
        var input = world.GetComponent<InputComponent>(viewEntity.LogicEntity);
        var state = world.GetComponent<StateComponent>(viewEntity.LogicEntity);
        if (input == null || state == null) { return; }

        // Locomotion 状态：用 Speed 参数驱动 BlendTree
        if (state.IsState(TopState.Locomotion))
        {
            float targetSpeed = input.HasMoveInput ? 1f : 0f;
            float current = _animator.GetFloat(Speed);
            _animator.SetFloat(Speed, Mathf.Lerp(current, targetSpeed, 10f * deltaTime));
        }
    }

    // 供后续步骤调用
    public void CrossFade(string stateName, float duration = 0.1f)
    {
        _animator.CrossFadeInFixedTime(stateName, duration);
    }
}