using Godot;
using System;

public partial class Launcher : Node2D
{
	// TODO: Pass int id to know which snood to instantiate on the tilemap.
	public event Action<Vector2> OnSnoodHit;
	
	private PackedScene SnoodScene { get; } = GD.Load<PackedScene>("res://snood.tscn");
	private Snood LoadedSnood { get; set; }
	private Snood FlyingSnood { get; set; }
	public Vector2 AimDirection { get; set; } = Vector2.Up;
	private float Speed { get; set; } = 700f;
	private bool Reloading { get; set; }
	private float ReloadTimer { get; set; }
	private float ReloadDuration { get; } = 1f;
	public Node Parent { get; set; }
	private AnimatedSprite2D Sprite { get; set; }

	public override void _Ready()
	{
		base._Ready();

		ReloadTimer = ReloadDuration;
		CallDeferred(MethodName.LoadSnood);
		Sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
	}

	public override void _Input(InputEvent @event)
	{
		base._Input(@event);
		
		if (@event.IsActionPressed("shoot"))
		{
			Shoot();
		}
		else if (@event is InputEventMouseMotion)
		{
			Rotate();
		}
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		
		if (Reloading)
		{
			ReloadTimer -= (float)delta;
			if (ReloadTimer < 0)
			{
				ReloadTimer = ReloadDuration;
				Reloading = false;
				LoadSnood();
			}
		}
	}

	private void LoadSnood()
	{
		LoadedSnood = (Snood)SnoodScene.Instantiate();
		Parent.AddChild(LoadedSnood);
		LoadedSnood.Position = Position;
		LoadedSnood.OnHitStickyThing += NotifyTilesetAboutSnoodHit;
	}

	public void Shoot()
	{
		if (!Reloading)
		{
			FlyingSnood = LoadedSnood;
			FlyingSnood.LinearVelocity = AimDirection * Speed;
			Reloading = true;
		}
	}

	private void Rotate()
	{
		AimDirection = (GetGlobalMousePosition() - Position).Normalized();
		Sprite.Rotation = AimDirection.Angle() + (Mathf.Pi / 2);
	}
	
	private void NotifyTilesetAboutSnoodHit(Vector2 cooordinates)
	{
		FlyingSnood.OnHitStickyThing -= NotifyTilesetAboutSnoodHit;
		OnSnoodHit?.Invoke(cooordinates);
		FlyingSnood.QueueFree();
		GD.Print("Notify tileset");
	}
}
