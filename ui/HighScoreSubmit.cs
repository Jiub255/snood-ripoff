using Godot;

public partial class HighScoreSubmit : Control
{
	public LineEdit NameEntry { get; set; }

	public override void _Ready()
	{
		base._Ready();
		
		NameEntry = GetNode<LineEdit>("%NameEntry");
	}
}
