using Godot;
using System;

public partial class EndLevelMenu : Control
{
	public event Action OnNextLevelPressed;
	
	private Label Score { get; set; }
	private Label SnoodsUsed { get; set; }
	private Label SnoodUseBonus { get; set; }
	private Label TotalScore { get; set; }
	private Button NextLevelButton { get; set; }

	public override void _Ready()
	{
		base._Ready();
		
		Score = GetNode<Label>("%Score");
		SnoodsUsed = GetNode<Label>("%SnoodsUsed");
		SnoodUseBonus = GetNode<Label>("%SnoodUseBonus");
		TotalScore = GetNode<Label>("%TotalScore");
		NextLevelButton = GetNode<Button>("%Button");

		NextLevelButton.Pressed += NextLevel;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		
		NextLevelButton.Pressed -= NextLevel;
	}

	public void SetupMenu(Score scores)
	{
		Score.Text = $"Level Score: {scores.Level}";
		SnoodsUsed.Text = $"Snoods Used: {scores.SnoodsUsed}";
		SnoodUseBonus.Text = $"Snood Use Bonus: {scores.SnoodUseBonus}";
		TotalScore.Text = $"Total Score: {scores.Total}";
	}
	
	private void NextLevel()
	{
		OnNextLevelPressed?.Invoke();
		Hide();
	}
}
