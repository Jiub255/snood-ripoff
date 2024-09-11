using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class SnoodBoard : Node2D
{
	public event Action OnGoToNextLevel;
	
	private const int SPRITE_SIZE = 64;

	private bool _disabled;

	[Export(PropertyHint.Range, "7,20,0.5")]
	public float Columns { get; set; }
	[Export]
	public int BaseSnoodUseBonus { get; set; } = 10000;
	[Export]
	public int PenaltyPerSnood { get; set; } = 100;
	
	public bool LevelWon
	{
		get => _disabled;
		set
		{
			if (value == true)
			{
				EndLevelTimer = EndLevelDuration;
			}
			_disabled = value;
		}
	}
	public Scores Scores { get; set; }
	public DangerBar DangerBar { get; set; }
	public TileMapLayer Tilemap { get; private set; }
	
	private Launcher Launcher { get; set; }
	private StaticBody2D WallRight { get; set; }
	private Area2D BottomLimit { get; set; }
	private Dictionary<int, int> SnoodsByIndex { get; set; } = new();
	private Random RNG { get; } = new();
	private float EndLevelTimer { get; set; }
	private float EndLevelDuration { get; } = 1.5f;
	

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
			EndLevelTimer -= (float)delta;
			if (EndLevelTimer < 0)
			{
				OnGoToNextLevel?.Invoke();
				LevelWon = false;
			}
		}
	}
	
	public void SetupScores(Scores scores)
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
		Launcher.LoadSnood();
		LevelWon = false;
	}
	
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
	
	private void AddSnoodToBoard(Vector2 coordinates, int altTileIndex)
	{
		Vector2I mapCoords = Tilemap.LocalToMap(coordinates);
		SetSnood(mapCoords, altTileIndex);
		UpdateDictionary(altTileIndex);

		IEnumerable<Vector2I> similarCells = GetTouchingCells(mapCoords);
		if (similarCells.Count() >= 3)
		{
			AddSimilarSnoodsScore(similarCells.Count());
			foreach (Vector2I cell in similarCells)
			{
				DeleteCell(cell);
			}
			CheckForHangingChunks();
			DangerBar.ChangeValue(-10);
		}
		else
		{
			DangerBar.ChangeValue(30);
		}
	}
	
	private void LowerBoard()
	{
		// TODO: Lower board.
		Tilemap.Position = new Vector2(Tilemap.Position.X, Tilemap.Position.Y + SPRITE_SIZE);
		
		if (BottomLimit.HasOverlappingBodies())
		{
			Die();
		}
	}

	private Vector2I SetSnood(Vector2I mapCoords, int altTileIndex)
	{
		// Second parameter is the source index (probably just using one index so it should always be 0).
		// Third parameter always needs to be (0,0) to work with scene tiles,
		// Last parameter is the scene tile index, get from flying snood.
		mapCoords = CorrectForSides(mapCoords);
		Tilemap.SetCell(mapCoords, 0, new Vector2I(0, 0), altTileIndex);
		if (BottomLimit.HasOverlappingBodies())
		{
			Die();
		}
		return mapCoords;
	}
	
	// TODO: Die.
	private void Die()
	{
		GD.Print("Bottom limit detected body");
		// Turn all snoods to skulls.
		// Wait 1.5 seconds or so.
		// Show death menu? Back to main?
	}

	private void UpdateDictionary(int altTileIndex)
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
		LevelWon = true;
		Scores.ApplyBonuses();
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
		Snood deadSnood = InstantiateDeadSnood(cell, index);
		DisableCollisions(deadSnood);
		DropSnood(deadSnood);
	}

	private Snood InstantiateDeadSnood(Vector2I cell, int index)
	{
		Snood deadSnood = (Snood)Launcher.Snoods[index].Instantiate();
		CallDeferred(MethodName.AddChild, deadSnood);
		deadSnood.Position = Tilemap.MapToLocal(cell) + Tilemap.Position;
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

	private void CheckForHangingChunks()
	{
		List<List<Vector2I>> chunks = new();
		List<Vector2I> checkedCells = new();
		GatherChunks(chunks, checkedCells);
		CheckChunks(chunks);
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

	private void CheckChunks(List<List<Vector2I>> chunks)
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

	private IEnumerable<Vector2I> GetTouchingCells(Vector2I centerCell, bool similarCellsOnly = true, List<Vector2I> checkedCells = null)
	{
		// Workaround to have non-compile-time-constant default variable list.
		checkedCells ??= new List<Vector2I>();

		List<Vector2I> similarCells = new();
		List<Vector2I> newlyCheckedCells = CheckSurroundingCellsForMatches(centerCell, similarCellsOnly, checkedCells, similarCells);

		RecursivelyCheckMatchingCells(similarCellsOnly, checkedCells, newlyCheckedCells, similarCells);

		return similarCells;
	}

	private List<Vector2I> CheckSurroundingCellsForMatches(Vector2I centerCell, bool similarCellsOnly, List<Vector2I> checkedCells, List<Vector2I> similarCells)
	{
		List<Vector2I> newlyCheckedCells = new();
		
		List<Vector2I> surroundingCells = Tilemap.GetSurroundingCells(centerCell).ToList();
		surroundingCells.Add(centerCell);
		foreach (Vector2I surroundingCell in surroundingCells)
		{
			int surroundingCellIndex = Tilemap.GetCellAlternativeTile(surroundingCell);
			
			// Don't check blank cells.
			if (surroundingCellIndex == -1)
			{
				continue;
			}
			
			if (!checkedCells.Contains(surroundingCell))
			{
				newlyCheckedCells.Add(surroundingCell);

				int centerCellIndex = Tilemap.GetCellAlternativeTile(centerCell);
				// If similarCellsOnly is false, then any touching cells are counted, and we've already counted out the blank cells. 
				if (!similarCellsOnly || surroundingCellIndex == centerCellIndex)
				{
					similarCells.Add(surroundingCell);
				}
			}
		}
		checkedCells.AddRange(newlyCheckedCells);
		return newlyCheckedCells;
	}

	private void RecursivelyCheckMatchingCells(bool similarCellsOnly, List<Vector2I> checkedCells, List<Vector2I> newlyCheckedCells, List<Vector2I> similarCells)
	{
		List<Vector2I> cellsToAdd = new();
		foreach (Vector2I similarCell in similarCells)
		{
			if (newlyCheckedCells.Contains(similarCell))
			{
				cellsToAdd.AddRange(GetTouchingCells(similarCell, similarCellsOnly, checkedCells));
			}
		}
		similarCells.AddRange(cellsToAdd);
	}
}
