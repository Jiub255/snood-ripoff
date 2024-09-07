using Godot;
using System;
using System.Collections.Generic;

public partial class GameScreen : TextureRect
{
	public event Action OnWinGame;
	
	private const int SPRITE_SIZE = 64;

	private PanelContainer GameHolder { get; set; }
	private SnoodBoard BoardInstance { get; set; }
	private Dictionary<int, PackedScene> Levels = new()
	{
		{ 1, GD.Load<PackedScene>("res://game/levels/level_1.tscn") },
		{ 2, GD.Load<PackedScene>("res://game/levels/level_2.tscn") },
		{ 3, GD.Load<PackedScene>("res://game/levels/level_3.tscn") }
	};
	private int CurrentLevel { get; set; }


	public override void _Ready()
	{
		base._Ready();
		
		GameHolder = GetNode<PanelContainer>("%GameHolder");
	}

	public void StartGame()
	{
		SetupLevel(1);
	}

	private void SetupLevel(int level)
	{
		CurrentLevel = level;
		BoardInstance = /* (SnoodBoard) */Levels[level].Instantiate<SnoodBoard>();
		GameHolder.CallDeferred(MethodName.AddChild, BoardInstance);
		BoardInstance.OnWinLevel += EndLevel;
		float width = BoardInstance.Columns * SPRITE_SIZE;
		GameHolder.CustomMinimumSize = new Vector2(width, GameHolder.CustomMinimumSize.Y);
	}
	
	private void EndLevel()
	{
		BoardInstance.OnWinLevel -= EndLevel;
		BoardInstance.QueueFree();
		if (Levels.ContainsKey(CurrentLevel + 1))
		{
			SetupLevel(CurrentLevel + 1);
		}
		else
		{
			WinGame();
		}
	}

	private void WinGame()
	{
		OnWinGame?.Invoke();
	}
}
