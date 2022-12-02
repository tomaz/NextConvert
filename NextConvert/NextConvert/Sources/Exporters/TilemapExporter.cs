using NextConvert.Sources.Data;
using NextConvert.Sources.Helpers;

namespace NextConvert.Sources.Exporters;

public class TilemapExporter
{
	public TilemapData? Tilemap { get; set; }
	public IndexedData? Definitions { get; set; }

	public int TransparentTileIndex { get; set; } = 0;
	public bool UseAttributeByte { get; set; } = true;
	public bool UseOptimizedOutput { get; set; } = false;

	#region Public

	public void Export(IStreamProvider streamProvider)
	{
		if (Tilemap == null) throw new InvalidDataException($"Tilemap is required, make sure property is assigned");

		using (var writer = new BinaryWriter(streamProvider.GetStream()))
		{
			if (UseOptimizedOutput)
			{
				ExportOptimized(writer, Tilemap);
			}
			else
			{
				ExportRaw(writer, Tilemap);
			}
		}
	}

	#endregion

	#region Exporting

	private void ExportRaw(BinaryWriter writer, TilemapData tilemap)
	{
		var bytesPerTile = UseAttributeByte ? 2 : 1;

		Log.Verbose($"Exporting raw data: {tilemap.Width}x{tilemap.Height} tiles, each tile {bytesPerTile} bytes");

		for (int y = 0; y < tilemap.Height; y++)
		{
			for (int x = 0; x < tilemap.Width; x++)
			{
				var tile = tilemap[x, y];

				if (UseAttributeByte)
				{
					writer.Write(Index(tile));
					writer.Write(Attributes(tile));
				}
				else
				{
					writer.Write(Index(tile));
				}
			}
		}

		Log.Verbose($"Exported, tilemap uses {tilemap.Width * tilemap.Height * bytesPerTile} bytes");
	}

	private void ExportOptimized(BinaryWriter writer, TilemapData tilemap)
	{
		Log.Verbose($"Exporting optimized data: {tilemap.Width}x{tilemap.Height} tiles");

		var bytesWritten = 0;
		var usedTilesCount = 0;

		void WriteByte(byte value)
		{
			writer.Write(value);
			bytesWritten++;
		}

		void WriteWord(ushort value)
		{
			WriteByte((byte)(value & 0x00FF));  // LSB first
			WriteByte((byte)(value >> 8));      // MSB second
		}

		void WriteTile(TilemapData.Tile tile)
		{
			if (UseAttributeByte)
			{
				WriteByte(Index(tile));
				WriteByte(Attributes(tile));
			}
			else
			{
				WriteByte(Index(tile));
			}
		}

		List<TilemapData.Tile> UsedTiles(int y)
		{
			var result = new List<TilemapData.Tile>();

			for (int x = 0; x < tilemap.Width; x++)
			{
				var tile = tilemap[x, y];

				if (tile.Index != TransparentTileIndex)
				{
					result.Add(tile);
				}
			}

			return result;
		}

		List<TilemapData.Tile> AllTiles(int y)
		{
			var result = new List<TilemapData.Tile>();

			for (int x = 0; x < tilemap.Width; x++)
			{
				result.Add(tilemap[x, y]);
			}

			return result;
		}

		for (int y = 0; y < tilemap.Height; y++)
		{
			var usedTiles = UsedTiles(y);
			if (usedTiles.Count == 0) continue;

			usedTilesCount += usedTiles.Count;

			// First write zero based row number as word.
			WriteWord((ushort)y);

			// If row is "mostly" empty, use optimized output.
			if (usedTiles.Count < 26)
			{
				// Write number of tile data that will follow as byte.
				WriteByte((byte)usedTiles.Count);

				// Write data for each used tile. Each tile uses 2 or 3 bytes (depending on whether attribute byte is used).
				foreach (var tile in usedTiles)
				{
					// First byte is X position within the row.
					WriteByte((byte)tile.X);

					// Then it's the actual tile data: 1 byte if no attributes, 2 bytes if attributes are used.
					WriteTile(tile);
				}
			}
			else
			{
				// If row is "mostly" used, write all tiles as in raw output, including unused (transparent) ones. This actually saves data.
				var tiles = AllTiles(y);

				// To indicate this is raw row data, set bit 7 of the tile count byte.
				var tilesCount = 0b10000000 | (byte)tiles.Count;
				WriteByte((byte)tilesCount);

				// Now write all tiles raw data.
				foreach (var tile in tiles)
				{
					WriteTile(tile);
				}
			}
		}

		var bytesPerTile = UseAttributeByte ? 2 : 1;
		var totalTilesCount = tilemap.Width * tilemap.Height;
		var expectedSize = totalTilesCount * bytesPerTile;
		var savedPercentage = expectedSize > 0 ? 100 - 100 * bytesWritten / expectedSize : 100;

		Log.Verbose($"{usedTilesCount} tiles used out of {totalTilesCount}");
		Log.Verbose($"Exported, tilemap uses {bytesWritten} bytes (from raw unoptimized {expectedSize} bytes, {savedPercentage}% space saved)");
	}

	#endregion

	#region Helpers

	/// <summary>
	/// Calculates tile index of the given tile.
	/// </summary>
	private byte Index(TilemapData.Tile tile)
	{
		return (byte)tile.Index;
	}

	/// <summary>
	/// Calculates binary attributes for given tile suitable for ZX Next export. Format is:
	/// 
	/// Bit
	/// 76543210
	/// --------
	/// -+--|||
	///  |  ||+--- rotated clockwise
	///  |  |+---- X mirror
	///  |  |+----- Y mirror
	///  +--------- palette offset
	/// </summary>
	private byte Attributes(TilemapData.Tile tile)
	{
		byte result = 0;

		if (Definitions != null)
		{
			var definition = Definitions.Images[tile.Index];
			if (definition.PaletteBankOffset > 0)
			{
				result |= (byte)(definition.PaletteBankOffset << 4);
			}
		}

		if (tile.RotatedClockwise)
		{
			result |= 1 << 1;
		}

		if (tile.FlippedY)
		{
			result |= 1 << 2;
		}

		if (tile.FlippedX)
		{
			result |= 1 << 3;
		}

		return result;
	}

	#endregion
}
