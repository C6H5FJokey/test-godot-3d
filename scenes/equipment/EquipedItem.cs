using Godot;
using System;
using System.Collections.Generic;

public partial class EquipedItem : Node3D
{
    private static Dictionary<Material, Material> _materialCache = [];
    [Export] public EquipmentData EquipmentData {get; set;}
    [Export] public bool IsAlwaysInFront {get; set;}

    public override void _Ready()
    {
        base._Ready();
        Node3D equipedObject = null;
        if (EquipmentData != null)
        {
            equipedObject = EquipmentData.GlbMesh.Instantiate<Node3D>();
        }
        if (equipedObject != null)
        {
            AddChild(equipedObject);
            if (IsAlwaysInFront)
            {
                foreach (var meshNodeItem in equipedObject.GetChildren())
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
                                baseMaterial.UseZClipScale = true;
                                baseMaterial.ZClipScale = 0.8f;
                            }
                            _materialCache[originalMaterial] = newMaterial;
                        }
                        meshNode.SetSurfaceOverrideMaterial(i, newMaterial);
                    }
                }
            }
        }
    }

}
