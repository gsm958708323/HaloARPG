using System;
using UnityEngine;

public class VAnimationComponent : IViewComponent
{
    private Animator _animator;

    // Animator 参数名（避免字符串散落）
    private static readonly int ParamSpeed = Animator.StringToHash("Speed");
    private string _pendingClip;
    private float _pendingCrossFade;

    public override void OnAttach(ViewEntity viewEntity, World world)
    {
        base.OnAttach(viewEntity, world);
        _animator = viewEntity.GameObject.GetComponent<Animator>();
        world.Events.Subscribe<ComboNodeChangedEvent>(OnComboNodeChanged);
        world.Events.Subscribe<StateChangedEvent>(OnStateChanged);
    }

    override public void OnDetach()
    {
        world.Events.Unsubscribe<ComboNodeChangedEvent>(OnComboNodeChanged);
        world.Events.Unsubscribe<StateChangedEvent>(OnStateChanged);
        base.OnDetach();
    }

    public override void OnUpdate(float deltaTime)
    {
        if (_animator == null) return;
        // 执行待播放动画
        if (_pendingClip != null)
        {
            _animator.CrossFadeInFixedTime(_pendingClip, _pendingCrossFade);
            _pendingClip = null;
        }

        // Locomotion 时驱动 BlendTree
        var state = world.GetComponent<StateComponent>(viewEntity.LogicEntity);
        if (state == null || state.Current != TopState.Locomotion) return;

        var input = world.GetComponent<InputComponent>(viewEntity.LogicEntity);
        if (input == null) return;

        float target = input.HasMoveInput ? 1f : 0f;
        float current = _animator.GetFloat(ParamSpeed);
        _animator.SetFloat(ParamSpeed, Mathf.Lerp(current, target, 10f * deltaTime));
    }

    // 供后续步骤调用
    public void CrossFade(string stateName, float duration = 0.1f)
    {
        _animator.CrossFadeInFixedTime(stateName, duration);
    }


    private void OnStateChanged(StateChangedEvent evt)
    {
        if (evt.Entity != viewEntity.LogicEntity) return;
        switch (evt.To)
        {
            case TopState.Locomotion:
                _pendingClip = "Locomotion";
                _pendingCrossFade = 0.15f;
                break;
            case TopState.Dodge:
                _pendingClip = "Dodge";
                _pendingCrossFade = 0.08f;
                break;
                // 后续扩展：HitStun, Death 等
        }
    }

    private void OnComboNodeChanged(ComboNodeChangedEvent evt)
    {
        if (evt.Entity != viewEntity.LogicEntity) return;

        _pendingClip = evt.Node.animationClip;
        _pendingCrossFade = 0.05f;  // 攻击间快速过渡
    }

}