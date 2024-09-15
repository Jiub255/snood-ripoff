public struct HighScore
{
	public string Name { get; }
	public int Score { get; }
	
	public HighScore(string name, int score)
	{
		Name = name;
		Score = score;
	}
}
