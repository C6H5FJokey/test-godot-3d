using Godot;
using System;

// 伤害来源指示器：屏幕中心外侧的一段圆弧 + 一个箭头，指出伤害大致来自哪个方位。
// 生成后 Lifetime 秒内持续指向来源对象；来源消失时保留最后一次已知方位，按原时长淡出。
public partial class DamageIndicator : Control
{
    [Export] public float Lifetime = 1f;
    [Export] public float FadeInDuration = 0.1f;
    [Export] public float FadeOutDuration = 0.3f;
    [Export] public float Radius = 110f;
    [Export] public float ArcHalfAngleDegrees = 40f;
    [Export] public float Thickness = 6f;
    [Export] public float ArrowSize = 14f;
    [Export] public Color IndicatorColor = new(1f, 0.72f, 0.55f, 0.85f);

    private Player _player;
    private Node3D _source;
    private float _elapsed;
    private float _bearing;

    public void Setup(Player player, Node3D source)
    {
        _player = player;
        _source = source;
        _elapsed = 0f;
    }

    public bool IsTracking(Node3D source)
    {
        return _source == source && IsInstanceValid(source);
    }

    public override void _Process(double delta)
    {
        _elapsed += (float)delta;

        if (_elapsed >= Lifetime || !IsInstanceValid(_player))
        {
            QueueFree();
            return;
        }

        UpdateBearing();
        Rotation = _bearing;
        SelfModulate = new Color(1f, 1f, 1f, ComputeAlpha());
    }

    public override void _Draw()
    {
        var halfAngle = Mathf.DegToRad(ArcHalfAngleDegrees);
        var up = -Mathf.Pi / 2f;
        const int pointCount = 24;

        // 外圈柔光，保证在亮背景上也能看清
        DrawArc(Vector2.Zero, Radius, up - halfAngle, up + halfAngle, pointCount, IndicatorColor with { A = IndicatorColor.A * 0.25f }, Thickness * 2.5f, true);
        DrawArc(Vector2.Zero, Radius, up - halfAngle, up + halfAngle, pointCount, IndicatorColor, Thickness, true);

        // 圆弧中间的箭头，指向来源所在方位
        var arrowHalfWidth = ArrowSize * 0.7f;
        DrawColoredPolygon(
        [
            new Vector2(0f, -(Radius + ArrowSize)),
            new Vector2(-arrowHalfWidth, -(Radius - Thickness)),
            new Vector2(arrowHalfWidth, -(Radius - Thickness)),
        ], IndicatorColor);
    }

    private void UpdateBearing()
    {
        if (!IsInstanceValid(_source))
        {
            return; // 来源已经没了：保留最后一次已知方位，继续按原时长淡出
        }

        var offset = _source.GlobalPosition - _player.GlobalPosition;
        offset.Y = 0f;
        if (offset.IsZeroApprox())
        {
            return;
        }

        // 玩家身体只绕 Y 轴旋转，所以用身体基把来源方向换算成"相对正前方"的水平夹角
        var localOffset = _player.GlobalBasis.Inverse() * offset.Normalized();
        _bearing = Mathf.Atan2(localOffset.X, -localOffset.Z);
    }

    private float ComputeAlpha()
    {
        if (_elapsed < FadeInDuration)
        {
            return FadeInDuration <= 0f ? 1f : _elapsed / FadeInDuration;
        }

        var fadeOutStart = Lifetime - FadeOutDuration;
        if (_elapsed <= fadeOutStart)
        {
            return 1f;
        }

        return FadeOutDuration <= 0f ? 0f : Mathf.Clamp((Lifetime - _elapsed) / FadeOutDuration, 0f, 1f);
    }
}
