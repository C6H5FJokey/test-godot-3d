using Godot;
using System;

[GlobalClass]
public partial class EquipmentData : Resource
{
    [Export] public String DisplayName {get; set;}
    [Export] public int Duration {get; set;} = -1;
    [Export] public int MaxDuration {get; set;}
    [Export] public PackedScene GlbMesh {get;set;}

    public void DecreaseDuration(int amount)
    {
        Duration = Mathf.Max(Duration - amount, 0);
    }
}
