using Godot;

public partial class DeadSnoodDestroyer : Area2D
{
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

    private void HandleCollision(Node2D body)
	{
		if (body is Snood snood)
		{
			snood.QueueFree();
		}
	}
}
