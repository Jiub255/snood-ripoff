using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class SnoodBoard : TileMapLayer
{
	private Launcher Launcher { get; set; }

	public override void _Ready()
	{
		base._Ready();
		
		Launcher = GetNode<Launcher>("%Launcher");
		Launcher.Parent = this;
		Launcher.OnSnoodHit += AddSnoodToBoard;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		
		Launcher.OnSnoodHit -= AddSnoodToBoard;
	}
	
	
	private void AddSnoodToBoard(Vector2 coordinates)
	{
		Vector2I mapCoords = LocalToMap(coordinates);
		// Second parameter is the source index (probably just using one so always 0).
		// Third parameter always needs to be (0,0) to work with scene tiles,
		// Last parameter is the scene tile index, eventually get from flying snood.
		SetCell(mapCoords, 0, new Vector2I(0, 0), 0);
		GD.Print($"Added snood at {mapCoords}");
		IEnumerable<Vector2I> similarCells = GetSimilarTouchingCells(mapCoords);
		GD.Print($"Similar cells size: {similarCells.Count()}");
		foreach (Vector2I cell in similarCells)
		{
			GD.Print($"Cell: {cell}");
		}
		if (similarCells.Count() >= 3)
		{
			foreach (Vector2I cell in similarCells)
			{
				GD.Print($"Deleting cell: {cell}");
				DeleteCell(cell);
			}
		}
	}
	
	private void DeleteCell(Vector2I cell)
	{
		SetCell(cell, -1);
	}
	
	private IEnumerable<Vector2I> GetSimilarTouchingCells(
		Vector2I centerCell, 
		List<Vector2I> similarCells = null,
		List<Vector2I> checkedCells = null,
		List<Vector2I> recursedCells = null)
	{
		// Hacky way to have non-compile-time-constant default variable lists.
		similarCells ??= new List<Vector2I>();
		checkedCells ??= new List<Vector2I>();
		recursedCells ??= new List<Vector2I>();
		recursedCells.Add(centerCell);
		// Get surrounding cells.
		List<Vector2I> surroundingCells = GetSurroundingCells(centerCell).ToList();
		surroundingCells.Add(centerCell);
		// For each of the seven cells,
		foreach (Vector2I surroundingCell in surroundingCells)
		{
			// Don't check blank cells.
			if (GetCellAlternativeTile(surroundingCell) == -1)
			{
				continue;
			}
			// If cell HASNT been checked,
			if (!checkedCells.Contains(surroundingCell))
			{
			// Add to checked cells list.
				checkedCells.Add(surroundingCell);
				// If cell matches center cell,
				if (GetCellAlternativeTile(surroundingCell) == GetCellAlternativeTile(centerCell))
				{
					// Add to similar cells list.
					similarCells.Add(surroundingCell);
					//yield return surroundingCell;
				}
			}
		}
		// For all similar cells ...
		List<Vector2I> cellsToAdd = new();
		List<Vector2I> similarCellsCopy = new();
		foreach (Vector2I similarCell in similarCells)
		{
			similarCellsCopy.Add(similarCell);
		}
		foreach (Vector2I similarCell in similarCellsCopy)
		{
			// In the surrounding cells EXCEPT center cell,
			if (surroundingCells.Contains(similarCell) && similarCell != centerCell)
			{
				// Need to check if cell has been recursed yet. A third list?
				// There's gotta be a cleaner way. 
				if (!recursedCells.Contains(similarCell))
				{
					// Recursively call method.
					cellsToAdd.AddRange(GetSimilarTouchingCells(
						similarCell, 
						similarCells, 
						checkedCells, 
						recursedCells));
				}
			}
		}
		similarCells.AddRange(cellsToAdd);
		return similarCells;
	}
	
/* 	private IEnumerable<Vector2I> GetSimilarTouchingCells(
		Vector2I centerCell, 
		List<Vector2I> similarCells = null,
		List<Vector2I> checkedCells = null)
	{
		similarCells ??= new List<Vector2I>();
		checkedCells ??= new List<Vector2I>();
		if (!similarCells.Contains(centerCell))
		{
			similarCells.Add(centerCell);
			checkedCells.Add(centerCell);
			yield return centerCell;
			GD.Print($"Added cell {centerCell}, in first block.");
		}
		int centerCellIndex = GetCellAlternativeTile(centerCell);
		Array<Vector2I> surroundingCells = GetSurroundingCells(centerCell);
		foreach (Vector2I surroundingCell in surroundingCells)
		{
			int surroundingCellIndex = GetCellAlternativeTile(surroundingCell);
			// Don't check blank cells.
			if (surroundingCellIndex == -1)
			{
				continue;
			}
			bool cellsAreSameType = surroundingCellIndex == centerCellIndex;
			if (cellsAreSameType)
			{
				if (!similarCells.Contains(surroundingCell))
				{
					foreach (Vector2I cell in GetSimilarTouchingCells(surroundingCell, similarCells))
					{
						// TODO: Is this necessary?
						if (!similarCells.Contains(cell))
						{
							similarCells.Add(cell);
							yield return cell;
							GD.Print($"Added cell {cell}, in third (possibly unnecessary) block.");
						}
					}
				}
			}
		}
	} */
}
