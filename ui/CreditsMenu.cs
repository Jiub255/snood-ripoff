using Godot;
using System;

public partial class CreditsMenu : Control
{
	public event Action OnBackPressed;
	
	private Button BackButton { get; set; }


	public override void _Ready()
	{
		base._Ready();
		
		BackButton = GetNode<Button>("%BackButton");

		BackButton.Pressed += Back;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		
		BackButton.Pressed -= Back;
	}

	private void Back()
	{
		OnBackPressed?.Invoke();
	}
}
