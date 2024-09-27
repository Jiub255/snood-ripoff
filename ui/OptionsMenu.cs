using Godot;
using System;

public partial class OptionsMenu : Control
{
	public event Action OnBackPressed;
	
	private HSlider SfxSlider { get; set; }
	private string SfxBusName { get; } = "SFX";
	private int SfxBusIndex { get; set;}
	private AudioStreamPlayer SFX { get; set; }
	
	private HSlider MusicSlider { get; set; }
	private string MusicBusName { get; } = "Music";
	private int MusicBusIndex { get; set;}
	
	private Button BackButton { get; set; }


	public override void _Ready()
	{
		base._Ready();
		
		SfxSlider = GetNode<HSlider>("%SfxSlider");
		SFX = GetNode<AudioStreamPlayer>("SFX");
		MusicSlider = GetNode<HSlider>("%MusicSlider");
		BackButton = GetNode<Button>("%BackButton");
		
		SfxBusIndex = AudioServer.GetBusIndex(SfxBusName);
		MusicBusIndex = AudioServer.GetBusIndex(MusicBusName);

		SfxSlider.ValueChanged += ChangeSfxVolume;
		MusicSlider.ValueChanged += ChangeMusicVolume;
		BackButton.Pressed += Back;

		AudioServer.SetBusVolumeDb(SfxBusIndex, Mathf.LinearToDb((float)0.5));
		AudioServer.SetBusVolumeDb(MusicBusIndex, Mathf.LinearToDb((float)0.5));
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		
		BackButton.Pressed -= Back;
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		
		
	}

	private void ChangeSfxVolume(double value)
	{
		AudioServer.SetBusVolumeDb(SfxBusIndex, Mathf.LinearToDb((float)value));
		if (!SFX.Playing)
		{
			SFX.Play();
		}
	}
	
	private void ChangeMusicVolume(double value)
	{
		AudioServer.SetBusVolumeDb(MusicBusIndex, Mathf.LinearToDb((float)value));
	}

	private void Back()
	{
		OnBackPressed?.Invoke();
	}
}
