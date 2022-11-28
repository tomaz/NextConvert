using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace NextConvert.Sources.ImageUtils;

/// <summary>
/// Maps colours from objects and prepares palette for export.
/// </summary>
public class PaletteMapper
{
	public Argb32 TransparentColour { get; set; }
	public bool Is4BitPalette { get; set; }

	#region Public

	public IndexedData Map(IEnumerable<Image<Argb32>> images)
	{
		var result = Is4BitPalette ? Colours4Bit(images) : Colours8Bit(images);

		Validate(result);

		return result;
	}

	#endregion

	#region Parsing

	private IndexedData Colours8Bit(IEnumerable<Image<Argb32>> images)
	{
		var result = new IndexedData();

		foreach (var image in images)
		{
			var indexedImage = new IndexedData.Image(width: image.Width, height: image.Height);

			result.Images.Add(indexedImage);

			image.ProcessPixelRows(accessor =>
			{
				for (int y = 0; y < accessor.Height; y++)
				{
					var row = accessor.GetRowSpan(y);

					for (int x = 0; x < row.Length; x++)
					{
						ref Argb32 colour = ref row[x];
						var index = (byte)result.AddIfDistinct(colour, colour == TransparentColour);

						indexedImage[x, y] = index;
					}
				}
			});
		}

		return result;
	}

	private IndexedData Colours4Bit(IEnumerable<Image<Argb32>> images)
	{
		var result = new IndexedData();

		return result;
	}

	#endregion

	#region Helpers

	private static void Validate(IndexedData data)
	{
		if (data.Colours.Count > 256) throw new Exception($"Detected {data.Colours.Count} colours, only 256 allowed.");
	}

	#endregion
}
