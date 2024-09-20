using Godot;

public partial class SnoodSprite : AnimatedSprite2D
{
	private const float MIN_DURATION = 3f;
	private const float MAX_DURATION = 10f;
	
	private int FrameCount { get; set; }
	private RandomNumberGenerator RNG { get; set; } = new();
	private float Timer { get; set; }
	
	public override void _Ready()
	{
		base._Ready();

		FrameCount = SpriteFrames.GetFrameCount("default");
		ChooseRandomFrame();
		SetRandomTimerDuration();
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		
		Timer -= (float)delta;
		if (Timer < 0)
		{
			ChooseRandomFrame();
			SetRandomTimerDuration();
		}
	}
	
	private void ChooseRandomFrame()
	{
		int randomIndex = RNG.RandiRange(0, FrameCount - 1);
		Frame = randomIndex;
	}
	
	private void SetRandomTimerDuration()
	{		
		float randomDuration = RNG.RandfRange(MIN_DURATION, MAX_DURATION);
		Timer = randomDuration;
	}
}
