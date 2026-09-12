using Godot;
using System;
using System.Collections.Generic;

public partial class GameState : Node
{
	public static GameState Instance { get; private set; } = null!;

	public BaseLevel CurrentLevel;
	public HashSet<Player> Players = [];
	public HashSet<Enemy> Enemies = [];
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Instance = this;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void RegisterLevel(BaseLevel level)
	{
		CurrentLevel = level;
	}

	public void RegisterPlayer(Player player)
	{
		Players.Add(player);
		player.TreeExiting += () => Players.Remove(player);
	}

	public void RegisterEnemy(Enemy enemy)
	{
		Enemies.Add(enemy);
		enemy.TreeExiting += () => Enemies.Remove(enemy);
	}
}
