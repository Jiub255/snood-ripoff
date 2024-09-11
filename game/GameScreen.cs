using Godot;
using System;
using System.Collections.Generic;

public partial class GameScreen : TextureRect
{
	public event Action OnEndLevelMenu;
	public event Action OnWinGame;
	
	private const int SPRITE_SIZE = 64;

	public Scores Scores { get; set; }
	
	private PanelContainer GameHolder { get; set; }
	private SnoodBoard BoardInstance { get; set; }
	private Dictionary<int, PackedScene> Levels = new()
	{
		{ 1, GD.Load<PackedScene>("res://game/levels/level_1.tscn") },
		{ 2, GD.Load<PackedScene>("res://game/levels/level_2.tscn") },
		{ 3, GD.Load<PackedScene>("res://game/levels/level_3.tscn") }
	};
	private int CurrentLevel { get; set; }
	private SnoodsUsedLabel SnoodsUsedLabel { get; set; }
	private ScoreLabel ScoreLabel { get; set; }
	private TextureProgressBar DangerBar { get; set; }


	public override void _Ready()
	{
		base._Ready();
		
		GameHolder = GetNode<PanelContainer>("%GameHolder");
		DangerBar = GetNode<TextureProgressBar>("%DangerBar");
	}

	public void StartGame()
	{
		SetupLevel(1);
	}
	
	public void EndLevel()
	{
		BoardInstance.OnGoToNextLevel -= OpenEndLevelMenu;
		
		BoardInstance.QueueFree();
		if (Levels.ContainsKey(CurrentLevel + 1))
		{
			SetupLevel(CurrentLevel + 1);
		}
		else
		{
			OnWinGame?.Invoke();
		}
	}
	
	public void SetupScores(Scores scores)
	{
		Scores = scores;
		
		SnoodsUsedLabel = GetNode<SnoodsUsedLabel>("%SnoodsUsedLabel");
		ScoreLabel = GetNode<ScoreLabel>("%ScoreLabel");
		
		SnoodsUsedLabel.Scores = scores;
		ScoreLabel.Scores = scores;
	}

	private void SetupLevel(int level)
	{
		CurrentLevel = level;
		BoardInstance = (SnoodBoard)Levels[level].Instantiate();
		GameHolder.CallDeferred(MethodName.AddChild, BoardInstance);
		BoardInstance.OnGoToNextLevel += OpenEndLevelMenu;
		
		Scores.ResetLevelScore();
		BoardInstance.SetupScores(Scores);
		
		float width = BoardInstance.Columns * SPRITE_SIZE;
		GameHolder.CustomMinimumSize = new Vector2(width, GameHolder.CustomMinimumSize.Y);
	}
	
	private void OpenEndLevelMenu()
	{
		OnEndLevelMenu?.Invoke();
	}

	private void WinGame()
	{
		OnWinGame?.Invoke();
	}
}
