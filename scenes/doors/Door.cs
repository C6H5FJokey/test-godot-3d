using Godot;
using System;

public partial class Door : Node3D
{
    private MeshInstance3D _door;
    private DoorBody _doorBody;
    public bool IsOpened = false;

    public override void _Ready()
    {
        _door = GetNode<MeshInstance3D>("door/Door");
        _doorBody = GetNode<DoorBody>("door/Door/DoorBody");
        _doorBody.OpenRequested += OnOpenRequested;
    }

    public void OnOpenRequested(Vector3 impulse)
    {
        OpenDoor(impulse);
    }

    public void OpenDoor(Vector3 impulse)
    {
        if (IsOpened)
        {
            return;
        }
        var tween = CreateTween();
        float rotationDegrees;
        if (-GlobalBasis.Z.Dot(impulse) > 0)
        {
            rotationDegrees = -90;
        }
        else
        {
            rotationDegrees = 90;
        }
        tween.TweenProperty(_door, Node3D.PropertyName.RotationDegrees+":y", rotationDegrees, 0.2 / impulse.Length());
        IsOpened = true;
    }
}
