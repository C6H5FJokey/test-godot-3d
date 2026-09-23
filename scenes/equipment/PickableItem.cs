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
    private bool _isFocused;

    public bool IsFocused => _isFocused;

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

    public void SetFocused(bool focused)
    {
        if (_isFocused == focused)
        {
            return;
        }
        _isFocused = focused;

        if (_equipedObject == null || !IsInstanceValid(_equipedObject))
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
                var overrideMaterial = focused ? GetHighlightMaterial(meshNode.GetActiveMaterial(i)) : null;
                meshNode.SetSurfaceOverrideMaterial(i, overrideMaterial);
            }
        }
    }

    private Material GetHighlightMaterial(Material originalMaterial)
    {
        if (originalMaterial == null)
        {
            return null;
        }

        if (_materialCache.TryGetValue(originalMaterial, out var highlightMaterial))
        {
            return highlightMaterial;
        }

        highlightMaterial = originalMaterial.Duplicate() as Material;
        if (highlightMaterial is BaseMaterial3D baseMaterial)
        {
            baseMaterial.AlbedoColor = HightlightColor;
        }
        _materialCache[originalMaterial] = highlightMaterial;
        return highlightMaterial;
    }

}
