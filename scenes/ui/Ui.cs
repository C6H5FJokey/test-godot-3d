using Godot;
using System;

public partial class Ui : CanvasLayer
{
    private static readonly PackedScene _damageIndicatorScene = GD.Load<PackedScene>("res://scenes/ui/damage_indicator.tscn");

    public Panel HurtVignette;
    public Panel DeathScreen;
    public Control DamageIndicators;

    public override void _Ready()
    {
        // 顿帧（StopServer）会暂停整棵树，UI 必须继续跑，不然受击反馈和死亡界面会卡住
        ProcessMode = ProcessModeEnum.Always;
        HurtVignette = GetNode<Panel>("%HurtVignette");
        DeathScreen = GetNode<Panel>("%DeathScreen");
        DamageIndicators = GetNode<Control>("%DamageIndicators");
        DeathScreen.Visible = false;
        GetNode<Button>("%Restart").Pressed += OnRestartPressed;
        GetNode<Button>("%ExitGame").Pressed += OnExitGamePressed;
        GameEvents.Instance.PlayerHurt += OnPlayerHurt;
        GameEvents.Instance.PlayerDied += OnPlayerDied;
    }

    public void OnPlayerHurt(Player player, Node3D source)
    {
        var tween = CreateTween();
        // tween.TweenProperty(HurtVignette, CanvasItem.PropertyName.SelfModulate + ":a", 0.5, 0.1).From(0.0);
        HurtVignette.SelfModulate = HurtVignette.SelfModulate with {A=0.5f};
        tween.TweenProperty(HurtVignette, CanvasItem.PropertyName.SelfModulate + ":a", 0.0, 0.2);

        ShowDamageIndicator(player, source);
    }

    public void OnPlayerDied(Player player)
    {
        DeathScreen.Visible = true;
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    // 同一个来源连续命中时只刷新已有的指示器，避免同一方位叠出好几段圆弧
    private void ShowDamageIndicator(Player player, Node3D source)
    {
        if (!IsInstanceValid(source))
        {
            return;
        }

        foreach (var child in DamageIndicators.GetChildren())
        {
            if (child is DamageIndicator indicator && indicator.IsTracking(source))
            {
                indicator.Setup(player, source);
                return;
            }
        }

        var newIndicator = _damageIndicatorScene.Instantiate<DamageIndicator>();
        DamageIndicators.AddChild(newIndicator);
        newIndicator.Setup(player, source);
    }

    private void OnRestartPressed()
    {
        GetTree().ReloadCurrentScene();
    }

    private void OnExitGamePressed()
    {
        GetTree().Quit();
    }
}
