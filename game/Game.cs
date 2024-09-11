using Godot;
using System;

public partial class Game : Node
{
	private GameScreen GameScreen { get; set; }
	private UI UI { get; set; }
	private Scores Scores { get; set; } = new();


	public override void _Ready()
	{
		base._Ready();
		
		GameScreen = GetNode<GameScreen>("%GameScreen");
		UI = GetNode<UI>("%UI");

		GameScreen.SetupScores(Scores);
		UI.SetupScores(Scores);

		GameScreen.OnWinGame += WinGame;
		GameScreen.OnEndLevelMenu += OpenEndLevelMenu;
		UI.OnStartPressed += StartGame;
		UI.OnNextLevelPressed += NextLevel;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		
		GameScreen.OnWinGame -= WinGame;
		GameScreen.OnEndLevelMenu -= OpenEndLevelMenu;
		UI.OnStartPressed -= StartGame;
		UI.OnNextLevelPressed -= NextLevel;
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

	private void OpenEndLevelMenu()
	{
		UI.OpenEndLevelMenu();
	}

	private void NextLevel()
	{
		GameScreen.EndLevel();
	}

	private void WinGame()
	{
		GD.Print("You Win!");
		// TODO: Win menu, add high score if you got one. 
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
}
