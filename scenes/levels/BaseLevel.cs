using Godot;
using System;

public partial class BaseLevel : Node3D
{
    private static readonly PackedScene _playerScene = GD.Load<PackedScene>("res://scenes/characters/player/player.tscn");

    public override void _Ready()
    {
        base._Ready();
        var playerSpawn = GetNode<Node3D>("%PlayerSpawn");
        var player = _playerScene.Instantiate<Player>();
        player.GlobalTransform = playerSpawn.GlobalTransform;
        AddChild(player);
    }

}
