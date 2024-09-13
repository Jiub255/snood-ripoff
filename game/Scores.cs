using Godot;
using System;

public class Scores
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
	public int Level
	{
		get => _level;
		set
		{
			_level = value;
			OnChanged?.Invoke();
		}
	}
	public int Total { get; private set; }
	public int BaseSnoodUseBonus { get; set; }
	public int PenaltyPerSnood { get; set; }
	public int SnoodUseBonus { get; set; }
	public bool Won { get; set; }
	
	
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
}
