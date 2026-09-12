using Godot;
using System;

public partial class Enemy : CharacterBody3D
{

    [Signal] public delegate void ScreamedEventHandler(Player player);

    public override void _Ready()
    {
        base._Ready();
        GameState.Instance.RegisterEnemy(this);
    }

    public virtual void TryReceiveHit(Player player, HurtData hurtData)
    {

    }

    public virtual void OnScreamHeard(Player player)
    {

    }
}

public partial class HurtData
{
    public int Damage { get; set; } = 0;
    public float PushbackIntensity { get; set; } = 0;
    public bool Stun { get; set; } = false;
}
