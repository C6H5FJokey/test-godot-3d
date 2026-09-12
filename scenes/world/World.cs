using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

public partial class World : Node3D
{
    public List<PackedScene> Levels = [
        GD.Load<PackedScene>("res://scenes/levels/level_01_welcome.tscn"),
    ];

    public int CurrentLevelIndex = 0;
    public BaseLevel CurrentLoadedLevel;

    public override void _Ready()
    {
        base._Ready();
        Input.MouseMode = Input.MouseModeEnum.Captured;
        LoadLevel(CurrentLevelIndex);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel"))
        {
            Input.MouseMode = Input.MouseMode == Input.MouseModeEnum.Captured ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured;
        }
    }

    public void LoadLevel(int index)
    {
        if (CurrentLoadedLevel != null)
        {
            CurrentLoadedLevel.QueueFree();
        }
        if (index < Levels.Count)
        {
            CurrentLoadedLevel = Levels[index].Instantiate<BaseLevel>();
            GameState.Instance.RegisterLevel(CurrentLoadedLevel);
            AddChild(CurrentLoadedLevel);
        }
    }
}
