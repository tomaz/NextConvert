using NextConvert.Sources.Data;
using NextConvert.Sources.Helpers;
using NextConvert.Sources.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace NextConvert.Sources.Utils;

/// <summary>
/// Splits image into smaller chunks
/// </summary>
public class ImageSplitter
{
	public Color TransparentColour { get; set; }
	public KeepTransparentType KeepTransparents { get; set; } = KeepTransparentType.None;

	public int ItemWidth { get; set; }
	public int ItemHeight { get; set; }

	public bool IgnoreCopies { get; set; }

	#region Public

	/// <summary>
	/// Returns the list of all items from the assigned image. Items are always parsed in left-to-right and top-to-bottom order. Fully transparent items are skipped.
	/// </summary>
	public List<ImageData> Images(IStreamProvider streamProvider)
	{
		using (var image = Image.Load<Argb32>(streamProvider.GetStream(FileMode.Open)))
		{
			return Split(image);
		}
	}

	/// <summary>
	/// Returns the list of all items from the assigned image. Items are always parsed in left-to-right and top-to-bottom order. Fully transparent items are skipped.
	/// </summary>
	public List<ImageData> Split(Image<Argb32> image)
	{
		// First split the image into images of requested size.
		var images = SplitImage(image);
		Log.Verbose($"Split image into {images.Count} objects");

		// Now remove all transparent images.
		var imagesWithoutTransparents = RemoveTransparent(images);
		switch (KeepTransparents)
		{
			case KeepTransparentType.All: Log.Verbose($"Keeping all transparents, all {imagesWithoutTransparents.Count} objects remaining"); break;
			case KeepTransparentType.None: Log.Verbose($"Removing all transparents, {imagesWithoutTransparents.Count} objects remaining"); break;
			case KeepTransparentType.Boxed: Log.Verbose($"Removing boxed transparents, {imagesWithoutTransparents.Count} objects remaining"); break;
		}

		// Remove all copies.
		var result = RemoveCopies(imagesWithoutTransparents);
		Log.Verbose(IgnoreCopies ? $"Ignoring copies, keeping all {result.Count} objects" : $"Removed copies, {result.Count} objects remaining");

		return result;
	}

	#endregion

	#region Helpers

	private List<ImageData> SplitImage(Image<Argb32> image)
	{
		var result = new List<ImageData>();
		var itemsPerRow = image.Width / ItemWidth;

		List<ImageData> CreateRowOfItemImages(int imageWidth, int imageY)
		{
			var result = new List<ImageData>();
			var itemsPerRow = imageWidth / ItemWidth;

			for (int i = 0; i < itemsPerRow; i++)
			{
				var image = new Image<Argb32>(width: ItemWidth, height: ItemHeight);

				result.Add(new ImageData
				{
					Image = image,
					Position = new Point(i, imageY),
					IsTransparent = false	// we don't yet have data copied into image so can't detect transparency
				});
			}

			return result;
		}

		image.ProcessPixelRows(accessor =>
		{
			var yPosition = 0;

			// Handle all item rows.
			for (int yBase = 0; yBase < accessor.Height; yBase += ItemHeight)
			{
				// Prepare the new row of items.
				var itemsRow = CreateRowOfItemImages(image.Width, yPosition);

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
							var colour = pixelRow[xBase + xOffs];
							itemsRow[xBase / ItemWidth].Image.Mutate(o => o.SetPixel(colour, xOffs, yOffs));
						}
					}
				}

				// Update transparent flag for all images now that we have the data for all images of the row.
				foreach (var image in itemsRow)
				{
					image.IsTransparent = image.Image.IsTransparent(TransparentColour);
				}

				// Add all items to the resulting list.
				result.AddRange(itemsRow);
				yPosition++;
			}
		});

		return result;
	}

	private List<ImageData> RemoveTransparent(List<ImageData> images)
	{
		switch (KeepTransparents)
		{
			case KeepTransparentType.None:
			{
				// Remove all transparent images.
				return images.Where(i => !i.IsTransparent).ToList();
			}

			case KeepTransparentType.All:
			{
				// Keep all transparent images.
				return images;
			}

			case KeepTransparentType.Boxed:
			{
				// Only remove transparent images outside of the max X and Y (except in the last row where we always remove transparent images AFTER the last non-transparent).

				// First detext maximum used coordinates of non-transparent images.
				var maxUsedX = 0;
				var maxUsedY = 0;
				foreach (var image in images)
				{
					if (image.IsTransparent) continue;
					if (image.Position.X > maxUsedX) maxUsedX = image.Position.X;
					if (image.Position.Y > maxUsedY) maxUsedY = image.Position.Y;
				}

				// Now selectively remove all transparent images outside the max coordinates. If this yields empty array, exit.
				var filteredImages = images.Where(i => !i.IsTransparent || (i.Position.X <= maxUsedX && i.Position.Y <= maxUsedY)).ToList();
				if (filteredImages.Count == 0) return new();

				// Since we parse images from top to bottom and left to right, last images are always from the last used row. Therefore we can easily remove all remaining transparent images from the last row.
				while (filteredImages.Count > 0)
				{
					var image = filteredImages.Last();

					// Exit once we reach rows below last.
					if (image.Position.Y < maxUsedY) break;

					// Exit as soon as we reach the first non-transparent image.
					if (!image.IsTransparent) break;

					// Otherwise remove the image and continue
					filteredImages.RemoveAt(filteredImages.Count - 1);
				}

				return filteredImages;
			}
		}

		throw new NotImplementedException($"Unknown keep transparent option {KeepTransparents}");
	}

	private List<ImageData> RemoveCopies(List<ImageData> images)
	{
		if (!IgnoreCopies) return images;

		for (int i = 0; i < images.Count; i++)
		{
			var original = images[i];

			for (int k = i + 1; k < images.Count; k++)
			{
				void Cleanup()
				{
					images.RemoveAt(k);
					k--;
				}

				var potentialCopy = images[k];

				if (potentialCopy.IsTransparent)
				{
					// We ignore fully transparent images in this phase, we should've dealt with them in previous step.
					continue;
				}

				if (potentialCopy.IsDuplicateOf(original))
				{
					Cleanup();
					continue;
				}

				if (potentialCopy.IsHorizontalMirrorOf(original))
				{
					Cleanup();
					continue;
				}

				if (potentialCopy.IsVerticalMirrorOf(original))
				{
					Cleanup();
					continue;
				}

				if (potentialCopy.Is90CWRotationOf(original))
				{
					Cleanup();
					continue;
				}

				if (potentialCopy.Is90CCWRotationOf(original))
				{
					Cleanup();
					continue;
				}

				if (potentialCopy.Is180RotationOf(original))
				{
					Cleanup();
					continue;
				}
			}
		}

		return images;
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
		var transparentColour = transparent.ToPixel<Argb32>();
		var result = true;

		image.ProcessPixelRows(accessor =>
		{
			for (int y = 0; y < image.Height; y++)
			{
				var row = accessor.GetRowSpan(y);

				for (int x = 0; x < row.Length; x++)
				{
					ref Argb32 pixel = ref row[x];
					if (pixel != transparentColour)
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
