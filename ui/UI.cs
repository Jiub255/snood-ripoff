using Godot;
using System;

public partial class UI : CanvasLayer
{
	public event Action OnStartPressed;
	public event Action OnNextLevelPressed;
	
	private MainMenu MainMenu { get; set; }
	private OptionsMenu OptionsMenu { get; set; }
	private CreditsMenu CreditsMenu { get; set; }
	private EndLevelMenu EndLevelMenu { get; set; }
	private GameOverMenu GameOverMenu { get; set; }
	private AudioStreamPlayer ClickSFX { get; set; }
	private AudioStreamPlayer Music { get; set; }
	private AudioStream MenuSong { get; } = GD.Load<AudioStream>("res://assets/music/Menu-2024-04-24_02.ogg");
	private AudioStream LevelEndSong { get; } = GD.Load<AudioStream>("res://assets/music/Swing-2024-02-21_02.ogg");


	public override void _Ready()
	{
		base._Ready();
		
		GetReferences();
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
		Music.Stream = LevelEndSong;
		Music.Play();
	}
	
	public void OpenGameOverMenu(Score score)
	{
		CloseAllMenus();
		GameOverMenu.Show();
		GameOverMenu.SetupMenu(score);
		Music.Stop();
	}
	
	public void CloseAllMenus()
	{
		MainMenu.Hide();
		OptionsMenu.Hide();
		CreditsMenu.Hide();
		EndLevelMenu.Hide();
		ClickSFX.Play();
	}
	
	private void SetupHighScores()
	{
		LeaderboardHandler leaderboardHandler = new(this);
		MainMenu.Scoreboard.InitializeHighScores(leaderboardHandler);
		GameOverMenu.LeaderboardHandler = leaderboardHandler;
	}

	private void StartGame()
	{
		OnStartPressed?.Invoke();
		CloseAllMenus();
		Music.Stop();
	}

	private void OpenMainMenu()
	{
		CloseAllMenus();
		MainMenu.Show();
		if (Music.Stream != MenuSong)
		{
			Music.Stream = MenuSong;
		}
		StartMusic();
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
		Music.Stop();
	}
	
	private void StartMusic()
	{
		if (!Music.Playing)
		{
			Music.Play();
		}
	}

	private void GetReferences()
	{
		MainMenu = GetNode<MainMenu>("%MainMenu");
		OptionsMenu = GetNode<OptionsMenu>("%OptionsMenu");
		CreditsMenu = GetNode<CreditsMenu>("%CreditsMenu");
		EndLevelMenu = GetNode<EndLevelMenu>("%EndLevelMenu");
		GameOverMenu = GetNode<GameOverMenu>("%GameOverMenu");
		ClickSFX = GetNode<AudioStreamPlayer>("%SFX");
		Music = GetNode<AudioStreamPlayer>("%Music");
		
		Music.Play();
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
