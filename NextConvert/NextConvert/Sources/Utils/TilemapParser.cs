using NextConvert.Sources.Data;
using NextConvert.Sources.Helpers;

namespace NextConvert.Sources.Utils;

/// <summary>
/// Parses tilemap into tiles.
/// </summary>
public class TilemapParser
{
	#region Public

	/// <summary>
	/// Parses the given tilemap file and creates <see cref="TilemapData"/> that describes it.
	/// </summary>
	public TilemapData Parse(IStreamProvider streamProvider)
	{
		var result = (streamProvider.GetExtension()?.Replace(".", "")?.ToLower()) switch
		{
			"gba" => ParseGBA(streamProvider),
			"map" => ParseGBA(streamProvider),
			"stm" => ParseSimpleTileMap(streamProvider),
			"txm" => ParseTextTileMap(streamProvider),
			"txt" => ParseTextTileMap(streamProvider),
			_ => throw new InvalidDataException($"Tilemap format {streamProvider.GetExtension()} not recognized, use GBA (gba, map), Simple Tile Map (stm) or Text Tilemap (txm, txt) formats"),
		};

		Log.Verbose($"Parsed tilemap of {result.Width}x{result.Height} tiles");

		return result;
	}

	#endregion

	#region Parsing

	/// <summary>
	/// Support for GBA .map files. Supports flipped tiles (X and Y), but no rotations (well, maybe the format supports rotations, but Pro Motion NG creates a new tilemap in such case, so can't verify).
	/// 
	/// Binary format, all values little endian:
	/// 
	/// Offset	Size	Description
	/// 0		4		width (number of columns)
	/// 4		4		height (number of rows)
	/// 8+		2		[width * height] tiles, each 2 bytes
	/// 
	/// Tile format:
	/// 
	/// Offset	Size	Description
	/// 0		1		tile index
	/// 1		1		tile attributes
	/// 
	/// Tile attributes (bits):
	/// 
	/// 76543210
	///     ||
	///     |+---- 1 if flipped X
	///     +----- 1 if flipped Y
	/// 
	/// Possible combinations:
	/// 
	/// 76543210	HEX
	/// --------
	/// 00000000	00	regular tile
	/// 00000100	04	flipped X
	/// 00001000	08	flipped Y
	/// 00001100	0C	flipped X & Y
	/// </summary>
	private TilemapData ParseGBA(IStreamProvider streamProvider)
	{
		using (var reader = new BinaryReader(streamProvider.GetStream(FileMode.Open)))
		{
			var width = reader.ReadInt32();
			var height = reader.ReadInt32();

			var result = new TilemapData(width: width, height: height);

			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					var index = reader.ReadByte();

					var attributes = reader.ReadByte();
					var flippedX = (attributes & 0b00000100) > 0;
					var flippedY = (attributes & 0b00001000) > 0;

					result[x, y] = new TilemapData.Tile
					{
						X = x,
						Y = y,
						Index = index,
						FlippedX = flippedX,
						FlippedY = flippedY
					};
				}
			}

			return result;
		}
	}

	/// <summary>
	/// Support for Simple Tile Map .stm files. Supports flipped tiles (X and Y), but no rotations (well, maybe the format supports rotations, but Pro Motion NG creates a new tilemap in such case, so can't verify).
	/// 
	/// Binary format, all values little endian:
	/// 
	/// Offset	Size	Description
	/// 0		4		fixed 0x53544D50 (= ASCII "STMP")
	/// 4		2		width (number of columns)
	/// 6		2		height (number of rows)
	/// 8+		4		[width * height] tiles, each 4 bytes
	/// 
	/// Tile format:
	/// 
	/// Offset	Size	Description
	/// 0		2		tile index
	/// 2		1		if 1 flipped Y, otherwise no horizontal flip
	/// 3		1		if 1 flipped Y, otherwise no vertical flip
	/// </summary>
	private TilemapData ParseSimpleTileMap(IStreamProvider streamProvider)
	{
		using (var reader = new BinaryReader(streamProvider.GetStream()))
		{
			reader.Skip(4); // STMP

			var width = reader.ReadInt16();
			var height = reader.ReadInt16();

			var result = new TilemapData(width: width, height: height);

			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					var index = reader.ReadInt16();
					var flippedX = reader.ReadByte() == 1;
					var flippedY = reader.ReadByte() == 1;

					result[x, y] = new TilemapData.Tile
					{
						X = x,
						Y = y,
						Index = index,
						FlippedX = flippedX,
						FlippedY = flippedY
					};
				}
			}

			return result;
		}
	}

	/// <summary>
	/// Support for text based tilemaps. Doesn't support any tile attributes (flipping or rotation).
	/// 
	/// The format requires each text row to contain comma delimited decimal numbers representing tile indices. Each row is required to have exact same number of values (aka columns). Therefore the first row is used to determine the number of columns. Whitespace inside row is ignored as are empty or whitespace only rows.
	/// 
	/// Example of 2x2 tilemap:
	/// 1,2
	/// 3,4
	/// </summary>
	private TilemapData ParseTextTileMap(IStreamProvider streamProvider)
	{
		var width = 0;
		var lines = new List<List<int>>();

		using (var reader = new StreamReader(streamProvider.GetStream()))
		{
			string? line;
			while ((line = reader.ReadLine()) != null)
			{
				// Skip empty lines.
				var trimmedLine = line.Trim();
				if (trimmedLine.Length == 0) continue;

				// Convert the line to a list of numbers.
				var columns = line.ToNumbersList();

				// If this is the first line, assign width, otherwise ensure the width of subsequent lines matches.
				if (width == 0)
				{
					width = columns.Count;
				}
				else if (columns.Count != width)
				{
					throw new InvalidDataException($"Line {lines.Count + 1} has {columns.Count} columns, expected {width}");
				}

				// If all is well, add new line.
				lines.Add(columns);
			}
		}

		// Prepare tilemap data structure.
		var result = new TilemapData(width, lines.Count);

		var y = 0;
		foreach (var row in lines)
		{
			var x = 0;

			foreach (var column in row)
			{
				result[x, y] = new TilemapData.Tile
				{
					X = x,
					Y = y,
					Index = column
				};

				x++;
			}

			y++;
		}

		return result;
	}

	#endregion
}

#region Extensions

internal static class ReaderExtensions
{
	public static void Skip(this BinaryReader reader, int bytes)
	{
		for (int i = 0; i < bytes; i++)
		{
			reader.ReadByte();
		}
	}

	public static List<int> ToNumbersList(this string line)
	{
		return line
			.Split(',')
			.Select(x => int.Parse(x.Trim()))
			.ToList();
	}
}

#endregion
