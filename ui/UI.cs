using Godot;
using System;

public partial class UI : CanvasLayer
{
	public event Action OnStartPressed;
	public event Action OnNextLevelPressed;
	
	//public Scores Scores { get; set; }
	
	private MainMenu MainMenu { get; set; }
	private OptionsMenu OptionsMenu { get; set; }
	private CreditsMenu CreditsMenu { get; set; }
	private EndLevelMenu EndLevelMenu { get; set; }


	public override void _Ready()
	{
		base._Ready();
		
		MainMenu = GetNode<MainMenu>("%MainMenu");
		OptionsMenu = GetNode<OptionsMenu>("%OptionsMenu");
		CreditsMenu = GetNode<CreditsMenu>("%CreditsMenu");
		EndLevelMenu = GetNode<EndLevelMenu>("%EndLevelMenu");
		
		//EndLevelMenu.Scores = Scores;

		MainMenu.OnStartPressed += StartGame;
		MainMenu.OnOptionsPressed += OpenOptions;
		MainMenu.OnCreditsPressed += OpenCredits;
		OptionsMenu.OnBackPressed += OpenMainMenu;
		CreditsMenu.OnBackPressed += OpenMainMenu;
		EndLevelMenu.OnNextLevelPressed += NextLevel;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		
		MainMenu.OnStartPressed -= StartGame;
		MainMenu.OnOptionsPressed -= OpenOptions;
		MainMenu.OnCreditsPressed -= OpenCredits;
		OptionsMenu.OnBackPressed -= OpenMainMenu;
		CreditsMenu.OnBackPressed -= OpenMainMenu;
		EndLevelMenu.OnNextLevelPressed += NextLevel;
	}
	
	public void OpenEndLevelMenu()
	{
		CloseAllMenus();
		EndLevelMenu.Show();
		EndLevelMenu.SetupMenu();
	}
	
	public void CloseAllMenus()
	{
		MainMenu.Hide();
		OptionsMenu.Hide();
		CreditsMenu.Hide();
		EndLevelMenu.Hide();
	}
	
	public void SetupScores(Scores scores)
	{
		//Scores = scores;
		EndLevelMenu.Scores = scores;
	}

	private void StartGame()
	{
		OnStartPressed?.Invoke();
		CloseAllMenus();
	}

	private void OpenMainMenu()
	{
		CloseAllMenus();
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
}
