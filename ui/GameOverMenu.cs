using Godot;
using System;

public partial class GameOverMenu : Control
{
	public event Action OnDonePressed;

	private Label Message { get; set; }
	private Label TotalScore { get; set; }
	private Button DoneButton { get; set; }

	public override void _Ready()
	{
		base._Ready();
		
		Message = GetNode<Label>("%Message");
		TotalScore = GetNode<Label>("%TotalScore");
		DoneButton = GetNode<Button>("%Button");

		DoneButton.Pressed += NextLevel;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		
		DoneButton.Pressed -= NextLevel;
	}

	public void SetupMenu(Scores scores)
	{
		Message.Text = scores.Won ? "Congratulations! You Won!" : "You lost. No one likes you.";
		TotalScore.Text = $"Total Score: {scores.Total}";
		// TODO: Setup HighScoreSubmit stuff.
	}
	
	private void NextLevel()
	{
		// TODO: Use different event for high score submit? Or handle in UI?
		// Probably different event, can hold data like name and score.
		OnDonePressed?.Invoke();
		Hide();
	}
}
