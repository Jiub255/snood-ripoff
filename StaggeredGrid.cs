using System;
using System.Collections.Generic;
using Godot;

public partial class StaggeredGrid : Node2D
{
	private const int TILE_SIZE_PX = 64;
	
	private List<List<Vector2>> TilePositions { get; set; } = new();
	//private List<List<Tile>> Tilemap { get; set; } = new();
	
	public void SetupTilemap(int rows, int columns)
	{
		for (int rowIndex = 0; rowIndex < rows; rowIndex++)
		{
			List<Vector2> rowTiles = new();
			for (int columnIndex = 0; columnIndex < columns; columnIndex++)
			{
				float xOffset = (rowIndex % 2) * (TILE_SIZE_PX / 2);
				Vector2 position = new(columnIndex * TILE_SIZE_PX + xOffset, rowIndex * TILE_SIZE_PX);
				rowTiles.Add(position);
			}
			TilePositions.Add(rowTiles);
		}
	}
	
	private void GetNearestTilePosition(Vector2 position)
	{
		
	}
	
/* 	public void SetupTilemap(int rows, int columns)
	{
		for (int rowIndex = 0; rowIndex < rows; rowIndex++)
		{
			List<Tile> rowTiles = new();
			for (int columnIndex = 0; columnIndex < columns; columnIndex++)
			{
				float xOffset = (rowIndex % 2) * (TILE_SIZE_PX / 2);
				Vector2 position = new(columnIndex * TILE_SIZE_PX + xOffset, rowIndex * TILE_SIZE_PX);
				Tile tile = new(null, position);
				rowTiles.Add(tile);
			}
			Tilemap.Add(rowTiles);
		}
	}
	
	private class Tile
	{
		public Snood Snood { get; set; }
		public Vector2 Position { get; set; }
		
		public Tile(Snood snood, Vector2 position)
		{
			Snood = snood;
			Position = position;
		}
	} */
}
