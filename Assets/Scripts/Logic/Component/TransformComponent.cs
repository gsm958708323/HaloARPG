
using UnityEngine;

public class TransformComponent : IComponent
{
    public Vector3 Position;
    public Quaternion Rotation;
    // 移动相关
    public float MoveSpeed;
    public float RotationSpeed;     // 度/秒
}