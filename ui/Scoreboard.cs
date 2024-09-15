using Godot;
using System.Collections.Generic;

public partial class Scoreboard : VBoxContainer
{
	public List<HighScore> HighScores { get; set; }
	
	private HighScore[] DefaultHighScores { get; } = new HighScore[10]
	{
		new HighScore("James", 500000),
		new HighScore("James", 300000),
		new HighScore("James", 200000),
		new HighScore("James", 100000),
		new HighScore("James", 50000),
		new HighScore("James", 40000),
		new HighScore("James", 30000),
		new HighScore("James", 20000),
		new HighScore("James", 10000),
		new HighScore("James", 5000)
	};
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
	
	public void InitializeHighScores(List<HighScore> highScores)
	{
		HighScores = highScores;
		HighScores.AddRange(DefaultHighScores);
		SetupScores();
	}
	
	public void SetupScores()
	{
		int place = 1;
		int previousScore = -1;
		for (int index = 0; index < HighScores.Count; index++)
		{
			if (HighScores[index].Score < previousScore)
			{
				place++;
			}
			previousScore = HighScores[index].Score;
			Entries[index].SetupEntry(place, HighScores[index]);
		}
	}
}
