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
			int before = _level;
			_level = value;
			int after = _level;
			int pointsAdded = after - before;
			Total += pointsAdded;
			OnChanged?.Invoke();
		}
	}
	public int Total { get; private set; }
	public int BaseSnoodUseBonus { get; set; }
	public int PenaltyPerSnood { get; set; }
	public int SnoodUseBonus { get; set; }
	
	
	public void ResetLevel()
	{
		_level = 0;
		SnoodsUsed = 0;
	}
	
	public void ApplyBonuses()
	{
		Total += COMPLETION_BONUS;
		SnoodUseBonus = BaseSnoodUseBonus - (SnoodsUsed * PenaltyPerSnood);
		SnoodUseBonus = Mathf.Max(SnoodUseBonus, MIN_SNOOD_USE_BONUS);
		Total += SnoodUseBonus;
		OnChanged?.Invoke();
	}
}
