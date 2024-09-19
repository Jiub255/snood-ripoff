using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

// TODO: Move a lot of this to TileMapLayer extension class. 
// Might be more work than it's worth. Could get ugly. But could be good practice too.
public partial class SnoodBoard : Node2D
{
	public event Action OnGoToNextLevel;
	public event Action OnLost;
	
	private const int SPRITE_SIZE = 64;

	[Export(PropertyHint.Range, "7,20,0.5")]
	public float Columns { get; set; }
	[Export]
	public int BaseSnoodUseBonus { get; set; } = 10000;
	[Export]
	public int PenaltyPerSnood { get; set; } = 100;
	
	public Score Scores { get; set; }
	public DangerBar DangerBar { get; set; }
	public TileMapLayer Tilemap { get; private set; }
	
	private Launcher Launcher { get; set; }
	private StaticBody2D WallRight { get; set; }
	private Area2D BottomLimit { get; set; }
	private Dictionary<int, int> SnoodsByIndex { get; set; } = new();
	private Random RNG { get; } = new();
	private bool LevelWon { get; set; }
	private float LevelWonTimer { get; set; } = 1.5f;
	private bool LevelLost { get; set; }
	private float LevelLostTimer { get; set; } = 1.5f;
	
	private enum SpecialSnoods
	{
		DropWallSnood = 9,
		
	}

	public override void _Ready()
	{
		base._Ready();
		
		Launcher ??= GetNode<Launcher>("%Launcher");
		WallRight = GetNode<StaticBody2D>("%WallRight");
		Tilemap = GetNode<TileMapLayer>("%TileMapLayer");
		BottomLimit = GetNode<Area2D>("%BottomLimit");
		
		Launcher.Parent = this;
		Launcher.OnSnoodHit += AddSnoodToBoard;

		SetupBoard();
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		
		Launcher.OnSnoodHit -= AddSnoodToBoard;
		DangerBar.OnDangerBarFull -= LowerBoard;
	}

	public override void _Process(double delta)
	{
		if (LevelWon)
		{
			LevelWonTimer -= (float)delta;
			if (LevelWonTimer < 0)
			{
				OnGoToNextLevel?.Invoke();
				LevelWon = false;
			}
		}
		
		if (LevelLost)
		{
			LevelLostTimer -= (float)delta;
			if (LevelLostTimer < 0)
			{
				OnLost?.Invoke();
				LevelLost = false;
			}
		}
	}
	
	public void SetupScores(Score scores)
	{
		Scores = scores;
		Scores.BaseSnoodUseBonus = BaseSnoodUseBonus;
		Scores.PenaltyPerSnood = PenaltyPerSnood;
		Launcher ??= GetNode<Launcher>("%Launcher");
		Launcher.Scores = scores;
	}
	
	public void SetupDangerBar(DangerBar dangerBar)
	{
		DangerBar = dangerBar;
		DangerBar.OnDangerBarFull += LowerBoard;
	}

	private void SetupBoard()
	{
		float width = Columns * SPRITE_SIZE;
		WallRight.Position = new Vector2(width, WallRight.Position.Y);
		Launcher.Position = new Vector2(width / 2, Launcher.Position.Y);
		CountSnoods();
		//CallDeferred(MethodName.ConnectToSnoodSignals);
		Launcher.LoadSnood();
	}
	
/* 	private void ConnectToSnoodSignals()
	{
		foreach (Node child in Tilemap.GetChildren())
		{
			if (child is BadTileDropWall dropWall)
			{
				// TODO: Are these the same? Do I need to unsubscribe from the top one?
				dropWall.OnHitTile += LowerBoard;
				//dropWall.Connect(BadTileDropWall.SignalName.OnHitTile, new Callable(this, MethodName.LowerBoard));
			}
		}
	} */
	
	// TODO: Move to TileMapLayer extension class.
	private void CountSnoods()
	{
		foreach (Vector2I cell in Tilemap.GetUsedCells())
		{
			int altTileIndex = Tilemap.GetCellAlternativeTile(cell);
			if (altTileIndex > 0)
			{
				if (SnoodsByIndex.ContainsKey(altTileIndex))
				{
					SnoodsByIndex[altTileIndex]++;
				}
				else
				{
					SnoodsByIndex[altTileIndex] = 1;
				}

				Launcher.UpdateDictionary(altTileIndex);
			}
		}
	}
	
	// TODO: Move to TileMapLayer extension class.
	private void AddSnoodToBoard(Vector2 coordinates, int altTileIndex)
	{
		Vector2I mapCoordinates = Tilemap.LocalToMap(coordinates);
		Vector2I correctedMapCoords = CorrectForSides(mapCoordinates);
		SetSnood(correctedMapCoords, altTileIndex);
		UpdateSnoodCountDictionary(altTileIndex);

		// TODO: Handle touching special snoods here.
		HandleSpecialSnoods(mapCoordinates);

		IEnumerable<Vector2I> similarCells = GetTouchingCells(correctedMapCoords);
		if (similarCells.Count() >= 3)
		{
			AddSimilarSnoodsScore(similarCells.Count());
			foreach (Vector2I cell in similarCells)
			{
				DeleteCell(cell);
			}
			CheckForAndDropHangingChunks();
			DangerBar.ChangeValue(-10);
		}
		else
		{
			DangerBar.ChangeValue(30);
		}
		
		if (Tilemap.GetSurroundingCells(mapCoordinates).Where(x => Tilemap.GetCellAlternativeTile(x) > -1).Count() == 0)
		{
			DeleteCell(mapCoordinates);
		}
	}
	
	// TODO: Move to TileMapLayer extension class.
	private void HandleSpecialSnoods(Vector2I mapCoordinates)
	{
		foreach (Vector2I cell in Tilemap.GetSurroundingCells(mapCoordinates))
		{
			int tileIndex = Tilemap.GetCellAlternativeTile(cell);
			switch (tileIndex)
			{
				case (int)SpecialSnoods.DropWallSnood:
					LowerBoard();
					DeleteCell(cell);
					break;
			}
		}
		
	}
	
	// TODO: Move to TileMapLayer extension class? Probably not, using Launcher and Tilemap.
	private void LowerBoard()
	{
		Tilemap.Position = new Vector2(Tilemap.Position.X, Tilemap.Position.Y + SPRITE_SIZE);
		
		Snood preloaded = Launcher.PreloadedSnood;
		preloaded.Position = new Vector2(preloaded.Position.X, preloaded.Position.Y - SPRITE_SIZE);
		
		if (BottomLimit.HasOverlappingBodies())
		{
			Lose();
		}
	}

	// TODO: Move to TileMapLayer extension class? Maybe not, has BottomLimit. Or take out the set snood part, then have 
	// this class check bottom limit. 
	private void SetSnood(Vector2I mapCoords, int altTileIndex)
	{
		// Second parameter is the source index (probably just using one index so it should always be 0).
		// Third parameter always needs to be (0,0) to work with scene tiles,
		// Last parameter is the scene tile index, get from flying snood.
		Tilemap.SetCell(mapCoords, 0, new Vector2I(0, 0), altTileIndex);
		if (BottomLimit.HasOverlappingBodies())
		{
			Lose();
		}
	}
	
	private void Lose()
	{
		Launcher.Disabled = true;
		// Turn all snoods to skulls.
		// TODO: Put this in Process under "if (dead)" part, to change them one by one.
		foreach (Vector2I cell in Tilemap.GetUsedCells())
		{
			int altTileIndex = Tilemap.GetCellAlternativeTile(cell);
			if (altTileIndex >= 1 || altTileIndex <= 7)
			{
				Tilemap.SetCell(cell, 0, new Vector2I(0, 0), 8);
			}
		}
		
		Scores.Won = false;
		Scores.AddUpScore();
		
		// Starts countdown in _Process.
		LevelLost = true;
	}

	private void UpdateSnoodCountDictionary(int altTileIndex)
	{
		if (SnoodsByIndex.ContainsKey(altTileIndex))
		{
			SnoodsByIndex[altTileIndex]++;
		}
		else
		{
			SnoodsByIndex[altTileIndex] = 1;
			Launcher.UpdateDictionary(altTileIndex);
		}
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
	
	private void AddSimilarSnoodsScore(int numberOfSnoods)
	{
		Scores.Level += numberOfSnoods * numberOfSnoods + 1;
	}
	
	private void DeleteCell(Vector2I cell)
	{
		int index = Tilemap.GetCellAlternativeTile(cell);
		DecrementSnoodsCount(index);
		// Deletes cell from TileMapLayer
		Tilemap.SetCell(cell, -1);
		InstantiateAndDropDeadSnood(cell, index);
		if (SnoodsByIndex.Count == 0)
		{
			WinLevel();
		}
	}

	private void WinLevel()
	{
		Launcher.Disabled = true;
		Scores.Won = true;
		Scores.AddUpScore();
		// Starts countdown in _Process.
		LevelWon = true;
	}

	private void DecrementSnoodsCount(int index)
	{
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
		Snood deadSnood = (Snood)Launcher.Snoods[index].Instantiate();
		CallDeferred(MethodName.AddChild, deadSnood);
		deadSnood.Position = Tilemap.MapToLocal(cell) + Tilemap.Position;
		deadSnood.Freeze = false;
		return deadSnood;
	}

	private static void DisableCollisions(Snood deadSnood)
	{
		deadSnood.SetCollisionLayerValue(1, false);
		deadSnood.SetCollisionLayerValue(2, true);
		deadSnood.SetCollisionMaskValue(1, false);
		deadSnood.SetCollisionMaskValue(3, true);
	}

	private void DropSnood(Snood deadSnood)
	{
		deadSnood.GravityScale = 1;
		float randomAngle = (float)RNG.NextDouble() * Mathf.Pi;
		Vector2 randomDirection = Vector2.FromAngle(randomAngle);
		deadSnood.ApplyImpulse(randomDirection * 300);
	}

	private void CheckForAndDropHangingChunks()
	{
		List<List<Vector2I>> chunks = new();
		List<Vector2I> checkedCells = new();
		GatherChunks(chunks, checkedCells);
		CheckAndDropChunks(chunks);
	}

	private void GatherChunks(List<List<Vector2I>> chunks, List<Vector2I> checkedCells)
	{
		foreach (Vector2I cell in Tilemap.GetUsedCells())
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

	private void CheckAndDropChunks(List<List<Vector2I>> chunks)
	{
		foreach (List<Vector2I> chunk in chunks)
		{
			bool isOnCeiling = false;
			foreach (Vector2I cell in chunk)
			{
				// If it's a ceiling tile,
				if (Tilemap.GetCellAlternativeTile(cell) == 0)
				{
					isOnCeiling = true;
				}
			}
			if (!isOnCeiling)
			{
				DropChunk(chunk);
			}
		}
	}

	private void DropChunk(List<Vector2I> chunk)
	{
		AddDroppedChunkScore(chunk.Count);
		foreach (Vector2I cell in chunk)
		{
			DeleteCell(cell);
		}
	}

	private void AddDroppedChunkScore(int numberOfSnoods)
	{
		Scores.Level += 10 * numberOfSnoods * numberOfSnoods;
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
			int cellIndex = Tilemap.GetCellAlternativeTile(cell);
			int startCellIndex = Tilemap.GetCellAlternativeTile(startCell);
			if (!similarCellsOnly || cellIndex == startCellIndex)
			{
				connectedCells.Add(cell);

				// Get all surrounding cells and recursively check them
				foreach (Vector2I neighbor in Tilemap.GetSurroundingCells(cell))
				{
					// Ensure the neighbor is not a blank cell
					if (Tilemap.GetCellAlternativeTile(neighbor) != -1)
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
