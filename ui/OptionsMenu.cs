using Godot;
using System;

public partial class OptionsMenu : Control
{
	public event Action OnBackPressed;
	
	private HSlider SfxSlider { get; set; }
	private HSlider MusicSlider { get; set; }
	private Button BackButton { get; set; }


	public override void _Ready()
	{
		base._Ready();
		
		SfxSlider = GetNode<HSlider>("%SfxSlider");
		MusicSlider = GetNode<HSlider>("%MusicSlider");
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
