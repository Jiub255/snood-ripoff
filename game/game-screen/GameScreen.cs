using Godot;
using System;

public partial class GameScreen : TextureRect
{
	public event Action OnEndLevel;
	public event Action OnWinGame;
	public event Action OnLoseGame;
	
	private const int SPRITE_SIZE = 64;

	public Score Score { get; set; }
	
	private PanelContainer GameHolder { get; set; }
	private SnoodBoard BoardInstance { get; set; }
	private PackedScene[] Levels =
	{
		//GD.Load<PackedScene>("res://game/levels/level_1.tscn"),
		GD.Load<PackedScene>("res://game/levels/level_2.tscn"),
		//GD.Load<PackedScene>("res://game/levels/level_3.tscn"),
		GD.Load<PackedScene>("res://game/levels/level_4.tscn")
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
		SnoodsUsedLabel = GetNode<SnoodsUsedLabel>("%SnoodsUsedLabel");
		ScoreLabel = GetNode<ScoreLabel>("%ScoreLabel");
	}

	public void StartGame(Score score)
	{
		SetupScores(score);
		SetupLevel(1);
	}
	
	public void EndLevel()
	{
		BoardInstance.OnWonLevel -= OpenEndLevelMenu;
		BoardInstance.OnLost -= Lose;
		BoardInstance.OnTilemapChanged -= HandleTilemapChange;
		DangerBar.OnDangerBarFull -= BoardInstance.Tilemap.LowerBoard;
		
		BoardInstance.QueueFree();
		if (CurrentLevel < Levels.Length)
		{
			SetupLevel(CurrentLevel + 1);
		}
		else
		{
			OnWinGame?.Invoke();
		}
	}
	
	private void SetupScores(Score scores)
	{
		Score = scores;
		SnoodsUsedLabel.Scores = scores;
		ScoreLabel.Scores = scores;
	}

	private void SetupLevel(int level)
	{
		DangerBar.Value = 0;
		Input.MouseMode = Input.MouseModeEnum.Hidden;
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
		Score.ResetLevel();
		Score.BaseSnoodUseBonus = BoardInstance.BaseSnoodUseBonus;
		Score.PenaltyPerSnood = BoardInstance.PenaltyPerSnood;
		BoardInstance.SetupBoard();
		
		BoardInstance.OnWonLevel += OpenEndLevelMenu;
		BoardInstance.OnLost += Lose;
		BoardInstance.OnTilemapChanged += HandleTilemapChange;
		DangerBar.OnDangerBarFull += BoardInstance.Tilemap.LowerBoard;
	}

	private void Lose()
	{
		Score.Won = false;
		Score.AddUpScore();
		
		Input.MouseMode = Input.MouseModeEnum.Visible;
		BoardInstance.OnWonLevel -= OpenEndLevelMenu;
		BoardInstance.OnLost -= Lose;
		BoardInstance.OnTilemapChanged -= DangerBar.ChangeValue;
		DangerBar.OnDangerBarFull -= BoardInstance.Tilemap.LowerBoard;

		BoardInstance.QueueFree();
		OnLoseGame?.Invoke();
	}
	
	private void HandleTilemapChange(int similarSnoods, int droppedSnoods)
	{
		DangerBar.ChangeValue(similarSnoods, droppedSnoods);
		Score.AddSimilarSnoodsScore(similarSnoods);
		Score.AddDroppedChunkScore(droppedSnoods);
		Score.SnoodsUsed++;
	}

	private void ResizeGameHolder()
	{
		float width = BoardInstance.Columns * SPRITE_SIZE;
		GameHolder.CustomMinimumSize = new Vector2(width, GameHolder.CustomMinimumSize.Y);
	}

	private void OpenEndLevelMenu()
	{
		Score.Won = true;
		Score.AddUpScore();
		Input.MouseMode = Input.MouseModeEnum.Visible;
		OnEndLevel?.Invoke();
	}

	private void WinGame()
	{
		Input.MouseMode = Input.MouseModeEnum.Visible;
		OnWinGame?.Invoke();
	}
}
