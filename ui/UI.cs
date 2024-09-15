using Godot;
using System;
using System.Collections.Generic;

public partial class UI : CanvasLayer
{
	public event Action OnStartPressed;
	public event Action OnNextLevelPressed;
	
	private MainMenu MainMenu { get; set; }
	private OptionsMenu OptionsMenu { get; set; }
	private CreditsMenu CreditsMenu { get; set; }
	private EndLevelMenu EndLevelMenu { get; set; }
	private GameOverMenu GameOverMenu { get; set; }


	public override void _Ready()
	{
		base._Ready();
		
		GetMenuReferences();
		SubscribeToEvents();
		SetupHighScores();
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		
		UnsubscribeFromEvents();
	}

	public void OpenEndLevelMenu(Score scores)
	{
		CloseAllMenus();
		EndLevelMenu.Show();
		EndLevelMenu.SetupMenu(scores);
	}
	
	public void OpenGameOverMenu(Score score)
	{
		CloseAllMenus();
		GameOverMenu.Show();
		GameOverMenu.SetupMenu(score);
	}
	
	public void CloseAllMenus()
	{
		MainMenu.Hide();
		OptionsMenu.Hide();
		CreditsMenu.Hide();
		EndLevelMenu.Hide();
	}
	
	// TODO: Send LeaderboardHandler instead
	private void SetupHighScores()
	{
		List<HighScore> highScores = new();
		MainMenu.Scoreboard.InitializeHighScores(highScores);
		GameOverMenu.HighScores = highScores;
	}

	private void StartGame()
	{
		OnStartPressed?.Invoke();
		CloseAllMenus();
	}

	private void OpenMainMenu()
	{
		CloseAllMenus();
		MainMenu.Scoreboard.SetupScores();
		MainMenu.Show();
	}

	private void OpenOptions()
	{
		CloseAllMenus();
		OptionsMenu.Show();
	}

	private void OpenCredits()
	{
		CloseAllMenus();
		CreditsMenu.Show();
	}
	
	private void NextLevel()
	{
		CloseAllMenus();
		OnNextLevelPressed?.Invoke();
	}

	private void GetMenuReferences()
	{
		MainMenu = GetNode<MainMenu>("%MainMenu");
		OptionsMenu = GetNode<OptionsMenu>("%OptionsMenu");
		CreditsMenu = GetNode<CreditsMenu>("%CreditsMenu");
		EndLevelMenu = GetNode<EndLevelMenu>("%EndLevelMenu");
		GameOverMenu = GetNode<GameOverMenu>("%GameOverMenu");
	}

	private void SubscribeToEvents()
	{
		MainMenu.OnStartPressed += StartGame;
		MainMenu.OnOptionsPressed += OpenOptions;
		MainMenu.OnCreditsPressed += OpenCredits;
		OptionsMenu.OnBackPressed += OpenMainMenu;
		CreditsMenu.OnBackPressed += OpenMainMenu;
		EndLevelMenu.OnNextLevelPressed += NextLevel;
		GameOverMenu.OnDonePressed += OpenMainMenu;
	}

	private void UnsubscribeFromEvents()
	{
		MainMenu.OnStartPressed -= StartGame;
		MainMenu.OnOptionsPressed -= OpenOptions;
		MainMenu.OnCreditsPressed -= OpenCredits;
		OptionsMenu.OnBackPressed -= OpenMainMenu;
		CreditsMenu.OnBackPressed -= OpenMainMenu;
		EndLevelMenu.OnNextLevelPressed -= NextLevel;
		GameOverMenu.OnDonePressed -= OpenMainMenu;
	}
}
