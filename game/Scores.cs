using Godot;
using System;

public class Scores
{
	public event Action OnChanged;
	
	private const int COMPLETION_BONUS = 1000;

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
	
	public void ResetLevelScore()
	{
		_level = 0;
		OnChanged?.Invoke();
	}
	
	public void AddCompletionBonus()
	{
		Total += COMPLETION_BONUS;
		OnChanged?.Invoke();
	}
}
