using Godot;
using System;

public partial class HealthComponent : Node
{
    [Signal] public delegate void HealthChangedEventHandler(int hpValue, int maxHPValue, int delta);

    [Signal] public delegate void DiedEventHandler();

    [Export] public int HPValue = -1;
    [Export] public int MaxHPValue;

    public override void _Ready()
    {
        base._Ready();
        if (HPValue == -1)
        {
            HPValue = MaxHPValue;
        }
    }

    public void TakeDamage(int damage)
    {
        var oldHPvalue = HPValue;
        HPValue = Mathf.Clamp(HPValue - damage, 0, MaxHPValue);
        EmitSignalHealthChanged(HPValue, MaxHPValue, HPValue - oldHPvalue);
        if (HPValue == 0 && oldHPvalue > 0)
        {
            EmitSignalDied();
        }
    }

    public bool IsDead()
    {
        return HPValue == 0;
    }
}
