using Godot;
using System;

public partial class Ui : CanvasLayer
{
    public Panel HurtVignette;

    public override void _Ready()
    {
        HurtVignette = GetNode<Panel>("%HurtVignette");
        GameEvents.Instance.PlayerHurt += OnPlayerHurt;
    }

    public void OnPlayerHurt(Player player)
    {
        var tween = CreateTween();
        // tween.TweenProperty(HurtVignette, CanvasItem.PropertyName.SelfModulate + ":a", 0.5, 0.1).From(0.0);
        HurtVignette.SelfModulate = HurtVignette.SelfModulate with {A=0.5f};
        tween.TweenProperty(HurtVignette, CanvasItem.PropertyName.SelfModulate + ":a", 0.0, 0.2);
    }

}
