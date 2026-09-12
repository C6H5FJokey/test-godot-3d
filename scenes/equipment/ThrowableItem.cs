using Godot;
using System;
using System.Reflection;

public partial class ThrowableItem : RigidBody3D
{
    private static readonly PackedScene _pickableItemScene = GD.Load<PackedScene>("res://scenes/equipment/pickable_item.tscn");
    [Export] public EquipmentData EquipmentData {get; set;}

    private Node3D _equipedObject;
    private bool _isConnectedSleepingStateChanged = false;

    public Basis OriginalBasis;
    public override void _Ready()
    {
        base._Ready();
        if (EquipmentData == null)
        {
            QueueFree();
            return;
        }
        _equipedObject = EquipmentData?.GlbMesh?.Instantiate<Node3D>();
        if (_equipedObject == null)
        {
            QueueFree();
            return;
        }
        AddChild(_equipedObject);
        var meshNode = _equipedObject.GetChild<MeshInstance3D>(0);
        var collisionShape = GetNode<CollisionShape3D>("%CollisionShape3D");
        collisionShape.Shape = meshNode.Mesh.CreateConvexShape();
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node node)
    {
        if (!_isConnectedSleepingStateChanged)
        {
            SleepingStateChanged += ConvertToPickup;
            _isConnectedSleepingStateChanged = true;
        }
        if (node is IImpaleable impaleable && EquipmentData is WeaponData weaponData && impaleable.CanImpale(weaponData))
        {
            impaleable.Impale(this);
            QueueFree();
        }
    }

    public void PrepareProjectile(bool isThrown, Vector3 appliedImpulse = default)
    {
        OriginalBasis = GlobalBasis;
        if (isThrown && EquipmentData is WeaponData weaponData)
        {
            LinearVelocity = -GlobalBasis.Z * weaponData.ThrowMovementSpeed;
            AngularVelocity = -GlobalBasis.Y * weaponData.ThrowRotationSpeed;
        }
        if (appliedImpulse != default)
        {
            LinearVelocity += appliedImpulse;
        }
    }

    public void ConvertToPickup()
    {
        var pickableItem = _pickableItemScene.Instantiate<PickableItem>();
        pickableItem.EquipmentData = EquipmentData;
        GameState.Instance.CurrentLevel.AddChild(pickableItem);
        pickableItem.GlobalTransform = GlobalTransform;
        QueueFree();
    }
}
