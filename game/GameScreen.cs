using Godot;
using System;

public partial class GameScreen : TextureRect
{
	public event Action OnEndLevel;
	public event Action OnWinGame;
	public event Action OnLoseGame;
	
	private const int SPRITE_SIZE = 64;

	public Scores Scores { get; set; }
	
	private PanelContainer GameHolder { get; set; }
	private SnoodBoard BoardInstance { get; set; }
	private PackedScene[] Levels =
	{
		GD.Load<PackedScene>("res://game/levels/level_1.tscn"),
		GD.Load<PackedScene>("res://game/levels/level_2.tscn"),
		//GD.Load<PackedScene>("res://game/levels/level_3.tscn"),
		//GD.Load<PackedScene>("res://game/levels/level_4.tscn")
	};
	private int CurrentLevel { get; set; }
	private SnoodsUsedLabel SnoodsUsedLabel { get; set; }
	private ScoreLabel ScoreLabel { get; set; }
	private DangerBar DangerBar { get; set; }


	public override void _Ready()
	{
		base._Ready();
		
		GameHolder = GetNode<PanelContainer>("%GameHolder");
		DangerBar = GetNode<DangerBar>("%DangerBar");
		DangerBar.Value = 0;
	}

	public void StartGame(Scores scores)
	{
		SetupScores(scores);
		SetupLevel(1);
	}
	
	public void EndLevel()
	{
		BoardInstance.OnGoToNextLevel -= OpenEndLevelMenu;
		BoardInstance.OnDead -= Die;
		
		BoardInstance.QueueFree();
		// CurrentLevel == [current Levels array index] + 1
		if (CurrentLevel < Levels.Length)
		{
			SetupLevel(CurrentLevel + 1);
		}
		else
		{
			OnWinGame?.Invoke();
		}
	}
	
	private void SetupScores(Scores scores)
	{
		Scores = scores;
		
		SnoodsUsedLabel ??= GetNode<SnoodsUsedLabel>("%SnoodsUsedLabel");
		ScoreLabel ??= GetNode<ScoreLabel>("%ScoreLabel");
		
		SnoodsUsedLabel.Scores = scores;
		ScoreLabel.Scores = scores;
	}

	private void SetupLevel(int level)
	{
		CurrentLevel = level;
		InstantiateBoard(level);
		InitializeBoard();
		ResizeGameHolder();
	}

	private void InstantiateBoard(int level)
	{
		BoardInstance = (SnoodBoard)Levels[level - 1].Instantiate();
		GameHolder.CallDeferred(MethodName.AddChild, BoardInstance);
	}

	private void InitializeBoard()
	{
		BoardInstance.OnGoToNextLevel += OpenEndLevelMenu;
		BoardInstance.OnDead += Die;
		
		Scores.ResetLevel();
		BoardInstance.SetupScores(Scores);
		BoardInstance.SetupDangerBar(DangerBar);
	}

	private void Die()
	{
		BoardInstance.OnGoToNextLevel -= OpenEndLevelMenu;
		BoardInstance.OnDead -= Die;

		BoardInstance.QueueFree();
		OnLoseGame?.Invoke();
	}

	private void ResizeGameHolder()
	{
		float width = BoardInstance.Columns * SPRITE_SIZE;
		GameHolder.CustomMinimumSize = new Vector2(width, GameHolder.CustomMinimumSize.Y);
	}

	private void OpenEndLevelMenu()
	{
		OnEndLevel?.Invoke();
	}

	private void WinGame()
	{
		OnWinGame?.Invoke();
	}
}
