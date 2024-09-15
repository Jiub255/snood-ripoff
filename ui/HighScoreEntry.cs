using Godot;

public partial class HighScoreEntry : PanelContainer
{
	private Label Place { get; set; }
	private new Label Name { get; set; }
	private Label Score { get; set; }

	public override void _Ready()
	{
		base._Ready();
		
		Place = GetNode<Label>("%Place");
		Name = GetNode<Label>("%Name");
		Score = GetNode<Label>("%Score");
	}

	public void SetupEntry(int place, HighScore highScore)
	{
		Place.Text = place.ToString();
		Name.Text = highScore.Name;
		Score.Text = highScore.Score.ToString();
	}
}
