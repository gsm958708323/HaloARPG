using System;
using System.Collections;
using System.Collections.Generic;

public class World
{
    public EventBus Events;

    private int entityIdCounter;
    private readonly Dictionary<int, int> entityVersions = new();
    private readonly HashSet<int> aliveEntities = new();
    private readonly Stack<int> freeIds = new();
    private readonly List<Entity> waitDestroy = new();

    // ============================================================
    // Component 存储
    // Component 中心存储：按类型分组，每组按 EntityId 索引
    // ============================================================
    private readonly Dictionary<Type, Dictionary<int, IComponent>> stores = new();

    // ============================================================
    // System 管理
    // ============================================================
    private readonly SortedList<int, List<ISystem>> systems = new();

    public World(EventBus eventBus)
    {
        this.Events = eventBus;
    }

    // ============================================================
    //  Entity 生命周期
    // ============================================================
    public Entity CreateEntity()
    {
        int id;
        int version;

        if (freeIds.Count > 0)
        {
            id = freeIds.Pop();
            version = ++entityVersions[id];
        }
        else
        {
            id = ++entityIdCounter;
            version = 1;
            entityVersions[id] = version;
        }

        aliveEntities.Add(id);
        return new Entity(id, version);
    }

    public void DestroyEntity(Entity entity)
    {
        // 下一帧销毁，防止迭代器失效
        if (!IsAlive(entity)) return;
        waitDestroy.Add(entity);
    }

    public bool IsAlive(Entity entity)
    {
        if (entity.IsNull) return false;
        return aliveEntities.Contains(entity.Id)
        && entityVersions.TryGetValue(entity.Id, out int ver)
        && ver == entity.Version;
    }

    // ============================================================
    //  Component 操作
    // ============================================================

    public void AddComponent(Entity entity, IComponent component)
    {
        if (!IsAlive(entity))
            throw new InvalidOperationException(
                $"entity已销毁 {entity}");

        var type = component.GetType();
        if (!stores.TryGetValue(type, out var store))
        {
            store = new Dictionary<int, IComponent>();
            stores[type] = store;
        }

        store[entity.Id] = component;
    }

    public T AddComponent<T>(Entity entity) where T : class, IComponent, new()
    {
        if (!IsAlive(entity))
            throw new InvalidOperationException(
                $"entity已销毁 {entity}");

        var type = typeof(T);
        if (!stores.TryGetValue(type, out var store))
        {
            store = new Dictionary<int, IComponent>();
            stores[type] = store;
        }
        if (store.TryGetValue(entity.Id, out var existing))
            return (T)existing;

        var comp = new T();
        store[entity.Id] = comp;
        return comp;
    }

    public void RemoveComponent<T>(Entity entity) where T : class, IComponent, new()
    {
        var type = typeof(T);
        if (!stores.TryGetValue(type, out var store)) return;
        if (!store.TryGetValue(entity.Id, out var comp)) return;

        store.Remove(entity.Id);
    }

    public T GetComponent<T>(Entity entity) where T : class, IComponent
    {
        var type = typeof(T);
        if (!stores.TryGetValue(type, out var store)) return null;
        return store.TryGetValue(entity.Id, out var comp) ? (T)comp : null;
    }

    public bool HasComponent<T>(Entity entity) where T : class, IComponent
    {
        var type = typeof(T);
        return stores.TryGetValue(type, out var store)
            && store.ContainsKey(entity.Id);
    }

    // ============================================================
    //  Query — 查询拥有指定 Component 组合的所有 Entity
    // ============================================================
    public void Query<T1>(Action<T1> action) where T1 : class, IComponent
    {
        var type1 = typeof(T1);
        if (!stores.TryGetValue(type1, out var store1)) return;
        foreach (var item in store1)
        {
            int id = item.Key;
            if (!aliveEntities.Contains(id)) continue;
            action((T1)item.Value);
        }
    }

    public void Query<T1, T2>(Action<T1, T2> action)
        where T1 : class, IComponent
        where T2 : class, IComponent
    {
        var type1 = typeof(T1);
        var type2 = typeof(T2);

        if (!stores.TryGetValue(type1, out var store1)) return;
        if (!stores.TryGetValue(type2, out var store2)) return;

        // 遍历较小的集合
        var smaller = store1.Count <= store2.Count ? store1 : store2;
        var other = smaller == store1 ? store2 : store1;

        foreach (var kvp in smaller)
        {
            int id = kvp.Key;
            if (!aliveEntities.Contains(id)) continue;
            if (!other.ContainsKey(id)) continue;
            action((T1)store1[id], (T2)store2[id]);
        }
    }

    public void Query<T1, T2, T3>(Action<T1, T2, T3> action)
        where T1 : class, IComponent
        where T2 : class, IComponent
        where T3 : class, IComponent
    {
        var type1 = typeof(T1);
        var type2 = typeof(T2);
        var type3 = typeof(T3);

        if (!stores.TryGetValue(type1, out var store1)) return;
        if (!stores.TryGetValue(type2, out var store2)) return;
        if (!stores.TryGetValue(type3, out var store3)) return;

        // 找最小集合作为遍历起点
        var smallest = store1;
        if (store2.Count < smallest.Count) smallest = store2;
        if (store3.Count < smallest.Count) smallest = store3;

        foreach (var kvp in smallest)
        {
            int id = kvp.Key;
            if (!aliveEntities.Contains(id)) continue;
            if (!store1.ContainsKey(id)) continue;
            if (!store2.ContainsKey(id)) continue;
            if (!store3.ContainsKey(id)) continue;
            action((T1)store1[id], (T2)store2[id], (T3)store3[id]);
        }
    }

    // ============================================================
    //  System 管理
    // ============================================================

    public void RegisterSystem(ISystem system, int order)
    {
        if (!systems.TryGetValue(order, out var list))
        {
            list = new List<ISystem>();
            systems[order] = list;
        }
        list.Add(system);
        system.Init(this);
    }

    /// <summary>
    /// 每帧调用。按 order 从小到大依次执行所有 System，
    /// 最后执行延迟销毁。
    /// </summary>
    public void Tick(float deltaTime)
    {
        foreach (var kvp in systems)
        {
            foreach (var system in kvp.Value)
            {
                system.Tick(deltaTime);
            }
        }

        FlushDestroy();
    }

    public void Destroy()
    {
        foreach (var kvp in systems)
        {
            foreach (var system in kvp.Value)
            {
                system.Destroy();
            }
        }
        systems.Clear();
    }

    private void FlushDestroy()
    {
        foreach (var entity in waitDestroy)
        {
            if (!aliveEntities.Contains(entity.Id)) continue;

            foreach (var item in stores)
            {
                if (item.Value.TryGetValue(entity.Id, out var comp))
                {
                    item.Value.Remove(entity.Id);
                    // todo 组件回池
                }
            }

            aliveEntities.Remove(entity.Id);
            freeIds.Push(entity.Id);
        }
        waitDestroy.Clear();
    }
}

/// <summary>
/// 组件是引用类型，虽然比值类型慢一些，但是开发边界，组件使用引用类型来存储数据，支持多态，方便开发
/// </summary>
public interface IComponent { }
public abstract class ISystem
{
    protected World world;
    public virtual void Init(World world)
    {
        this.world = world;
    }
    public virtual void Tick(float deltaTime) { }
    public virtual void Destroy() { }
}

