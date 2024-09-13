using Godot;
using System;

public partial class Game : Node
{
	private GameScreen GameScreen { get; set; }
	private UI UI { get; set; }
	private Scores Scores { get; set; }


	public override void _Ready()
	{
		base._Ready();
		
		GameScreen = GetNode<GameScreen>("%GameScreen");
		UI = GetNode<UI>("%UI");

		//GameScreen.SetupScores(Scores);

		GameScreen.OnWinGame += WinGame;
		GameScreen.OnLoseGame += LoseGame;
		GameScreen.OnEndLevel += OpenEndLevelMenu;
		UI.OnStartPressed += StartGame;
		UI.OnNextLevelPressed += NextLevel;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		
		GameScreen.OnWinGame -= WinGame;
		GameScreen.OnLoseGame -= LoseGame;
		GameScreen.OnEndLevel -= OpenEndLevelMenu;
		UI.OnStartPressed -= StartGame;
		UI.OnNextLevelPressed -= NextLevel;
	}

/* 	public override void _Input(InputEvent @event)
	{
		base._Input(@event);

		if (@event.IsActionPressed("pause"))
		{
			Pause();
		}
	} */

	private void StartGame()
	{
		Scores = new Scores();
		GameScreen.StartGame(Scores);
	}

	private void OpenEndLevelMenu()
	{
		UI.OpenEndLevelMenu(Scores);
	}

	private void NextLevel()
	{
		GameScreen.EndLevel();
	}

	private void WinGame()
	{
		GD.Print("You Win!");
		// TODO: Win menu, add high score if you got one. 
		UI.OpenGameOverMenu(Scores);
	}
	
	private void LoseGame()
	{
		GD.Print("You Lose!");
		// TODO: Lose/Dead menu, high score if you got one.
		// Use same menu for win and lose? Just different message?
		// Could have Won bool in Scores to keep track. 
		UI.OpenGameOverMenu(Scores);
	}
	
	// TODO: Handle this separately for different menus?
/* 	private void Pause()
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
	} */
}
