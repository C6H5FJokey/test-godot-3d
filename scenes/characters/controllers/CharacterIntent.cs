using Godot;

public partial class CharacterIntent
{
    public Vector3 MoveDirection;
    public Vector3 FacingDirection;
    public float Speed;
    public bool Jump;
    public bool Attack;
    public bool Kick;
    public bool Throw;
    public bool Defend;
    public bool Interact;

    public void Reset()
    {
        MoveDirection = Vector3.Zero;
        FacingDirection = Vector3.Zero;
        Speed = 0f;
        Jump = false;
        Attack = false;
        Kick = false;
        Throw = false;
        Defend = false;
        Interact = false;
    }
}
