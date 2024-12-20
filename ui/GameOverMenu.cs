using Godot;
using System;

public partial class GameOverMenu : Control
{
	public event Action OnDonePressed;

	private const string WIN_MESSAGE = "Congratulations! You Won!";
	private const string LOSE_MESSAGE = "You lost. No one likes you.";
	
	public LeaderboardHandler LeaderboardHandler { get; set; }

	private Label Message { get; set; }
	private Label TotalScore { get; set; }
	private HighScoreSubmit HighScoreSubmit { get; set; }
	private Button DoneButton { get; set; }
	private int HighScore { get; set; }
	private int Place { get; set; }
	private AudioStreamPlayer Music { get; set; }
	private AudioStream WinSong { get; } = GD.Load<AudioStream>("res://assets/music/Victory-2024-08-01.ogg");
	private AudioStream LoseSong { get; } = GD.Load<AudioStream>("res://assets/music/Death-2024-06-05_01.ogg");


	public override void _Ready()
	{
		base._Ready();
		
		Message = GetNode<Label>("%Message");
		TotalScore = GetNode<Label>("%TotalScore");
		HighScoreSubmit = GetNode<HighScoreSubmit>("%HighScoreSubmit");
		DoneButton = GetNode<Button>("%Button");
		Music = GetNode<AudioStreamPlayer>("%Music");

		DoneButton.Pressed += NextLevel;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		
		DoneButton.Pressed -= NextLevel;
	}

	public void SetupMenu(Score score)
	{
		Message.Text = score.Won ? WIN_MESSAGE : LOSE_MESSAGE;
		Music.Stream = score.Won ? WinSong : LoseSong;
		Music.Play();
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
		// Only check top ten. Or less if there aren't ten scores yet. 
		int highScoresCount = LeaderboardHandler.HighScores.Count;
		for (int index = 0; index < Mathf.Min(10, highScoresCount); index++)
		{
			if (score.Total >= LeaderboardHandler.HighScores[index].Score)
			{
				Place = index;
				return true;
			}
		}
		if (highScoresCount < 10)
		{
			Place = highScoresCount;
			return true;
		}

		Place = -1;
		return false;
	}
	
	private void NextLevel()
	{
		if (Place > -1)
		{
			HighScoreSubmit.NameEntry.Text = HighScoreSubmit.NameEntry.Text.Trim();
			if (HighScoreSubmit.NameEntry.Text == "")
			{
				HighScoreSubmit.NameEntry.PlaceholderText = "Name must be non-empty";
				return;
			}
			AddHighScore();
		}
		OnDonePressed?.Invoke();
		Hide();
		Music.Stop();
	}

	private void AddHighScore()
	{
		HighScore newHighScore = new(HighScoreSubmit.NameEntry.Text, HighScore);
		LeaderboardHandler.UploadScore(newHighScore);
	}
}
