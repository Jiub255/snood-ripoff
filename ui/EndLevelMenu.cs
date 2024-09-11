using Godot;
using System;

public partial class EndLevelMenu : Control
{
	public event Action OnNextLevelPressed;
	
	public Scores Scores { get; set; }
	
	private Label SnoodsUsed { get; set; }
	private Label Score { get; set; }
	private Label TotalScore { get; set; }
	private Button NextLevelButton { get; set; }

	public override void _Ready()
	{
		base._Ready();
		
		SnoodsUsed = GetNode<Label>("%SnoodsUsed");
		Score = GetNode<Label>("%Score");
		TotalScore = GetNode<Label>("%TotalScore");
		NextLevelButton = GetNode<Button>("%Button");

		NextLevelButton.Pressed += NextLevel;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		
		NextLevelButton.Pressed -= NextLevel;
	}

	public void SetupMenu()
	{
		SnoodsUsed.Text = $"Snoods Used: {Scores.SnoodsUsed}";
		Score.Text = $"Score: {Scores.Level}";
		TotalScore.Text = $"Total Score: {Scores.Total}";
	}
	
	private void NextLevel()
	{
		OnNextLevelPressed?.Invoke();
		Hide();
	}
}
