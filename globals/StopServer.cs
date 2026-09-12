using Godot;
using System;
using System.Collections.Generic;

public partial class StopServer : Node
{
    public StopServer Instance { get; private set; } = null;

    public bool IsPaused = false;

    public ulong TimeShouldStopPause = 0;

    private static readonly Dictionary<GameEvents.ImpactIntensity, ulong> _intensityTick = new()
    {
        [GameEvents.ImpactIntensity.LOW] = 40,
        [GameEvents.ImpactIntensity.MEDIUM] = 100,
        [GameEvents.ImpactIntensity.HIGH] = 160,
    };

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        GameEvents.Instance.ImpactFelt += OnImpactFelt;
    }

    public override void _Process(double delta)
    {
        if (Time.GetTicksMsec() > TimeShouldStopPause)
        {
            GetTree().Paused = false;
            IsPaused = false;
        }
    }


    public void OnImpactFelt(GameEvents.ImpactIntensity intensity)
    {
        GetTree().Paused = true;
        IsPaused = true;
        if (Time.GetTicksMsec() + _intensityTick[intensity] > TimeShouldStopPause)
        {
            TimeShouldStopPause = Time.GetTicksMsec() + _intensityTick[intensity];
        }
    }


}
