using Godot;

public abstract partial class ControllerComponent : Node
{
    public CharacterIntent Intent { get; } = new();

    public Node Actor { get; private set; }

    public override void _Ready()
    {
        base._Ready();
        Actor = GetParent();
    }

    public abstract void UpdateIntent(double delta);

    protected void ResetIntent()
    {
        Intent.Reset();
    }
}
