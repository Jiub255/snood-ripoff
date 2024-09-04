using Godot;
using System;

public partial class SnoodBoard : TileMapLayer
{
	private Launcher Launcher { get; set; }

	public override void _Ready()
	{
		base._Ready();
		
		Launcher = GetNode<Launcher>("%Launcher");
		Launcher.Parent = this;
		Launcher.OnSnoodHit += AddSnoodToBoard;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		
		Launcher.OnSnoodHit -= AddSnoodToBoard;
	}
	
	private void AddSnoodToBoard(Vector2 coordinates)
	{
		Vector2I mapCoords = LocalToMap(coordinates);
		// Second parameter is the source index (probably just using one so always 0).
		// Third parameter always needs to be (0,0) to work with scene tiles,
		// Last parameter is the scene tile index, eventually get from flying snood.
		SetCell(mapCoords, 0, new Vector2I(0, 0), 0);
		GD.Print($"Added snood at {mapCoords}");
	}
}
