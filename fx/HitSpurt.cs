using Godot;
using System;

public partial class HitSpurt : Node3D
{   
    private static readonly PackedScene _hitSpurtScene = GD.Load<PackedScene>("res://fx/hit_spurt.tscn");
    public static void CreateHitSpurt (Vector3 position)
    {
        var hitSpurt = _hitSpurtScene.Instantiate<HitSpurt>();
        GameState.Instance.CurrentLevel.AddChild(hitSpurt);
        hitSpurt.GlobalPosition = position;
    }
    public override void _Ready()
    {
        var sparks = GetNode<GpuParticles3D>("%Sparks");
        sparks.Emitting = true;
        sparks.Finished += () => QueueFree();
    }

}
