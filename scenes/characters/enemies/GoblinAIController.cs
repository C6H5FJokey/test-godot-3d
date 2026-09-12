using Godot;

public partial class GoblinAIController : ControllerComponent
{
    private Goblin _goblin;

    public Player Target { get; private set; }

    public override void _Ready()
    {
        base._Ready();
        _goblin = GetParent<Goblin>();
        GetNode<Area3D>("%PlayerDetectArea").BodyEntered += OnBodyEntered;
    }

    public override void UpdateIntent(double delta)
    {
        ResetIntent();

        if (Target == null || !IsInstanceValid(Target))
        {
            Target = null;
            return;
        }

        if (CanAttack())
        {
            Intent.Attack = true;
            return;
        }

        Intent.FacingDirection = GetChaseDirection();
        Intent.Speed = _goblin.Speed;
    }

    public void SetTargetIfEmpty(Player player)
    {
        if (Target == null)
        {
            Target = player;
        }
    }

    private void OnBodyEntered(Node3D body)
    {
        if (body is Player player)
        {
            Target = player;
        }
    }

    private bool CanAttack()
    {
        var isCooldownOver = Time.GetTicksMsec() - _goblin.TimeSinceLastAttack > _goblin.DurationBetweenAttacks;
        return isCooldownOver && _goblin.IsPlayerWithinReach(Target);
    }

    private Vector3 GetChaseDirection()
    {
        _goblin.NavigationAgent3D.TargetPosition = Target.GlobalPosition;

        var nextPosition = _goblin.NavigationAgent3D.GetNextPathPosition();
        var direction = nextPosition - _goblin.GlobalPosition;
        direction.Y = 0f;

        if (direction.LengthSquared() < 0.0001f)
        {
            return Vector3.Zero;
        }

        return direction.Normalized();
    }
}
