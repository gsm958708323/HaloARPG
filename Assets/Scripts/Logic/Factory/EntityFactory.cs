using UnityEngine;

public class EntityFactory
{
    public static Entity CreatePlayer(World world, Vector3 position, EntityConfig config)
    {
        var entity = world.CreateEntity();
        world.AddComponent(entity, new TransformComponent
        {
            Position = position,
            Rotation = Quaternion.identity,
            MoveSpeed = config.MoveSpeed,
            RotationSpeed = config.RotationSpeed
        });

        world.AddComponent(entity, new InputComponent());
        world.AddComponent(entity, new StateComponent());
        world.AddComponent(entity, new ComboComponent(config.ComboTree));
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