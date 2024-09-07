using Godot;
using System;

public partial class Game : Node
{
	private GameScreen GameScreen { get; set; }
	private UI UI { get; set; }


	public override void _Ready()
	{
		base._Ready();
		
		GameScreen = GetNode<GameScreen>("%GameScreen");
		UI = GetNode<UI>("%UI");

		GameScreen.OnWinGame += WinGame;
		UI.OnStartPressed += StartGame;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		
		GameScreen.OnWinGame -= WinGame;
		UI.OnStartPressed -= StartGame;
	}

	public override void _Input(InputEvent @event)
	{
		base._Input(@event);

		if (@event.IsActionPressed("pause"))
		{
			Pause();
		}
	}

	private void StartGame()
	{
		GameScreen.StartGame();
	}
	
	// TODO: Handle this separately for different menus?
	private void Pause()
	{
		SceneTree tree = GetTree();
		if (tree.Paused)
		{
			tree.Paused = false;
			UI.CloseAllMenus();
		}
		else
		{
			tree.Paused = true;
			// What to do here?
		}
	}

	private void WinGame()
	{
		GD.Print("You Win!");
	}
}
