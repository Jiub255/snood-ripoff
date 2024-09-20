using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class SnoodTilemap : TileMapLayer
{
	private const int SPRITE_SIZE = 64;
	
	public event Action OnHitLoseControlSnood;
	public event Action<int> OnSnoodAdded;
	public event Action<int> OnSnoodDeleted;
	public event Action<int, int> OnTilemapChanged;
	
	private Dictionary<int, PackedScene> Snoods { get; set; }
	private float Columns { get; set; }
	
	private Random RNG { get; } = new();
	
	private enum SpecialSnoods
	{
		DropWall = 9,
		RaiseWall = 10,
		LoseControl = 11,
	}


	public void SetupTilemap(Dictionary<int, PackedScene> snoods, float columns)
	{
		Snoods = snoods;
		Columns = columns;
	}

	public void AddSnoodToBoard(Vector2 coordinates, int altTileIndex)
	{
		Vector2I mapCoordinates = LocalToMap(ToLocal(coordinates));
		Vector2I correctedMapCoords = CorrectForSides(mapCoordinates);
		SetSnood(correctedMapCoords, altTileIndex);

		HandleSpecialSnoods(mapCoordinates);

		int similarSnoods = 0;
		int droppedSnoods = 0;
		IEnumerable<Vector2I> similarCells = GetTouchingCells(correctedMapCoords);
		if (similarCells.Count() >= 3)
		{
			similarSnoods = similarCells.Count();
			foreach (Vector2I cell in similarCells)
			{
				DeleteCell(cell);
			}
			droppedSnoods = CheckForAndDropHangingChunks();
		}
		
		if (GetSurroundingCells(mapCoordinates).Where(x => GetCellAlternativeTile(x) > -1).Count() == 0)
		{
			DeleteCell(mapCoordinates);
		}

		OnTilemapChanged?.Invoke(similarSnoods, droppedSnoods);
	}
	
	public void LowerBoard()
	{
		Position = new Vector2(Position.X, Position.Y + SPRITE_SIZE);
	}
	
	private void HandleSpecialSnoods(Vector2I mapCoordinates)
	{
		foreach (Vector2I cell in GetSurroundingCells(mapCoordinates))
		{
			int tileIndex = GetCellAlternativeTile(cell);
			switch (tileIndex)
			{
				case (int)SpecialSnoods.DropWall:
					LowerBoard();
					DeleteCell(cell);
					break;
				
				case (int)SpecialSnoods.RaiseWall:
					RaiseBoard();
					DeleteCell(cell);
					break;
				
				case (int)SpecialSnoods.LoseControl:
					OnHitLoseControlSnood?.Invoke();
					DeleteCell(cell);
					break;
					
				default:
					break;
			}
		}
	}
	
	private void RaiseBoard()
	{
		if (Position.Y == 0) return;
		Position = new Vector2(Position.X, Position.Y - SPRITE_SIZE);
	}
	
	private void SetSnood(Vector2I mapCoords, int altTileIndex)
	{
		// Second parameter is the source index (probably just using one index so it should always be 0).
		// Third parameter always needs to be (0,0) to work with scene tiles,
		// Last parameter is the scene tile index, get from flying snood.
		SetCell(mapCoords, 0, new Vector2I(0, 0), altTileIndex);
		OnSnoodAdded?.Invoke(altTileIndex);
	}
	
	private Vector2I CorrectForSides(Vector2I mapCoords)
	{
		// Left wall
		if (mapCoords.X < 0)
		{
			Vector2I newCoords = new Vector2I(0, mapCoords.Y);
			return newCoords;
		}
		// Right wall
		// Odd rows
		if (mapCoords.Y % 2 == 1)
		{
			if (mapCoords.X > Columns - 1.5f)
			{
				Vector2I newCoords = new Vector2I(Mathf.FloorToInt(Columns - 1.5f), mapCoords.Y);
				return newCoords;
			}
		}
		// Even rows
		else if (mapCoords.X > Columns - 1)
		{
			Vector2I newCoords = new Vector2I((int)(Columns - 1), mapCoords.Y);
			return newCoords;
		}
		return mapCoords;
	}
	
	public void DeleteCell(Vector2I cell)
	{
		int index = GetCellAlternativeTile(cell);
		// Deletes cell from TileMapLayer
		SetCell(cell, -1);
		InstantiateAndDropDeadSnood(cell, index);
		OnSnoodDeleted?.Invoke(index);
	}
	
	private void InstantiateAndDropDeadSnood(Vector2I cell, int index)
	{
		if (index > -1)
		{
			Snood deadSnood = InstantiateDeadSnood(cell, index);
			DisableCollisions(deadSnood);
			DropSnood(deadSnood);
		}
	}
	
	private Snood InstantiateDeadSnood(Vector2I cell, int index)
	{
		Snood deadSnood = (Snood)Snoods[index].Instantiate();
		CallDeferred(MethodName.AddChild, deadSnood);
		deadSnood.Position = MapToLocal(cell);
		deadSnood.Freeze = false;
		return deadSnood;
	}

	private static void DisableCollisions(Snood deadSnood)
	{
		deadSnood.CollisionLayer = 0b10;
		deadSnood.CollisionMask = 0b100;
	}

	private void DropSnood(Snood deadSnood)
	{
		deadSnood.GravityScale = 1;
		float randomAngle = (float)RNG.NextDouble() * Mathf.Pi;
		Vector2 randomDirection = Vector2.FromAngle(randomAngle);
		deadSnood.ApplyImpulse(randomDirection * 300);
	}
	
	private int CheckForAndDropHangingChunks()
	{
		List<List<Vector2I>> chunks = new();
		List<Vector2I> checkedCells = new();
		GatherChunks(chunks, checkedCells);
		return CheckAndDropChunks(chunks);
	}

	private void GatherChunks(List<List<Vector2I>> chunks, List<Vector2I> checkedCells)
	{
		foreach (Vector2I cell in GetUsedCells())
		{
			if (checkedCells.Contains(cell))
			{
				continue;
			}
			IEnumerable<Vector2I> chunk = GetTouchingCells(cell, false);
			checkedCells.AddRange(chunk);
			chunks.Add(chunk.ToList());
		}
	}

	private int CheckAndDropChunks(List<List<Vector2I>> chunks)
	{
		int droppedSnoods = 0;
		foreach (List<Vector2I> chunk in chunks)
		{
			bool isOnCeiling = false;
			foreach (Vector2I cell in chunk)
			{
				// If it's a ceiling tile,
				if (GetCellAlternativeTile(cell) == 0)
				{
					isOnCeiling = true;
				}
			}
			if (!isOnCeiling)
			{
				droppedSnoods += DropChunk(chunk);
			}
		}
		return droppedSnoods;
	}

	private int DropChunk(List<Vector2I> chunk)
	{
		int droppedSnoods = 0;
		foreach (Vector2I cell in chunk)
		{
			DeleteCell(cell);
			droppedSnoods++;
		}
		return droppedSnoods;
	}
	
	private IEnumerable<Vector2I> GetTouchingCells(Vector2I startCell, bool similarCellsOnly = true)
	{
		List<Vector2I> connectedCells = new();
		HashSet<Vector2I> visitedCells = new(); // Use a HashSet for faster lookups

		// Helper function to perform the flood fill
		void FloodFill(Vector2I cell)
		{
			// If the cell has already been checked, skip it
			if (visitedCells.Contains(cell))
			{
				return;
			}
			visitedCells.Add(cell);

			// Add the cell if it matches the criteria
			int cellIndex = GetCellAlternativeTile(cell);
			int startCellIndex = GetCellAlternativeTile(startCell);
			if (!similarCellsOnly || cellIndex == startCellIndex)
			{
				connectedCells.Add(cell);

				// Get all surrounding cells and recursively check them
				foreach (Vector2I neighbor in GetSurroundingCells(cell))
				{
					// Ensure the neighbor is not a blank cell
					if (GetCellAlternativeTile(neighbor) != -1)
					{
						FloodFill(neighbor);
					}
				}
			}
		}

		// Start the flood fill from the initial cell
		FloodFill(startCell);

		return connectedCells;
	}
}
