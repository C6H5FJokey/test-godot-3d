using Godot;
using System;

public partial class DoorBody : StaticBody3D
{
    [Signal] public delegate void OpenRequestedEventHandler(Vector3 impulse);

    public void TriggerOpenRequested(Vector3 impulse)
    {
        EmitSignalOpenRequested(impulse);
    }
}
