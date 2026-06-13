using UnityEngine;

public class VTransformComponent : IViewComponent
{
    private Transform _transform;

    // 插值参数
    private float _lerpSpeed = 15f;

    public override void OnAttach(ViewEntity viewEntity, World world)
    {
        base.OnAttach(viewEntity, world);
        _transform = viewEntity.GameObject.transform;

        var tc = world.GetComponent<TransformComponent>(viewEntity.LogicEntity);
        // 立即同步初始位置
        if (tc != null)
        {
            _transform.SetPositionAndRotation(tc.Position, tc.Rotation);
        }
    }

    public override void OnUpdate(float deltaTime)
    {
        var tc = world.GetComponent<TransformComponent>(viewEntity.LogicEntity);
        if (tc != null)
        {
            // 平滑插值（避免逻辑帧率和渲染帧率不一致时的抖动）
            _transform.position = Vector3.Lerp(_transform.position, tc.Position, _lerpSpeed * deltaTime);
            _transform.rotation = Quaternion.Slerp(_transform.rotation, tc.Rotation, _lerpSpeed * deltaTime);
        }
    }
}