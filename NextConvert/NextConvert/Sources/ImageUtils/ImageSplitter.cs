using NextConvert.Sources.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Tga;
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
	private Color TransparentColor { get; set; }

	private bool KeepOriginalPositions { get; set; }

	#region Initialization & Disposal

	public ImageSplitter(int width, int height, bool keepOriginalPositions)
	{
		ItemWidth = width;
		ItemHeight = height;
		KeepOriginalPositions = keepOriginalPositions;
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
		TransparentColor = transparent;

		var images = new List<ImageData>();
		var itemsPerRow = image.Width / ItemWidth;

		image.ProcessPixelRows(accessor =>
		{
			var imageY = 0;

			// Handle all item rows.
			for (int yBase = 0; yBase < accessor.Height; yBase += ItemHeight)
			{
				// Prepare the new row of items.
				var itemsRow = CreateRowOfItemImages(image.Width, imageY);

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
							itemsRow[xBase / ItemWidth].Image.Mutate(o => o.SetPixel(color, xOffs, yOffs));
						}
					}
				}

				// Update transparent flag for all images now that we have the data
				foreach (var image in itemsRow)
				{
					image.IsTransparent = image.Image.IsTransparent(TransparentColor);
				}

				// Add all items to the resulting list.
				images.AddRange(itemsRow);
				imageY++;
			}
		});

		return RemoveTransparent(images).Select(i => i.Image).ToList();
	}

	#endregion

	#region Helpers

	private List<ImageData> CreateRowOfItemImages(int imageWidth, int imageY)
	{
		var result = new List<ImageData>();
		var itemsPerRow = imageWidth / ItemWidth;

		for (int i = 0; i < itemsPerRow; i++)
		{
			var image = new Image<Argb32>(width: ItemWidth, height: ItemHeight);

			result.Add(new ImageData
			{
				Image = image,
				X = i,
				Y = imageY,
				IsTransparent = false
			});
		}

		return result;
	}

	private List<ImageData> RemoveTransparent(List<ImageData> images)
	{
		if (KeepOriginalPositions)
		{
			// If we need to respect image positions, we only remove transparent images outside of the max X and Y (except in the last row where we always remove transparent images AFTER the last non-transparent).

			// First detext maximum used coordinates of non-transparent images.
			var maxUsedX = 0;
			var maxUsedY = 0;
			foreach (var image in images)
			{
				if (image.IsTransparent) continue;
				if (image.X > maxUsedX) maxUsedX = image.X;
				if (image.Y > maxUsedY) maxUsedY = image.Y;
			}

			// Now selectively remove all transparent images outside the max coordinates. If this yields empty array, exit.
			var filteredImages = images.Where(i => !i.IsTransparent || (i.X <= maxUsedX && i.Y <= maxUsedY)).ToList();
			if (filteredImages.Count == 0) return new();

			// Since we parse images from top to bottom and left to right, last images are always from the last used row. Therefore we can easily remove all remaining transparent images from the last row.
			while (filteredImages.Count > 0)
			{
				var image = filteredImages.Last();

				// Exit once we reach rows below last.
				if (image.Y < maxUsedY) break;

				// Exit as soon as we reach the first non-transparent image.
				if (!image.IsTransparent) break;

				// Otherwise remove the image and continue
				filteredImages.RemoveAt(filteredImages.Count - 1);
			}

			return filteredImages;
		}
		else
		{
			// If we don't have to respect image positions, we simply remove all transparent images.
			return images.Where(i => !i.IsTransparent).ToList();
		}
	}

	#endregion

	#region Declarations

	private class ImageData
	{
		public Image<Argb32> Image { get; set; }
		public int X { get; set; }
		public int Y { get; set; }
		public bool IsTransparent { get; set; }
	}

	#endregion
}

#region Extensions

internal static class ImageSplitterExtensions
{
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
