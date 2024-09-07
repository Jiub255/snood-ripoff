using Godot;
using System;

public partial class UI : CanvasLayer
{
	public event Action OnStartPressed;
	
	private MainMenu MainMenu { get; set; }
	private OptionsMenu OptionsMenu { get; set; }
	private CreditsMenu CreditsMenu { get; set; }


	public override void _Ready()
	{
		base._Ready();
		
		MainMenu = GetNode<MainMenu>("%MainMenu");
		OptionsMenu = GetNode<OptionsMenu>("%OptionsMenu");
		CreditsMenu = GetNode<CreditsMenu>("%CreditsMenu");

		MainMenu.OnStartPressed += StartGame;
		MainMenu.OnOptionsPressed += OpenOptions;
		MainMenu.OnCreditsPressed += OpenCredits;
		OptionsMenu.OnBackPressed += OpenMainMenu;
		CreditsMenu.OnBackPressed += OpenMainMenu;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		
		MainMenu.OnStartPressed -= StartGame;
		MainMenu.OnOptionsPressed -= OpenOptions;
		MainMenu.OnCreditsPressed -= OpenCredits;
		OptionsMenu.OnBackPressed -= OpenMainMenu;
		CreditsMenu.OnBackPressed -= OpenMainMenu;
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
	
	public void CloseAllMenus()
	{
		MainMenu.Hide();
		OptionsMenu.Hide();
		CreditsMenu.Hide();
	}
}
