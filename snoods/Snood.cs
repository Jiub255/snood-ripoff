using Godot;
using System;

public partial class Snood : RigidBody2D
{
	[Export]
	private int altTileIndex = 0;
	
	public event Action<Vector2, int> OnHitStickyThing;
	
	public override void _Ready()
	{
		base._Ready();

		BodyEntered += HandleCollision;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		
		BodyEntered -= HandleCollision;
	}

	private void HandleCollision(Node body)
	{
		//GD.Print("Hit ANY thing");
		if (body is StickyStaticBody)
		{
			OnHitStickyThing?.Invoke(Position, altTileIndex);
			//GD.Print("Hit sticky thing");
		}
	}
}
