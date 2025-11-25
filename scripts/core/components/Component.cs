using Godot;
using Scribe.Scripts.Core.Entities;

namespace Scribe.Scripts.Core.Components;

public abstract partial class Component : RefCounted
{
    protected Entity _entity;
    
    public void SetEntity(Entity entity)
    {
        _entity = entity;
    }
    
    public virtual void Initialize() { }
    
    public virtual void Process(double delta) { }
    
    public virtual void Cleanup() { }
    
    public virtual void OnReady() { }
}