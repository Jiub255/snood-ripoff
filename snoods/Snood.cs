using Godot;
using System;

public partial class Snood : RigidBody2D
{
	[Export]
	private int altTileIndex = 0;
	
	public event Action<Vector2, int> OnHitStickyThing;
	
	private bool Waiting { get; set; }
	private float WaitTimer { get; set; } = 2f;
	private AudioStreamPlayer SFX { get; set; }
	//private AudioStream LaunchSound = GD.Load<AudioStream>("res://assets/sfx/explosion.wav");
	//private AudioStreamRandomizer BounceSoundRandomizer = GD.Load<AudioStreamRandomizer>("res://assets/sfx/hit_sound_randomizer.tres");
	
	public override void _Ready()
	{
		base._Ready();

		SFX = GetNode<AudioStreamPlayer>("%SFX");

		BodyEntered += HandleCollision;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		
		BodyEntered -= HandleCollision;
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		
		if (Waiting)
		{
			WaitTimer -= (float)delta;
			if (WaitTimer < 0)
			{
				QueueFree();
			}
		}
	}
	
	public void Launch(Vector2 launchVector)
	{
		Freeze = false;
		LinearVelocity = launchVector;
		//SFX.Stream = LaunchSound;
		SFX.Play();
	}

	public void WaitForParticlesThenDie()
	{
		Waiting = true;
		CollisionMask = 0;
		CollisionLayer = 0;
		foreach (Node child in GetChildren())
		{
			if (child is AnimatedSprite2D sprite)
			{
				sprite.Hide();
			}
			if (child is GpuParticles2D particles)
			{
				particles.Emitting = false;
			}
		}
	}

	private void HandleCollision(Node body)
	{
		if (body is StickyStaticBody)
		{
			OnHitStickyThing?.Invoke(GlobalPosition, altTileIndex);
		}
		if (body is not DeadSnoodDestroyer)
		{
			//SFX.Stream = BounceSoundRandomizer;
			SFX.Play();
		}
	}
}
