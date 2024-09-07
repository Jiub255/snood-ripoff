using Godot;
using System;

public partial class MainMenu : Control
{
	public event Action OnStartPressed;
	public event Action OnOptionsPressed;
	public event Action OnCreditsPressed;
	
	private Button StartButton { get; set; }
	private Button OptionsButton { get; set; }
	private Button CreditsButton { get; set; }
	private Button QuitButton { get; set; }
	private HighScores HighScores { get; set; }


	public override void _Ready()
	{
		base._Ready();
		
		StartButton = GetNode<Button>("%StartButton");
		OptionsButton = GetNode<Button>("%OptionsButton");
		CreditsButton = GetNode<Button>("%CreditsButton");
		QuitButton = GetNode<Button>("%QuitButton");
		HighScores = GetNode<HighScores>("%HighScores");
		
		StartButton.Pressed += StartGame;
		OptionsButton.Pressed += OpenOptions;
		CreditsButton.Pressed += OpenCredits;
		QuitButton.Pressed += QuitGame;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		
		StartButton.Pressed -= StartGame;
		OptionsButton.Pressed -= OpenOptions;
		CreditsButton.Pressed -= OpenCredits;
		QuitButton.Pressed -= QuitGame;
	}

	private void StartGame()
	{
		OnStartPressed?.Invoke();
	}

	private void OpenOptions()
	{
		OnOptionsPressed?.Invoke();
	}

	private void OpenCredits()
	{
		OnCreditsPressed?.Invoke();
	}

	private void QuitGame()
	{
		throw new NotImplementedException();
	}
}
