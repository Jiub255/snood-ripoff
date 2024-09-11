using Godot;

public partial class SnoodsUsedLabel : Label
{
	private Scores _scores;
	
	public Scores Scores
	{
		get => _scores;
		set
		{
			_scores = value;
			_scores.OnChanged += SetLabelText;
			SetLabelText();
		}
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		
		Scores.OnChanged -= SetLabelText;
	}

	private void SetLabelText()
	{
		Text = Scores.SnoodsUsed.ToString();
	}
}
