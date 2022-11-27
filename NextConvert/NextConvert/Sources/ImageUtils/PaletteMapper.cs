using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace NextConvert.Sources.ImageUtils;

/// <summary>
/// Maps colours from objects and prepares palette for export.
/// </summary>
public class PaletteMapper
{
	private Argb32 TransparentColour { get; set; }
	private bool IsImage4Bit { get; set; }

	#region Initialization & Disposal

	public PaletteMapper(bool is4Bit)
	{
		IsImage4Bit = is4Bit;
	}

	#endregion

	#region Public

	public IndexedData Map(IEnumerable<Image<Argb32>> images, Argb32 transparentColour)
	{
		TransparentColour = transparentColour;

		var result = IsImage4Bit ? Colours4Bit(images) : Colours8Bit(images);

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
			var indexedImage = new IndexedImage(width: image.Width, height: image.Height);

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
		if (data.Palette.Count > 256) throw new Exception($"Detected {data.Palette.Count} colours, only 256 allowed.");
	}

	#endregion
}
