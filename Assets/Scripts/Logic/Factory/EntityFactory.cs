using UnityEngine;

public class EntityFactory
{
    public static Entity CreatePlayer(World world, Vector3 position)
    {
        var entity = world.CreateEntity();
        world.AddComponent(entity, new TransformComponent
        {
            Position = position,
            Rotation = Quaternion.identity,
            MoveSpeed = 5f,
            RotationSpeed = 720f
        });
        world.AddComponent(entity, new InputComponent());
        world.AddComponent(entity, new StateComponent
        {
            Current = TopState.Locomotion
        });
        world.Events.Publish(new EntityCreatedEvent { Entity = entity });
        return entity;
    }

    public static ViewEntity CreateViewPlayer(World world, Entity entity, GameObject playerGO)
    {
        var viewEntity = new ViewEntity(world, entity, playerGO);
        viewEntity.Add<VTransformComponent>();
        viewEntity.Add<VAnimationComponent>();

        return viewEntity;
    }
}