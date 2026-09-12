using Godot;

public partial class PlayerController : ControllerComponent
{
    private static readonly float MAX_ANGLE_LOOK_UP = Mathf.DegToRad(70f);
    private static readonly float MAX_ANGLE_LOOK_DOWN = Mathf.DegToRad(-70f);

    [Export] public float MouseSensitivity = 0.002f;

    private Player _player;
    private Camera3D _camera;

    public override void _Ready()
    {
        base._Ready();
        _player = GetParent<Player>();
        _camera = GetNode<Camera3D>("%MainCamera");
    }

    public override void _Input(InputEvent @event)
    {
        if (Input.MouseMode != Input.MouseModeEnum.Captured || @event is not InputEventMouseMotion mouseMotion)
        {
            return;
        }

        _player.RotateY(-mouseMotion.Relative.X * MouseSensitivity);
        _camera.RotateX(-mouseMotion.Relative.Y * MouseSensitivity);
        _camera.Rotation = _camera.Rotation with { X = Mathf.Clamp(_camera.Rotation.X, MAX_ANGLE_LOOK_DOWN, MAX_ANGLE_LOOK_UP) };
    }

    public override void UpdateIntent(double delta)
    {
        ResetIntent();

        var inputDir = Input.GetVector("strafe_left", "strafe_right", "backward", "forward");
        var input3DSpace = new Vector3(inputDir.X, 0f, -inputDir.Y);

        Intent.MoveDirection = _player.GlobalBasis * input3DSpace;
        Intent.Speed = Input.IsActionPressed("run") ? _player.RunSpeed : _player.WalkSpeed;
        Intent.Jump = Input.IsActionJustPressed("jump");
        Intent.Attack = Input.IsActionJustPressed("action");
        Intent.Kick = Input.IsActionJustPressed("kick");
        Intent.Throw = Input.IsActionJustPressed("throw");
        Intent.Defend = Input.IsActionPressed("offhand_action");
        Intent.Interact = Input.IsActionJustPressed("use");
    }
}
