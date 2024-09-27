using Godot;
using System;

public partial class MainMenu : Control
{
	public event Action OnStartPressed;
	public event Action OnOptionsPressed;
	public event Action OnCreditsPressed;

	public Scoreboard Scoreboard { get; private set; }
	private Button StartButton { get; set; }
	private Button OptionsButton { get; set; }
	private Button CreditsButton { get; set; }
	private Button QuitButton { get; set; }


	public override void _Ready()
	{
		base._Ready();

		StartButton = GetNode<Button>("%StartButton");
		OptionsButton = GetNode<Button>("%OptionsButton");
		CreditsButton = GetNode<Button>("%CreditsButton");
		QuitButton = GetNode<Button>("%QuitButton");
		Scoreboard = GetNode<Scoreboard>("%Scoreboard");
		
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
		GD.Print("Quit button pressed");
		if (OS.HasFeature("HTML5")) // Ensure it's running on WebGL/HTML5
   		{
        	JavaScriptBridge.Eval("location.reload();"); // Reloads the page
    	}
	}
}
