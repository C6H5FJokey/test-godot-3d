using Godot;
using System;

public partial class PlayerState : State
{
	protected Player Player => (Player)Actor;
}
