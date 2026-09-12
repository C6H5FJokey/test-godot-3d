using Godot;
using System;

public partial class EquipmentComponent : Node
{
    private static readonly PackedScene _throwableItemScene = GD.Load<PackedScene>("res://scenes/equipment/throwable_item.tscn");
    private static readonly PackedScene _equipedItemScene = GD.Load<PackedScene>("res://scenes/equipment/equiped_item.tscn");

    [Export] public WeaponData WeaponData;
    [Export] public OffhandData OffhandData;
    [Export] public bool IsAlwaysInFront;
    [Export] public Node3D WeaponPlaceHolder;
    [Export] public Node3D WeaponSpawner;
    [Export] public Node3D ExtraPitch; // 用于放玩家相机来结算俯仰角
    [Export] public RayCast3D WeaponReachRaycast;
    [Export] public Node3D OffhandPlaceHolder;

    private Node3D _host;

    public override void _Ready()
    {
        base._Ready();
        if (WeaponData != null)
        {
            EquipWeapon(WeaponData);
        }
        _host = GetParent<Node3D>();
    }

    public void Equip(EquipmentData equipmentData, Transform3D pickupTransform = default)
    {
        switch (equipmentData)
        {
            case WeaponData weaponData:
                DropWeapon();
                EquipWeapon(weaponData, pickupTransform);
                break;
            case OffhandData offhandData:
                DropOffhand();
                EquipOffhand(offhandData, pickupTransform);
                break;
        }
    }
    public void EquipWeapon(WeaponData weaponData, Transform3D pickupTransform = default)
    {
        if (WeaponPlaceHolder == null)
        {
            return;
        }
        if (WeaponData != null)
        {
            foreach (var child in WeaponPlaceHolder.GetChildren())
            {
                child.QueueFree();
            }
            WeaponData = null;
        }
        if (weaponData == null) // 有武器时卸载装备空武器是卸载武器
        {
            return;
        }
        // 保证初始化时所有武器都会进行唯一化。
        WeaponData = weaponData.Duplicate() as WeaponData;
        var equipedItem = _equipedItemScene.Instantiate<EquipedItem>();
        equipedItem.EquipmentData = WeaponData;
        // equipedItem.IsAlwaysInFront = IsAlwaysInFront;
        WeaponPlaceHolder.AddChild(equipedItem);
        if (pickupTransform != default)
        {
            equipedItem.GlobalTransform = pickupTransform;
            var tween = equipedItem.CreateTween();
            tween.SetTrans(Tween.TransitionType.Quad);
            tween.SetEase(Tween.EaseType.Out);
            tween.Parallel().TweenProperty(equipedItem, "position", Vector3.Zero, 0.4);
            tween.Parallel().TweenProperty(equipedItem, "rotation", Vector3.Zero, 0.2);
        }
        WeaponReachRaycast.TargetPosition = Vector3.Forward * WeaponData.Reach;
    }

    public bool HasWeapon()
    {
        return WeaponData != null;
    }

    public void ThrowWeapon(int maskValue, Vector3 appliedImpulse = default)
    {
        if (!HasWeapon())
        {
            return;
        }
        // var moveDirection = Transform.Basis * Camera.Basis * Vector3.Forward;
        var throwableItem = _throwableItemScene.Instantiate<ThrowableItem>();
        throwableItem.EquipmentData = WeaponData;
        throwableItem.SetCollisionMaskValue(maskValue, true);
        var transform = WeaponSpawner.GlobalTransform;
        throwableItem.GlobalTransform = transform;
        GameState.Instance.CurrentLevel.AddChild(throwableItem);
        throwableItem.PrepareProjectile(true, appliedImpulse);
        EquipWeapon(null);
    }

    public void DropWeapon(Vector3 appliedImpulse = default)
    {
        if (!HasWeapon())
        {
            return;
        }
        // var moveDirection = Transform.Basis * Camera.Basis * Vector3.Forward;
        var throwableItem = _throwableItemScene.Instantiate<ThrowableItem>();
        throwableItem.EquipmentData = WeaponData;
        var transform = WeaponSpawner.GlobalTransform;
        throwableItem.GlobalTransform = transform;
        GameState.Instance.CurrentLevel.AddChild(throwableItem);
        throwableItem.PrepareProjectile(false, appliedImpulse);
        EquipWeapon(null);
    }

    public bool HasOffhandEquipment()
    {
        return OffhandData != null;
    }

    public void EquipOffhand(OffhandData offhandData, Transform3D pickupTransform = default)
    {
        if (OffhandPlaceHolder == null)
        {
            return;
        }
        if (offhandData == null && OffhandData != null) // 有武器时卸载装备空武器是卸载武器
        {
            OffhandPlaceHolder.GetChild<EquipedItem>(0).QueueFree();
            OffhandData = null;
            return;
        }
        // 保证初始化时所有武器都会进行唯一化。
        OffhandData = offhandData.Duplicate() as OffhandData;
        var equipedItem = _equipedItemScene.Instantiate<EquipedItem>();
        equipedItem.EquipmentData = OffhandData;
        // equipedItem.IsAlwaysInFront = IsAlwaysInFront;
        OffhandPlaceHolder.AddChild(equipedItem);
        if (pickupTransform != default)
        {
            equipedItem.GlobalTransform = pickupTransform;
            var tween = equipedItem.CreateTween();
            tween.SetTrans(Tween.TransitionType.Quad);
            tween.SetEase(Tween.EaseType.Out);
            tween.Parallel().TweenProperty(equipedItem, "position", Vector3.Zero, 0.4);
            tween.Parallel().TweenProperty(equipedItem, "rotation", Vector3.Zero, 0.2);
        }
    }

    public void DropOffhand(Vector3 appliedImpulse = default)
    {
        if (!HasOffhandEquipment())
        {
            return;
        }
        // var moveDirection = Transform.Basis * Camera.Basis * Vector3.Forward;
        var throwableItem = _throwableItemScene.Instantiate<ThrowableItem>();
        throwableItem.EquipmentData = OffhandData;
        var transform = WeaponSpawner.GlobalTransform;
        throwableItem.GlobalTransform = transform;
        GameState.Instance.CurrentLevel.AddChild(throwableItem);
        throwableItem.PrepareProjectile(false, appliedImpulse);
        EquipOffhand(null);
    }
}
