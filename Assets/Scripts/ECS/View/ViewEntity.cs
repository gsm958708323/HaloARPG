using System.Collections.Generic;
using UnityEngine;

public class ViewEntity
{
    public Entity LogicEntity { get; private set; }
    public World World { get; private set; }
    public GameObject GameObject { get; private set; }

    private readonly List<IViewComponent> _components = new();

    public ViewEntity(World world, Entity logicEntity, GameObject go)
    {
        LogicEntity = logicEntity;
        GameObject = go;
        World = world;
    }

    public T Add<T>() where T : IViewComponent, new()
    {
        var component = new T();
        _components.Add(component);
        component.OnAttach(this, World);
        return component;
    }

    public T Get<T>() where T : IViewComponent
    {
        foreach (var c in _components)
            if (c is T t) return t;
        return null;
    }

    public void Remove<T>() where T : IViewComponent
    {
        for (int i = _components.Count - 1; i >= 0; i--)
        {
            if (_components[i] is T t)
            {
                t.OnDetach();
                _components.RemoveAt(i);
                return;
            }
        }
    }

    public bool Has<T>() where T : IViewComponent
    {
        foreach (var c in _components)
            if (c is T) return true;
        return false;
    }

    public void Update(float deltaTime)
    {
        for (int i = 0; i < _components.Count; i++)
            _components[i].OnUpdate(deltaTime);
    }

    public void Destroy()
    {
        for (int i = _components.Count - 1; i >= 0; i--)
            _components[i].OnDetach();
        _components.Clear();

        if (GameObject != null)
            Object.Destroy(GameObject);
    }
}
public class IViewComponent
{
    protected ViewEntity viewEntity;
    protected World world;
    public virtual void OnAttach(ViewEntity viewEntity, World world)
    {
        this.viewEntity = viewEntity;
        this.world = world;
    }
    public virtual void OnUpdate(float deltaTime) { }
    public virtual void OnDetach() { }
}
