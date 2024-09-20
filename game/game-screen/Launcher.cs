using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Launcher : Node2D
{
	private const float MIN_ANGLE = 25f;
	private const float SPEED = 1500f;
	private const float RELOAD_DURATION = 1f;
	private const float ROTATION_SPEED = 270f;

	public event Action<Vector2, int> OnSnoodHit;

	public Dictionary<int, PackedScene> Snoods { get; set; }
	public Dictionary<int, PackedScene> SnoodsInUse { get; } = new();
	public Vector2 AimDirection { get; set; } = Vector2.Up;
	public bool Disabled { get; set; }
	public Snood PreloadedSnood { get; private set; }

	private Snood LoadedSnood { get; set; }
	private Snood FlyingSnood { get; set; }
	private Snood WaitingSnood { get; set; }
	private float ReloadTimer { get; set; }
	private bool Reloading { get; set; }
	private bool SnoodLanded { get; set; } = true;
	private AnimatedSprite2D Sprite { get; set; }
	private Random RNG { get; } = new(69420);
	private Vector2 Offset { get; } = new(128, 0);
	private bool OutOfControl { get; set; }
	private float TargetAngle { get; set; } = 9f;


	public override void _Ready()
	{
		base._Ready();

		ReloadTimer = RELOAD_DURATION;
		Sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		Rotate();
	}

	public override void _Input(InputEvent @event)
	{
		base._Input(@event);
		
		if (@event.IsActionPressed("shoot"))
		{
			Shoot();
		}
		else if (!OutOfControl && @event is InputEventMouseMotion)
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
				ReloadTimer = RELOAD_DURATION;
				Reloading = false;
				if (SnoodLanded)
				{
					LoadSnood();
				}
			}
		}
		
		if (OutOfControl)
		{
			RotateTowardRandomAngle((float)delta);
		}
	}

	public void SetupLauncher(Dictionary<int, PackedScene> snoods, float width)
	{
		Position = new Vector2(width / 2, Position.Y);
		Snoods = snoods;
	}

	public void LoadSnood()
	{
		if (SnoodsInUse.Count == 0 || Disabled)
		{
			return;
		}
		
		PackedScene snoodScene = ChooseRandomSnood();
		SetupSnood(snoodScene);
	}

	public void Shoot()
	{
		if (OutOfControl)
		{
			OutOfControl = false;
			CallDeferred(MethodName.Rotate);
		}
		if (!Reloading && SnoodLanded && !Disabled && LoadedSnood != null)
		{
			FlyingSnood = LoadedSnood;
			FlyingSnood.Freeze = false;
			FlyingSnood.LinearVelocity = AimDirection * SPEED;
			Reloading = true;
			SnoodLanded = false;
		}
	}

	public void UpdateDictionary(int altTileIndex)
	{
		if (altTileIndex == 8) return;
		if (!SnoodsInUse.ContainsKey(altTileIndex) && Snoods.ContainsKey(altTileIndex) && altTileIndex < 9)
		{
			SnoodsInUse.Add(altTileIndex, Snoods[altTileIndex]);
		}
	}
	
	public void LoseControl()
	{
		OutOfControl = true;
	}

	private PackedScene ChooseRandomSnood()
	{
		int randomIndex = RNG.Next(0, SnoodsInUse.Count);
		PackedScene snoodScene = SnoodsInUse.ElementAt(randomIndex).Value;
		return snoodScene;
	}

	private void SetupSnood(PackedScene snoodScene)
	{
		if (PreloadedSnood == null)
		{
			PreloadSnood(ChooseRandomSnood());
		}
		
		LoadedSnood = PreloadedSnood;
		LoadedSnood.CollisionMask = 1;
		LoadedSnood.Position -= Offset;

		PreloadSnood(snoodScene);
	}

	private void PreloadSnood(PackedScene snoodScene)
	{
		PreloadedSnood = (Snood)snoodScene.Instantiate();
		CallDeferred(MethodName.AddChild, PreloadedSnood);
		PreloadedSnood.Position = Offset;
		PreloadedSnood.OnHitStickyThing += HandleSnoodHit;
		PreloadedSnood.CollisionMask = 0;
	}

	private void Rotate()
	{
		Vector2 vectorToMousePosition = (GetGlobalMousePosition() - GlobalPosition).Normalized();
		float angle = Mathf.RadToDeg(vectorToMousePosition.Angle()) + 90f;
		if (angle > 180)
		{
			angle -= 360;
		}
		Sprite.RotationDegrees = Mathf.Clamp(angle, -90 + MIN_ANGLE, 90 - MIN_ANGLE);
		AimDirection = Vector2.FromAngle(Sprite.Rotation - (Mathf.Pi / 2));
	}
	
	private void RotateTowardRandomAngle(float delta)
	{
		float difference = TargetAngle - Sprite.RotationDegrees;
		float direction = difference == 0 ? 1 : difference / Mathf.Abs(difference);
		Sprite.RotationDegrees += direction * ROTATION_SPEED * delta;
		AimDirection = Vector2.FromAngle(Sprite.Rotation - (Mathf.Pi / 2));
		float newDifference = TargetAngle - Sprite.RotationDegrees;
		float newDirection = newDifference == 0 ? 1 : newDifference / Mathf.Abs(newDifference);
		if (newDirection - direction != 0)
		{
			ChooseRandomAngle();
		}
	}
	
	private void ChooseRandomAngle()
	{
		float maxAngle = 90 - MIN_ANGLE;
		float minAngle = -90 + MIN_ANGLE;
		TargetAngle = (float)RNG.NextDouble() * (maxAngle - minAngle) + minAngle;
	}

	private void HandleSnoodHit(Vector2 cooordinates, int altTileIndex)
	{
		FlyingSnood.OnHitStickyThing -= HandleSnoodHit;
		WaitingSnood = FlyingSnood;
		FlyingSnood = null;
		WaitingSnood.WaitForParticlesThenDie();
		OnSnoodHit?.Invoke(cooordinates, altTileIndex);
		SnoodLanded = true;
		if (!Reloading)
		{
			LoadSnood();
		}
	}
}
