using Godot;
using System;
using System.Collections.Generic;

public partial class PickableItem : Area3D
{
    private static Dictionary<Material, Material> _materialCache = [];
    [Export] public EquipmentData EquipmentData { get; set; }
    [Export] public Color HightlightColor { get; set; } = Colors.Yellow;
    [Export] public bool IsScenePlaced { get; set; }
    private Node3D _equipedObject;

    public override void _Ready()
    {
        base._Ready();
        if (IsScenePlaced)
        {
            EquipmentData = EquipmentData?.Duplicate() as EquipmentData;
        }
        _equipedObject = EquipmentData?.GlbMesh?.Instantiate<Node3D>();
        if (_equipedObject != null)
        {
            AddChild(_equipedObject);
            var meshNode = _equipedObject.GetChild<MeshInstance3D>(0);
            var collisionShape = GetNode<CollisionShape3D>("%CollisionShape3D");
            collisionShape.Shape = meshNode.Mesh.CreateConvexShape();
        }
    }

    public void Hightlight()
    {
        if (_equipedObject == null)
        {
            return;
        }
        foreach (var meshNodeItem in _equipedObject.GetChildren())
        {
            if (meshNodeItem is not MeshInstance3D meshNode)
            {
                continue;
            }
            var mesh = meshNode.Mesh;
            for (int i = 0; i < mesh.GetSurfaceCount(); i++)
            {
                var originalMaterial = meshNode.GetActiveMaterial(i);

                if (!_materialCache.TryGetValue(originalMaterial, out var newMaterial))
                {
                    newMaterial = originalMaterial.Duplicate() as Material;
                    if (newMaterial is BaseMaterial3D baseMaterial)
                    {
                        baseMaterial.AlbedoColor = HightlightColor;
                    }
                    _materialCache[originalMaterial] = newMaterial;
                }
                meshNode.SetSurfaceOverrideMaterial(i, newMaterial);
            }
        }
    }

    public void Unhightlight()
    {
        if (_equipedObject == null)
        {
            return;
        }
        foreach (var meshNodeItem in _equipedObject.GetChildren())
        {
            if (meshNodeItem is not MeshInstance3D meshNode)
            {
                continue;
            }
            var mesh = meshNode.Mesh;
            for (int i = 0; i < mesh.GetSurfaceCount(); i++)
            {
                meshNode.SetSurfaceOverrideMaterial(i, null);
            }
        }
    }

}
