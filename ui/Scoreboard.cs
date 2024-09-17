using Godot;
using System.Collections.Generic;

public partial class Scoreboard : VBoxContainer
{
	private LeaderboardHandler LeaderboardHandler { get; set; }
	private List<HighScoreEntry> Entries { get; set; } = new();

	public override void _Ready()
	{
		base._Ready();
		
		foreach (Node child in GetChildren())
		{
			if (child is HighScoreEntry entry)
			{
				Entries.Add(entry);
			}
		}
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		
		LeaderboardHandler.OnHighScoresRecieved -= SetupScores;
	}

	public void InitializeHighScores(LeaderboardHandler leaderboardHandler)
	{
		LeaderboardHandler = leaderboardHandler;
		LeaderboardHandler.OnHighScoresRecieved += SetupScores;
		LeaderboardHandler.RequestHighScores();
	}
	
	public void SetupScores(List<HighScore> highScores)
	{
		int place = 1;
		int previousScore = 0;
		for (int index = 0; index < highScores.Count; index++)
		{
			if (highScores[index].Score < previousScore)
			{
				place++;
			}
			previousScore = highScores[index].Score;
			Entries[index].SetupEntry(place, highScores[index]);
		}
	}
}
