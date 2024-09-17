using Godot;
using System;

public partial class GameOverMenu : Control
{
	public event Action OnDonePressed;
	
	public LeaderboardHandler LeaderboardHandler { get; set; }

	private Label Message { get; set; }
	private Label TotalScore { get; set; }
	private HighScoreSubmit HighScoreSubmit { get; set; }
	private Button DoneButton { get; set; }
	private int HighScore { get; set; }
	private int Place { get; set; }

	public override void _Ready()
	{
		base._Ready();
		
		Message = GetNode<Label>("%Message");
		TotalScore = GetNode<Label>("%TotalScore");
		HighScoreSubmit = GetNode<HighScoreSubmit>("%HighScoreSubmit");
		DoneButton = GetNode<Button>("%Button");

		DoneButton.Pressed += NextLevel;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		
		DoneButton.Pressed -= NextLevel;
	}

	public void SetupMenu(Score score)
	{
		Message.Text = score.Won ? "Congratulations! You Won!" : "You lost. No one likes you.";
		TotalScore.Text = $"Total Score: {score.Total}";
		HighScoreSubmit.Hide();
		if (IsHighScore(score))
		{
			HighScore = score.Total;
			HighScoreSubmit.Show();
		}
	}

	public bool IsHighScore(Score score)
	{
		// TODO: Only check top ten. Or less if there aren't ten scores yet. 
		for (int index = 0; index < LeaderboardHandler.HighScores.Count; index++)
		{
			if (score.Total >= LeaderboardHandler.HighScores[index].Score)
			{
				Place = index;
				return true;
			}
		}

		Place = 0;
		return false;
	}
	
	private void NextLevel()
	{
		if (Place > 0)
		{
			AddHighScore();
		}
		OnDonePressed?.Invoke();
		Hide();
	}

	private void AddHighScore()
	{
		HighScore newHighScore = new(HighScoreSubmit.NameEntry.Text, HighScore);
		LeaderboardHandler.UploadScore(newHighScore);
	}
}
