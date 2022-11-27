using NextConvert.Sources.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace NextConvert.Sources.ImageUtils;

/// <summary>
/// Splits image into smaller chunks
/// </summary>
public class ImageSplitter
{
	private int ItemWidth { get; set; }
	private int ItemHeight { get; set; }

	#region Initialization & Disposal

	public ImageSplitter(int width, int height)
	{
		ItemWidth = width;
		ItemHeight = height;
	}

	#endregion

	#region Public

	/// <summary>
	/// Returns the list of all items from the assigned image. Items are always parsed in left-to-right and top-to-bottom order. Fully transparent items are skipped.
	/// </summary>
	public List<Image<Argb32>> Images(string path, Color transparent)
	{
		using (var image = Image.Load<Argb32>(path))
		{
			return Images(image, transparent);
		}
	}

	/// <summary>
	/// Returns the list of all items from the assigned image. Items are always parsed in left-to-right and top-to-bottom order. Fully transparent items are skipped.
	/// </summary>
	public List<Image<Argb32>> Images(Image<Argb32> image, Color transparent)
	{
		var result = new List<Image<Argb32>>();
		var itemsPerRow = image.Width / ItemWidth;

		image.ProcessPixelRows(accessor =>
		{
			// Handle all item rows.
			for (int yBase = 0; yBase < accessor.Height; yBase += ItemHeight)
			{
				// Prepare the new row of items.
				var itemsRow = CreateRowOfItemImages(image.Width);

				// Handle each pixel row of the items.
				for (int yOffs = 0; yOffs < ItemHeight; yOffs++)
				{
					var pixelRow = accessor.GetRowSpan(yBase + yOffs);

					// Handle all item columns.
					for (int xBase = 0; xBase < pixelRow.Length; xBase += ItemWidth)
					{
						// Handle each pixel column of the item.
						for (int xOffs = 0; xOffs < ItemWidth; xOffs++)
						{
							var color = pixelRow[xBase + xOffs];
							itemsRow[xBase / ItemWidth].Mutate(o => o.SetPixel(color, xOffs, yOffs));
						}
					}
				}

				// Add all items to the resulting list.
				result.AddRange(itemsRow);
			}
		});

		return result.RemoveTransparent(transparent);
	}

	#endregion

	#region Helpers

	private List<Image<Argb32>> CreateRowOfItemImages(int imageWidth)
	{
		var result = new List<Image<Argb32>>();
		var itemsPerRow = imageWidth / ItemWidth;

		for (int i = 0; i < itemsPerRow; i++)
		{
			result.Add(new Image<Argb32>(width: ItemWidth, height: ItemHeight));
		}

		return result;
	}

	#endregion
}

#region Extensions

internal static class ImageSplitterExtensions
{
	/// <summary>
	/// Removes all fully transparent items from the given enumerable.
	/// </summary>
	internal static List<Image<Argb32>> RemoveTransparent(this List<Image<Argb32>> images, Color transparent)
	{
		return images.Where(image => !image.IsTransparent(transparent)).ToList();
	}

	/// <summary>
	/// Determines if the given image is fully transparent.
	/// </summary>
	internal static bool IsTransparent(this Image<Argb32> image, Color transparent)
	{
		var transparentColor = transparent.ToPixel<Argb32>();
		var result = true;

		image.ProcessPixelRows(accessor =>
		{
			for (int y = 0; y < image.Height; y++)
			{
				var row = accessor.GetRowSpan(y);

				for (int x = 0; x < row.Length; x++)
				{
					ref Argb32 pixel = ref row[x];
					if (pixel != transparentColor)
					{
						result = false;
						return;
					}
				}
			}
		});

		return result;
	}
}

#endregion
