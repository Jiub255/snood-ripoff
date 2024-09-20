using Godot;
using System;

public partial class DangerBar : TextureProgressBar
{
	public event Action OnDangerBarFull;

	private const int ADD_AMOUNT = 30;
	private const int REMOVE_AMOUNT = 10;
	
	public void ChangeValue(int similarSnoods, int droppedSnoods)
	{
		int amount = 0;
		if (similarSnoods == 0 && droppedSnoods == 0)
		{
			amount += ADD_AMOUNT;
		}
		else
		{
			amount -= (similarSnoods + droppedSnoods) * REMOVE_AMOUNT;
		}
		
		Value += amount;
		if (Value == MaxValue)
		{
			// Drops level down one block.
			OnDangerBarFull?.Invoke();
			Value = 0;
		}
	}
}
