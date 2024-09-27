using Godot;
using System;

public class Score
{
	public event Action OnChanged;

	private const int COMPLETION_BONUS = 1000;
	private const int MIN_SNOOD_USE_BONUS = 100;

	private int _snoodsUsed;
	private int _level;

	public int SnoodsUsed
	{
		get => _snoodsUsed;
		set
		{
			_snoodsUsed = value;
			OnChanged?.Invoke();
		}
	}
	public int BaseSnoodUseBonus { get; set; }
	public int PenaltyPerSnood { get; set; }
	public bool Won { get; set; }
	public int Level
	{
		get => _level;
		private set
		{
			_level = value;
			OnChanged?.Invoke();
		}
	}
	public int Total { get; private set; }
	public int SnoodUseBonus { get; private set; }
	
	
	public void ResetLevel()
	{
		_level = 0;
		SnoodsUsed = 0;
	}
	
	public void AddUpScore()
	{
		if (Won)
		{
			Total += COMPLETION_BONUS;
			SnoodUseBonus = BaseSnoodUseBonus - (SnoodsUsed * PenaltyPerSnood);
			SnoodUseBonus = Mathf.Max(SnoodUseBonus, MIN_SNOOD_USE_BONUS);
			Total += SnoodUseBonus;
		}
		Total += Level;
	}
	
	public void AddSimilarSnoodsScore(int numberOfSnoods)
	{
		if (numberOfSnoods > 0)
		{
			Level += numberOfSnoods * numberOfSnoods + 1;
		}
	}
	
	public void AddDroppedChunkScore(int numberOfSnoods)
	{
		Level += 10 * numberOfSnoods * numberOfSnoods;
	}
}
