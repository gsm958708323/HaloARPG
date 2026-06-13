using System;
using System.Collections.Generic;
using UnityEngine;

public class ViewEntityManager
{
    private readonly World _world;
    GameObject playerGo;
    private readonly Dictionary<int, ViewEntity> _map = new(); // entityId → ViewEntity
    private readonly List<ViewEntity> _all = new();

    public ViewEntityManager(World world, GameObject playerGo)
    {
        _world = world;
        this.playerGo = playerGo;
        _world.Events.Subscribe<EntityCreatedEvent>(OnEntityCreated);
        _world.Events.Subscribe<EntityDestroyedEvent>(OnEntityDestroyed);
    }

    public World World => _world;

    // --- 查询 ---
    public ViewEntity Get(Entity logicEntity)
    {
        _map.TryGetValue(logicEntity.Id, out var ve);
        return ve;
    }

    public void Query<T>(Action<ViewEntity, T> action) where T : IViewComponent
    {
        foreach (var ve in _all)
        {
            var comp = ve.Get<T>();
            if (comp != null)
                action(ve, comp);
        }
    }

    // --- 生命周期 ---
    public void UpdateAll(float deltaTime)
    {
        for (int i = 0; i < _all.Count; i++)
            _all[i].Update(deltaTime);
    }

    public void Shutdown()
    {
        foreach (var ve in _all)
            ve.Destroy();
        _all.Clear();
        _map.Clear();
    }

    // --- 事件响应 ---
    private void OnEntityCreated(EntityCreatedEvent evt)
    {
        var ve = EntityFactory.CreateViewPlayer(_world, evt.Entity, playerGo);
        _map[evt.Entity.Id] = ve;
        _all.Add(ve);
    }

    private void OnEntityDestroyed(EntityDestroyedEvent evt)
    {
        if (!_map.TryGetValue(evt.Entity.Id, out var ve))
            return;

        ve.Destroy();
        _map.Remove(evt.Entity.Id);
        _all.Remove(ve);
    }
}

public class EntityCreatedEvent : IEvent
{
    public Entity Entity;
}
public class EntityDestroyedEvent : IEvent
{
    public Entity Entity;
}

// todo 优化成system可选
