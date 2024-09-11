using Godot;
using System;

public partial class DangerBar : TextureProgressBar
{
	public event Action OnDangerBarFull;
	
	public void ChangeValue(int amount)
	{
		Value += amount;
		if (Value == MaxValue)
		{
			// TODO: Drop level down one block.
			OnDangerBarFull?.Invoke();
			//GD.Print("Danger Bar reached the top.");
			Value = 0;
		}
	}
}
