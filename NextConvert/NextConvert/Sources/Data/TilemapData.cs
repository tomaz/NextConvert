using SixLabors.ImageSharp;

using System.Runtime.CompilerServices;

namespace NextConvert.Sources.Data;

/// <summary>
/// Holds the data for a tilemap.
/// </summary>
public class TilemapData
{
	/// <summary>
	/// Width of the tilemap in number of tiles.
	/// </summary>
	public int Width { get; private set; }

	/// <summary>
	/// Height of the tilemap in number of tiles.
	/// </summary>
	public int Height { get; private set; }

	private Tile[,] tiles;

	#region Initialization & Disposal

	public TilemapData(int width, int height)
	{
		Width = width;
		Height = height;
		tiles = new Tile[Height, Width];
	}

	#endregion

	#region Public

	/// <summary>
	/// Access to underlying tiles array.
	/// </summary>
	public Tile this[int x, int y]
	{
		get => tiles[y, x];
		set => tiles[y, x] = value;
	}

	#endregion

	#region Declarations

	public class Tile
	{
		/// <summary>
		/// Index of the tile definition.
		/// </summary>
		public int Index { get; set; } = 0;

		/// <summary>
		/// X position within tilemap row. Zero based.
		/// </summary>
		public int X { get; set; } = 0;

		/// <summary>
		/// Y position in the tilemap. Zero based.
		/// </summary>
		public int Y { get; set; } = 0;

		/// <summary>
		/// Specifies whether the tile is flipped horizontally.
		/// </summary>
		public bool FlippedX { get; set; } = false;

		/// <summary>
		/// Specifies whetehr the tile is flipped vertically.
		/// </summary>
		public bool FlippedY { get; set; } = false;

		/// <summary>
		/// Specifies whether the tile is rotated clockwise.
		/// </summary>
		public bool RotatedClockwise { get; set; } = false;
	}

	#endregion
}
