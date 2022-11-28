using NextConvert.Sources.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using System.Runtime.InteropServices;

namespace NextConvert.Sources.ImageUtils;

/// <summary>
/// Splits image into smaller chunks
/// </summary>
public class ImageSplitter
{
	public Color TransparentColor { get; set; }

	public int ItemWidth { get; set; }
	public int ItemHeight { get; set; }

	public bool IgnoreCopies { get; set; }
	public bool KeepBoxedTransparents { get; set; }

	#region Public

	/// <summary>
	/// Returns the list of all items from the assigned image. Items are always parsed in left-to-right and top-to-bottom order. Fully transparent items are skipped.
	/// </summary>
	public List<Image<Argb32>> Images(IStreamProvider streamProvider)
	{
		using (var image = Image.Load<Argb32>(streamProvider.GetStream(FileMode.Open)))
		{
			return Split(image);
		}
	}

	/// <summary>
	/// Returns the list of all items from the assigned image. Items are always parsed in left-to-right and top-to-bottom order. Fully transparent items are skipped.
	/// </summary>
	public List<Image<Argb32>> Split(Image<Argb32> image)
	{
		// First split the image into images of requested size.
		var images = SplitImage(image);

		// Now remove all transparent images.
		var imagesWithoutTransparents = RemoveTransparent(images);

		// Remove all copies. Note this might result in extra transparents that might 
		var imagesWithoutCopies = RemoveCopies(imagesWithoutTransparents);

		// Returns just the images.
		return imagesWithoutCopies.Select(i => i.Image).ToList();
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
					X = i,
					Y = imageY,
					IsTransparent = false	// we don't yet have data copied into image so can't detect transparency
				});
			}

			return result;
		}

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

				// Update transparent flag for all images now that we have the data for all images of the row.
				foreach (var image in itemsRow)
				{
					image.IsTransparent = image.Image.IsTransparent(TransparentColor);
				}

				// Add all items to the resulting list.
				result.AddRange(itemsRow);
				imageY++;
			}
		});

		return result;
	}

	private List<ImageData> RemoveTransparent(List<ImageData> images)
	{
		if (KeepBoxedTransparents)
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

	#region Declarations

	private class ImageData
	{
#pragma warning disable CS8618	// image is always assigned, #pragma just prevents unnecessary warning without having to create a constructor
		public Image<Argb32> Image { get; set; }
#pragma warning restore CS8618

		public int X { get; set; }
		public int Y { get; set; }
		public bool IsTransparent { get; set; }

		public override string ToString() => $"({X},{Y}) {(IsTransparent ? "T" : "")}";

		public bool IsDuplicateOf(ImageData other)
		{
			for (int y = 0; y < Image.Height; y++)
			{
				for (int x = 0; x < Image.Width; x++)
				{
					if (Image[x, y] != other.Image[x, y]) return false;
				}
			}

			return true;
		}

		public bool IsHorizontalMirrorOf(ImageData other)
		{
			/* +---+---+---+    +---+---+---+
			 * | 1 | 2 | 3 |    | 3 | 2 | 1 |
			 * +---+---+---+    +---+---+---+
			 * | 4 | 5 | 6 | => | 6 | 5 | 4 |
			 * +---+---+---+    +---+---+---+
			 * | 7 | 8 | 9 |    | 9 | 8 | 7 |
			 * +---+---+---+    +---+---+---+
			 */
			var lastX = Image.Width - 1;

			for (int y = 0; y < Image.Height; y++)
			{
				for (int x = 0; x < Image.Width; x++)
				{
					if (Image[x, y] != other.Image[lastX - x, y]) return false;
				}
			}

			return true;
		}

		public bool IsVerticalMirrorOf(ImageData other)
		{
			/* +---+---+---+    +---+---+---+
			 * | 1 | 2 | 3 |    | 7 | 8 | 9 |
			 * +---+---+---+    +---+---+---+
			 * | 4 | 5 | 6 | => | 4 | 5 | 6 |
			 * +---+---+---+    +---+---+---+
			 * | 7 | 8 | 9 |    | 1 | 2 | 3 |
			 * +---+---+---+    +---+---+---+
			 */
			var lastY = Image.Height - 1;

			for (int y = 0; y < Image.Height; y++)
			{
				for (int x = 0; x < Image.Width; x++)
				{
					if (Image[x, y] != other.Image[x, lastY - y]) return false;
				}
			}

			return true;
		}

		public bool Is90CWRotationOf(ImageData other)
		{
			/* +---+---+---+    +---+---+---+
			 * | 1 | 2 | 3 |    | 7 | 4 | 1 |
			 * +---+---+---+    +---+---+---+
			 * | 4 | 5 | 6 | => | 8 | 5 | 2 |
			 * +---+---+---+    +---+---+---+
			 * | 7 | 8 | 9 |    | 9 | 6 | 3 |
			 * +---+---+---+    +---+---+---+
			 */
			var lastY = Image.Height - 1;

			for (int y = 0; y < Image.Height; y++)
			{
				for (int x = 0; x < Image.Width; x++)
				{
					if (Image[x, y] != other.Image[lastY - y, x]) return false;
				}
			}

			return true;
		}

		public bool Is90CCWRotationOf(ImageData other)
		{
			/* +---+---+---+    +---+---+---+
			 * | 1 | 2 | 3 |    | 3 | 6 | 9 |
			 * +---+---+---+    +---+---+---+
			 * | 4 | 5 | 6 | => | 2 | 5 | 8 |
			 * +---+---+---+    +---+---+---+
			 * | 7 | 8 | 9 |    | 1 | 4 | 7 |
			 * +---+---+---+    +---+---+---+
			 */
			var lastX = Image.Width - 1;

			for (int y = 0; y < Image.Height; y++)
			{
				for (int x = 0; x < Image.Width; x++)
				{
					if (Image[x, y] != other.Image[y, lastX - x]) return false;
				}
			}

			return true;
		}

		public bool Is180RotationOf(ImageData other)
		{
			/* +---+---+---+    +---+---+---+
			 * | 1 | 2 | 3 |    | 9 | 8 | 7 |
			 * +---+---+---+    +---+---+---+
			 * | 4 | 5 | 6 | => | 6 | 5 | 4 |
			 * +---+---+---+    +---+---+---+
			 * | 7 | 8 | 9 |    | 3 | 2 | 1 |
			 * +---+---+---+    +---+---+---+
			 */
			var lastX = Image.Width - 1;
			var lastY = Image.Height - 1;

			for (int y = 0; y < Image.Height; y++)
			{
				for (int x = 0; x < Image.Width; x++)
				{
					if (Image[x, y] != other.Image[lastX - x, lastY - y]) return false;
				}
			}

			return true;
		}
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
