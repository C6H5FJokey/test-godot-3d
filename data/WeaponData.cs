using Godot;
using System;

[GlobalClass]
public partial class WeaponData : EquipmentData
{
    [Export] public int DamageMin;
    [Export] public int DamageMax;
    [Export] public float Reach;
    [Export] public float SwingHalfAngle = 45f;
    [Export] public float ThrowRotationSpeed;
    [Export] public float ThrowMovementSpeed;
    [Export] public Vector3 ImpaleLocalTranslation;
    [Export] public float ImpaleLocalRotation;

    public int GetDamageDealt()
    {
        return GD.RandRange(DamageMin, DamageMax);
    }

}
