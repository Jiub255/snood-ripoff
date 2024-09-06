using Godot;

public partial class GameScreen : TextureRect
{
	private const int SPRITE_SIZE = 64;
	
	private PanelContainer GameHolder { get; set; }
	private PackedScene BoardScene = GD.Load<PackedScene>("res://game/snood_board.tscn");

	public override void _Ready()
	{
		base._Ready();
		
		GameHolder = GetNode<PanelContainer>("%GameHolder");

		// TESTING
		SetupBoard(19);
	}
	
	private void SetupBoard(int columns)
	{
		int width = columns * SPRITE_SIZE;
		GameHolder.CustomMinimumSize = new Vector2(width, GameHolder.CustomMinimumSize.Y);
		SnoodBoard board = (SnoodBoard)BoardScene.Instantiate();
		GameHolder.AddChild(board);
		board.SetupBoard(width);
	}
}
