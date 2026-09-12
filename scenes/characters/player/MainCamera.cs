using Godot;
using System.Collections.Generic;

public partial class MainCamera : Camera3D
{
    private readonly Dictionary<GameEvents.ImpactIntensity, ulong> _intensityDuration = new()
    {
        [GameEvents.ImpactIntensity.LOW] = 80,
        [GameEvents.ImpactIntensity.MEDIUM] = 140,
        [GameEvents.ImpactIntensity.HIGH] = 200,
    };

    private readonly Dictionary<GameEvents.ImpactIntensity, float> _intensityShake = new()
    {
        [GameEvents.ImpactIntensity.LOW] = 0.1f,
        [GameEvents.ImpactIntensity.MEDIUM] = 0.15f,
        [GameEvents.ImpactIntensity.HIGH] = 0.2f,
    };

    private readonly Dictionary<GameEvents.ImpactIntensity, ulong> _intensityInterval = new()
    {
        [GameEvents.ImpactIntensity.LOW] = 30,
        [GameEvents.ImpactIntensity.MEDIUM] = 40,
        [GameEvents.ImpactIntensity.HIGH] = 50,
    };

    private ulong _shakeStartTime;
    private ulong _shakeEndTime;
    private ulong _lastShakeTime;

    private Vector3 _initialPosition;
    private GameEvents.ImpactIntensity _currentIntensity;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;

        GameEvents.Instance.ImpactFelt += OnImpactFelt;

        _initialPosition = Position;
    }

    public override void _Process(double delta)
    {
        var currentTime = Time.GetTicksMsec();

        if (currentTime >= _shakeEndTime)
        {
            Position = _initialPosition;
            return;
        }

        var interval = _intensityInterval[_currentIntensity];
        if (currentTime - _lastShakeTime < interval)
        {
            return;
        }

        _lastShakeTime = currentTime;

        var duration = _shakeEndTime - _shakeStartTime;
        var elapsed = currentTime - _shakeStartTime;

        float progress = duration == 0
            ? 1.0f
            : (float)elapsed / duration;

        // 从 1 平滑衰减到 0
        float decay = 1.0f - progress;

        var strength =
            _intensityShake[_currentIntensity] * decay;

        Position = _initialPosition;

        var offset = new Vector3(
            (float)GD.RandRange(-strength, strength),
            (float)GD.RandRange(-strength, strength),
            (float)GD.RandRange(-strength, strength)
        );

        Position += offset;
    }

    public void OnImpactFelt(GameEvents.ImpactIntensity intensity)
    {
        var currentTime = Time.GetTicksMsec();
        var duration = _intensityDuration[intensity];

        _currentIntensity = intensity;
        _shakeStartTime = currentTime;
        _shakeEndTime = currentTime + duration;

        // 立刻产生一次 Shake
        _lastShakeTime = 0;
    }
}