using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class SnoodBoard : TileMapLayer
{
	private const int SPRITE_SIZE = 64;
	
	private Launcher Launcher { get; set; }
	private StaticBody2D WallLeft { get; set; }
	private Dictionary<int, int> SnoodsByIndex { get; set; } = new();

	public override void _Ready()
	{
		base._Ready();
		
		Launcher = GetNode<Launcher>("%Launcher");
		WallLeft = GetNode<StaticBody2D>("%WallLeft");
		
		Launcher.Parent = this;
		Launcher.OnSnoodHit += AddSnoodToBoard;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		
		Launcher.OnSnoodHit -= AddSnoodToBoard;
	}
	
	public void SetupBoard(int width)
	{
		WallLeft.Position = new Vector2(width, WallLeft.Position.Y);
		Launcher.Position = new Vector2(width / 2, Launcher.Position.Y);
		CountSnoods();
	}
	
	private void CountSnoods()
	{
		Godot.Collections.Array<Vector2I> cells = GetUsedCells();
		foreach (Vector2I cell in cells)
		{
			int index = GetCellAlternativeTile(cell);
			if (index >= 0)
			{
				if (SnoodsByIndex.ContainsKey(index))
				{
					SnoodsByIndex[index]++;
				}
				else
				{
					SnoodsByIndex[index] = 1;
				}
			}
		}
		for (int altTileIndex = Launcher.SnoodsInUse.Count - 1; altTileIndex >= 0; altTileIndex--)
		{
			if (!SnoodsByIndex.ContainsKey(altTileIndex))
			{
				Launcher.SnoodsInUse.Remove(altTileIndex);
			}
		}
	}
	
	private void AddSnoodToBoard(Vector2 coordinates, int altTileIndex)
	{
		Vector2I mapCoords = LocalToMap(coordinates);
		// Second parameter is the source index (probably just using one so always 0).
		// Third parameter always needs to be (0,0) to work with scene tiles,
		// Last parameter is the scene tile index, get from flying snood.
		SetCell(mapCoords, 0, new Vector2I(0, 0), altTileIndex);
		if (SnoodsByIndex.ContainsKey(altTileIndex))
		{
			SnoodsByIndex[altTileIndex]++;
		}
		else
		{
			SnoodsByIndex[altTileIndex] = 1;
			Launcher.SnoodsInUse.Add(altTileIndex, Launcher.Snoods[altTileIndex]);
		}
		GD.Print($"Added snood at {mapCoords}");
		
		IEnumerable<Vector2I> similarCells = GetSimilarTouchingCells(mapCoords);
		GD.Print($"Similar cells size: {similarCells.Count()}");
/* 		foreach (Vector2I cell in similarCells)
		{
			GD.Print($"Cell: {cell}");
		} */
		if (similarCells.Count() >= 3)
		{
			foreach (Vector2I cell in similarCells)
			{
				//GD.Print($"Deleting cell: {cell}");
				DeleteCell(cell);
			}
		}
	}
	
	private void DeleteCell(Vector2I cell)
	{
		int index = GetCellAlternativeTile(cell);
		if (SnoodsByIndex.ContainsKey(index))
		{
			if (SnoodsByIndex[index] > 1)
			{
				SnoodsByIndex[index]--;
			}
			else
			{
				SnoodsByIndex.Remove(index);
				Launcher.SnoodsInUse.Remove(index);
			}
		}
		SetCell(cell, -1);
	}
	
	private IEnumerable<Vector2I> GetSimilarTouchingCells(Vector2I centerCell, List<Vector2I> checkedCells = null)
	{
		// Workaround to have non-compile-time-constant default variable lists.
		checkedCells ??= new List<Vector2I>();
		
		List<Vector2I> surroundingCells = GetSurroundingCells(centerCell).ToList();
		List<Vector2I> newlyCheckedCells = new();
		List<Vector2I> similarCells = new();
		surroundingCells.Add(centerCell);
		
		foreach (Vector2I surroundingCell in surroundingCells)
		{
			// Don't check blank cells.
			if (GetCellAlternativeTile(surroundingCell) == -1)
			{
				continue;
			}
			if (!checkedCells.Contains(surroundingCell))
			{
				newlyCheckedCells.Add(surroundingCell);
				if (GetCellAlternativeTile(surroundingCell) == GetCellAlternativeTile(centerCell))
				{
					similarCells.Add(surroundingCell);
				}
			}
		}
		
		// Add newly checked cells to checked list to pass on to next recursion.
		checkedCells.AddRange(newlyCheckedCells);
		
		List<Vector2I> cellsToAdd = new();
		
		foreach (Vector2I similarCell in similarCells)
		{
			if (newlyCheckedCells.Contains(similarCell))
			{
				cellsToAdd.AddRange(GetSimilarTouchingCells(similarCell, checkedCells));
			}
		}
		
		similarCells.AddRange(cellsToAdd);
		
		return similarCells;
	}
}
