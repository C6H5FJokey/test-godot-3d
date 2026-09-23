using Godot;
using System;

public partial class GameEvents : Node
{
    public static GameEvents Instance { get; private set; } = null;

    public override void _Ready()
    {
        Instance = this;
    }

    public enum ImpactIntensity { LOW, MEDIUM, HIGH };

    [Signal] public delegate void ImpactFeltEventHandler(ImpactIntensity intensity);

    [Signal] public delegate void PlayerHurtEventHandler(Player player, Node3D source);

    [Signal] public delegate void PlayerDiedEventHandler(Player player);

    public void TriggerImpactFelt(ImpactIntensity intensity)
    {
        EmitSignalImpactFelt(intensity);
    }

    public void TriggerPlayerHurtEvent(Player player, Node3D source)
    {
        EmitSignalPlayerHurt(player, source);
    }

    public void TriggerPlayerDiedEvent(Player player)
    {
        EmitSignalPlayerDied(player);
    }
}
