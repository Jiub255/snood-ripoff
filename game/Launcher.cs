using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Launcher : Node2D
{
	private const float MIN_ANGLE = 15;
	
	// TODO: Pass int id to know which snood to instantiate on the tilemap.
	public event Action<Vector2, int> OnSnoodHit;
	
	public Dictionary<int, PackedScene> Snoods { get; } = new()
	{
		{ 1, GD.Load<PackedScene>("res://snoods/snood_red.tscn") },
		{ 2, GD.Load<PackedScene>("res://snoods/snood_dark_blue.tscn") },
		{ 3, GD.Load<PackedScene>("res://snoods/snood_yellow.tscn") }
	};
	public Dictionary<int, PackedScene> SnoodsInUse { get; } = new()
	{
		{ 1, GD.Load<PackedScene>("res://snoods/snood_red.tscn") },
		{ 2, GD.Load<PackedScene>("res://snoods/snood_dark_blue.tscn") },
		{ 3, GD.Load<PackedScene>("res://snoods/snood_yellow.tscn") }
	};
	public Vector2 AimDirection { get; set; } = Vector2.Up;
	public Node Parent { get; set; }
	
	private Snood LoadedSnood { get; set; }
	private Snood FlyingSnood { get; set; }
	private float Speed { get; set; } = 1500f;
	private bool Reloading { get; set; }
	private float ReloadTimer { get; set; }
	private bool SnoodLanded { get; set; } = true;
	private AnimatedSprite2D Sprite { get; set; }
	private Random RNG { get; } = new();
	private float ReloadDuration { get; } = 1f;

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
				if (SnoodLanded)
				{
					LoadSnood();
				}
			}
		}
	}

	private void LoadSnood()
	{
		if (SnoodsInUse.Count == 0)
		{
			return;
		}
		int randomIndex = RNG.Next(0, SnoodsInUse.Count);
		PackedScene snoodScene = SnoodsInUse.ElementAt(randomIndex).Value;
		LoadedSnood = (Snood)snoodScene.Instantiate();
		Parent.CallDeferred(MethodName.AddChild, LoadedSnood);
		LoadedSnood.Position = Position;
		LoadedSnood.OnHitStickyThing += NotifyTilesetAboutSnoodHit;
	}

	public void Shoot()
	{
		if (!Reloading && SnoodLanded)
		{
			FlyingSnood = LoadedSnood;
			FlyingSnood.LinearVelocity = AimDirection * Speed;
			Reloading = true;
			SnoodLanded = false;
		}
	}

	private void Rotate()
	{
		Vector2 vectorToMousePosition = (GetGlobalMousePosition() - GlobalPosition).Normalized();
		float angle = vectorToMousePosition.Angle() + (Mathf.Pi / 2);
		if (angle > Mathf.Pi)
		{
			angle -= 2 * Mathf.Pi;
		}
		Sprite.Rotation = Mathf.Clamp(
			angle,
			(-Mathf.Pi / 2) + Mathf.DegToRad(MIN_ANGLE),
			(Mathf.Pi / 2) - Mathf.DegToRad(MIN_ANGLE));
		AimDirection = Vector2.FromAngle(Sprite.Rotation - (Mathf.Pi / 2));
		//Sprite.Rotation = angle;
		//GD.Print($"Rotation: {Sprite.Rotation}");
	}
	
	private void NotifyTilesetAboutSnoodHit(Vector2 cooordinates, int altTileIndex)
	{
		FlyingSnood.OnHitStickyThing -= NotifyTilesetAboutSnoodHit;
		FlyingSnood.QueueFree();
		OnSnoodHit?.Invoke(cooordinates, altTileIndex);
		SnoodLanded = true;
		if (!Reloading)
		{
			LoadSnood();
		}
		//GD.Print("Notify tileset");
	}
}
