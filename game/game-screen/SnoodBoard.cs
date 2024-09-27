using Godot;
using System;
using System.Collections.Generic;

public partial class SnoodBoard : Node2D
{
	public event Action OnWonLevel;
	public event Action OnLost;
	public event Action<int, int> OnTilemapChanged;
	
	private const int SPRITE_SIZE = 64;
	private const float LAUNCHER_TO_TILE_MAX_DIST = 190;

	[Export(PropertyHint.Range, "7,20,0.5")]
	public float Columns { get; set; }
	[Export]
	public int BaseSnoodUseBonus { get; set; } = 10000;
	[Export]
	public int PenaltyPerSnood { get; set; } = 100;
	
	public SnoodTilemap Tilemap { get; private set; }
	
	private Launcher Launcher { get; set; }
	private StaticBody2D WallRight { get; set; }
	private Dictionary<int, int> SnoodsByIndex { get; set; } = new();
	private bool LevelWon { get; set; }
	private float LevelWonTimer { get; set; } = 1.5f;
	private bool LevelLost { get; set; }
	private float LevelLostTimer { get; set; } = 1.5f;
	private Dictionary<int, PackedScene> Snoods { get; } = new()
	{
		{ 1, GD.Load<PackedScene>("res://snoods/snood_red.tscn") },
		{ 2, GD.Load<PackedScene>("res://snoods/snood_dark_blue.tscn") },
		{ 3, GD.Load<PackedScene>("res://snoods/snood_yellow.tscn") },
		{ 4, GD.Load<PackedScene>("res://snoods/snood_green.tscn") },
		{ 5, GD.Load<PackedScene>("res://snoods/snood_purple.tscn") },
		{ 6, GD.Load<PackedScene>("res://snoods/snood_light_blue.tscn") },
		{ 7, GD.Load<PackedScene>("res://snoods/snood_gray.tscn") },
		{ 8, GD.Load<PackedScene>("res://snoods/snood_skull.tscn") },
		{ 9, GD.Load<PackedScene>("res://snoods/bad_snood_drop_wall.tscn") },
		{ 10, GD.Load<PackedScene>("res://snoods/good_snood_raise_wall.tscn") },
		{ 11, GD.Load<PackedScene>("res://snoods/bad_snood_lose_control.tscn") },
	};


	public override void _ExitTree()
	{
		base._ExitTree();

		UnsubscribeFromEvents();
	}

	public override void _Process(double delta)
	{
		if (LevelWon)
		{
			LevelWonTimer -= (float)delta;
			if (LevelWonTimer < 0)
			{
				OnWonLevel?.Invoke();
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
	
	public void SetupBoard()
	{
		GetReferences();
		SubscribeToEvents();
		SetupLevel();
	}

	private void SetupLevel()
	{
		float width = Columns * SPRITE_SIZE;
		WallRight.Position = new Vector2(width, WallRight.Position.Y);
		
		Launcher.SetupLauncher(Snoods, width);
		// Must call after Launcher.SetupLauncher(), to have Launcher.Snoods populated.
		CountSnoods();
		// Must call after CountSnoods(), to have Launcher.SnoodsInUse populated.
		Launcher.LoadSnood();
		
		Tilemap.SetupTilemap(Snoods, Columns);
	}
	
	private void CountSnoods()
	{
		foreach (Vector2I cell in Tilemap.GetUsedCells())
		{
			int altTileIndex = Tilemap.GetCellAlternativeTile(cell);
			// Ignores blank, ceiling, and skull tiles.
			if (altTileIndex > 0 && altTileIndex != 8)
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
	
	private void HandleTilemapChange(int similarSnoods, int droppedSnoods)
	{
		OnTilemapChanged?.Invoke(similarSnoods, droppedSnoods);
		CheckBottomLimit();
	}
	
	private void CheckBottomLimit()
	{
		float launcherHeight = Launcher.GlobalPosition.Y;
		Vector2I lowestCell = new Vector2I(0, -1000);
		foreach (Vector2I cell in Tilemap.GetUsedCells())
		{
			if (cell.Y > lowestCell.Y)
			{
				lowestCell = cell;
			}
		}
	
		float tileHeight = Tilemap.ToGlobal(Tilemap.MapToLocal(lowestCell)).Y;
		if (launcherHeight - tileHeight < LAUNCHER_TO_TILE_MAX_DIST)
		{
			Lose();
		}
	}

	private void WinLevel()
	{
		Launcher.Disabled = true;
		// Starts countdown in _Process()
		LevelWon = true;
	}
	
	private void Lose()
	{
		// Turn all snoods to skulls.
		// TODO: Put this in Process under "if (LevelLost)" part, to change them one by one.
		foreach (Vector2I cell in Tilemap.GetUsedCells())
		{
			int altTileIndex = Tilemap.GetCellAlternativeTile(cell);
			if (altTileIndex >= 1 && altTileIndex != 8)
			{
				Tilemap.SetCell(cell, 0, new Vector2I(0, 0), 8);
			}
		}
	
		Launcher.Disabled = true;
		// Starts countdown in _Process()
		LevelLost = true;
	}

	private void IncrementSnoodCount(int altTileIndex)
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

	private void DecrementSnoodCount(int index)
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
		
		if (SnoodsByIndex.Count == 0)
		{
			WinLevel();
		}
	}

	private void GetReferences()
	{
		Launcher = GetNode<Launcher>("%Launcher");
		WallRight = GetNode<StaticBody2D>("%WallRight");
		Tilemap = GetNode<SnoodTilemap>("%TileMapLayer");
	}

	private void SubscribeToEvents()
	{
		Tilemap.OnBoardLowered += CheckBottomLimit;
		Tilemap.OnHitLoseControlSnood += Launcher.LoseControl;
		Tilemap.OnSnoodAdded += IncrementSnoodCount;
		Tilemap.OnSnoodDeleted += DecrementSnoodCount;
		Tilemap.OnTilemapChanged += HandleTilemapChange;
		Launcher.OnSnoodHit += Tilemap.AddSnoodToBoard;
	}

	private void UnsubscribeFromEvents()
	{
		Tilemap.OnBoardLowered -= CheckBottomLimit;
		Tilemap.OnHitLoseControlSnood -= Launcher.LoseControl;
		Tilemap.OnSnoodAdded -= IncrementSnoodCount;
		Tilemap.OnSnoodDeleted -= DecrementSnoodCount;
		Tilemap.OnTilemapChanged -= HandleTilemapChange;
		Launcher.OnSnoodHit -= Tilemap.AddSnoodToBoard;
	}
}
