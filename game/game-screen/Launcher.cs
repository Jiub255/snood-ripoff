using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Launcher : Node2D
{
	private const float MIN_ANGLE = 15;

	public event Action<Vector2, int> OnSnoodHit;

	public Dictionary<int, PackedScene> Snoods { get; } = new()
	{
		{ 1, GD.Load<PackedScene>("res://snoods/snood_red.tscn") },
		{ 2, GD.Load<PackedScene>("res://snoods/snood_dark_blue.tscn") },
		{ 3, GD.Load<PackedScene>("res://snoods/snood_yellow.tscn") },
		{ 4, GD.Load<PackedScene>("res://snoods/snood_green.tscn") },
		{ 5, GD.Load<PackedScene>("res://snoods/snood_purple.tscn") },
		{ 6, GD.Load<PackedScene>("res://snoods/snood_light_blue.tscn") },
		{ 7, GD.Load<PackedScene>("res://snoods/snood_gray.tscn") },
		{ 8, GD.Load<PackedScene>("res://snoods/snood_skull.tscn") }
	};
	public Dictionary<int, PackedScene> SnoodsInUse { get; } = new();
	public Vector2 AimDirection { get; set; } = Vector2.Up;
	public SnoodBoard Parent { get; set; }
	public bool Disabled { get; set; }
	public Score Scores { get; set; }

	public Snood PreloadedSnood { get; private set; }
	private Snood LoadedSnood { get; set; }
	private Snood FlyingSnood { get; set; }
	private float Speed { get; set; } = 1500f;
	private float ReloadTimer { get; set; }
	private bool Reloading { get; set; }
	private bool SnoodLanded { get; set; } = true;
	private AnimatedSprite2D Sprite { get; set; }
	private Random RNG { get; } = new(69420);
	private float ReloadDuration { get; } = 1f;
	// FOR TESTING
	private Vector2 _offset = new(128, 0);


	public override void _Ready()
	{
		base._Ready();

		ReloadTimer = ReloadDuration;
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
		if (!Reloading && SnoodLanded && !Disabled && LoadedSnood != null)
		{
			FlyingSnood = LoadedSnood;
			FlyingSnood.Freeze = false;
			FlyingSnood.LinearVelocity = AimDirection * Speed;
			Reloading = true;
			SnoodLanded = false;
			Scores.SnoodsUsed++;
		}
	}

	public void UpdateDictionary(int altTileIndex)
	{
		if (altTileIndex == 8) return;
		if (!SnoodsInUse.ContainsKey(altTileIndex) && Snoods.ContainsKey(altTileIndex))
		{
			SnoodsInUse.Add(altTileIndex, Snoods[altTileIndex]);
		}
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
		LoadedSnood.Position -= _offset;

		PreloadSnood(snoodScene);
	}

	private void PreloadSnood(PackedScene snoodScene)
	{
		PreloadedSnood = (Snood)snoodScene.Instantiate();
		Parent.Tilemap.CallDeferred(MethodName.AddChild, PreloadedSnood);
		PreloadedSnood.Position = Position + _offset - Parent.Tilemap.Position;
		PreloadedSnood.OnHitStickyThing += HandleSnoodHit;
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
	}

	private void HandleSnoodHit(Vector2 cooordinates, int altTileIndex)
	{
		FlyingSnood.OnHitStickyThing -= HandleSnoodHit;
		FlyingSnood.QueueFree();
		OnSnoodHit?.Invoke(cooordinates, altTileIndex);
		SnoodLanded = true;
		if (!Reloading)
		{
			LoadSnood();
		}
	}
}