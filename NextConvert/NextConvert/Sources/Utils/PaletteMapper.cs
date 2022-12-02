using NextConvert.Sources.Data;
using NextConvert.Sources.Helpers;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace NextConvert.Sources.Utils;

/// <summary>
/// Maps colours from objects and prepares palette for export.
/// </summary>
public class PaletteMapper
{
	public Argb32 TransparentColour { get; set; }
	public bool Is4BitPalette { get; set; }

	#region Public

	public IndexedData Map(IEnumerable<ImageData> images)
	{
		return Is4BitPalette ? Colours4Bit(images) : Colours8Bit(images);
	}

	#endregion

	#region Parsing

	private IndexedData Colours8Bit(IEnumerable<ImageData> images)
	{
		Log.Verbose("Mapping as 8-bit palette");

		var result = new IndexedData();

		foreach (var image in images)
		{
			var indexedImage = new IndexedData.Image(image);

			result.Images.Add(indexedImage);

			image.Image.ProcessPixelRows(accessor =>
			{
				for (int y = 0; y < accessor.Height; y++)
				{
					var row = accessor.GetRowSpan(y);

					for (int x = 0; x < row.Length; x++)
					{
						ref Argb32 colour = ref row[x];
						var colourIndex = (byte)result.Colours.AddIfDistinct(colour, colour == TransparentColour);
						indexedImage[x, y] = colourIndex;
					}
				}
			});
		}

		if (result.Colours.Count > 256) throw new Exception($"Detected {result.Colours.Count} colours, only 256 allowed.");
		
		return result;
	}

	private IndexedData Colours4Bit(IEnumerable<ImageData> images)
	{
		// Preparing 4-bit palette is quite complex operation composed of several steps. In a nutshell: we need to parse inteligently attempting to fit as many objects as possible into each 16-colour bank. One of the colours in the bank is transparent and it must be the same index in each bank.

		// First step is to gather all distinct colours for each object.
		List<IndexedImageData> IndexedImages(IEnumerable<ImageData> images)
		{
			var result = new List<IndexedImageData>();

			foreach (var image in images)
			{
				var indexedImage = new IndexedImageData(image);

				result.Add(indexedImage);

				image.Image.ProcessPixelRows(accessor =>
				{
					for (int y = 0; y < accessor.Height; y++)
					{
						var row = accessor.GetRowSpan(y);

						for (int x = 0; x < row.Length; x++)
						{
							ref Argb32 colour = ref row[x];
							var colourIndex = (byte)indexedImage.Colours.AddIfDistinct(colour, colour == TransparentColour);

							indexedImage.Image[x, y] = colourIndex;
						}
					}

					if (indexedImage.Colours.Count > IndexedBankData.MaxColours)
					{
						throw new InvalidDataException($"Found object with more than {IndexedBankData.MaxColours} colours on {image}");
					}
				});
			}

			return result;
		}

		// Once we have all indexed images and the corresponding palettes, we try to find the best fit into 16-colour banks. Colours are reused as much as possible and multiple objects are inserted into the same bank as long as they can fit. But algorithm is not perfect, it may result in unused gaps or duplicated colours. Or in worse case, for very complex images with lots of objects and colours, it will outright not be able to fit all objects. In such case, palette should be exported from image editor and loaded manually into Next program, provided the image editor is able to maintain 16-colour banks. If not, then the only way is to simplify the image...
		List<IndexedBankData> MapImagesIntoBanks(List<IndexedImageData> images)
		{
			var result = new List<IndexedBankData>();

			foreach (var image in images)
			{
				IndexedBankData? bestBank = null;

				// Find the best matching bank or create a new one. "Best bank" means the bank that requires the least amount of colours added and still accomodate the object. And of course, transparent colour must match too.
				var bestBankAddedColours = int.MaxValue;
				foreach (var existingBank in result)
				{
					var existingBankAddedColours = existingBank.TryAddImage(image, TransparentColour);
					if (existingBankAddedColours != null && existingBankAddedColours < bestBankAddedColours)
					{
						bestBank = existingBank;
						bestBankAddedColours = existingBankAddedColours!.Value;
					}

					// If we found perfect match (all colours can be reused), no need to search further.
					if (bestBankAddedColours == 0) break;
				}

				// If we didn't find matching bank, create a new one.
				if (bestBank == null)
				{
					bestBank = new IndexedBankData();
					result.Add(bestBank);
				}

				// Add the image to the bank.
				bestBank.AddImage(image, TransparentColour);

				// Adjust image's bank offset so we can correctly render it.
				image.Image.PaletteBankOffset = result.IndexOf(bestBank);
			}

			return result;
		}

		// If we were able to determine banks, combine them into single uniform palette. The main thing here is to ensure "missing" colours at the end of the bank are filled in.
		IndexedData MapBanksIntoPalette(List<IndexedBankData> banks)
		{
			var result = new IndexedData();

			foreach (var bank in banks)
			{
				// Add all images.
				result.Images.AddRange(bank.Images.Select(x => x.Image));

				// Add all colours
				result.Colours.AddRange(bank.Colours);

				// Make sure to fill in remaining colours so that each bank is full. It doesn't matter which colour we add since images are already properly mapped, so we simply add transparent. However note that we mark it as non-transparent - that's only used when generating info sheet image; all colours marked as transparent will be have a check mark over them.
				while (result.Colours.Count % IndexedBankData.MaxColours != 0)
				{
					result.Colours.Add(new IndexedData.Colour(TransparentColour, isTransparent: false, isUsed: false));
				}
			}

			return result;
		}

		// Generate the palette.
		Log.Verbose($"Mapping {images.Count()} images as 4-bit palette");
		var indexedImages = IndexedImages(images);
		Log.Verbose($"Detected {indexedImages.Sum(x => x.Colours.Count)} total colours");
		var indexedBanks = MapImagesIntoBanks(indexedImages);
		var usedColoursCount = indexedBanks.Sum(x => x.Colours.Count);
		Log.Verbose($"Merged into {indexedBanks.Count} colour banks with {usedColoursCount} distinct colours");
		var result = MapBanksIntoPalette(indexedBanks);
		var extraColoursCount = result.Colours.Count - usedColoursCount;
		Log.Verbose($"Filled in {extraColoursCount} unused colours: {indexedBanks.Count} banks * {IndexedData.MaxColoursPerBank} colours = {result.Colours.Count} total ({usedColoursCount} used)");
		return result;
	}

	#endregion

	#region Declarations

	private class IndexedImageData
	{
		public List<IndexedData.Colour> Colours { get; }

		public IndexedData.Image Image { get; }

		public IndexedImageData(ImageData image)
		{
			Colours = new();
			Image = new IndexedData.Image(image);
		}

		public IndexedImageData(IndexedImageData copy)
		{
			Colours = new(copy.Colours);
			Image = new IndexedData.Image(copy.Image);
		}

		public override string ToString() => $"{Image}, {Colours.Count} colours";
	}

	private class IndexedBankData
	{
		public const int MaxBanks = 16;
		public const int MaxColours = IndexedData.MaxColoursPerBank;

		public List<IndexedData.Colour> Colours { get; }

		public List<IndexedImageData> Images { get; }

		public IndexedBankData()
		{
			Colours = new();
			Images = new();
		}

		public override string ToString() => $"{Colours.Count} colours, {Images.Count} images";

		/// <summary>
		/// Attempts to add the given image and returns the number of colours that would be added if image was actually added to the bank. Also ensures that transparent colour matches with the image. Returns null if adding is not possible.
		/// </summary>
		public int? TryAddImage(IndexedImageData image, Argb32 transparentColour)
		{
			// If we don't yet have any colour, we should be able to add the image.
			if (Colours.Count == 0) return image.Colours.Count;

			// Otherwise create a copy of the bank and add the image to it.
			var bankCopy = new IndexedBankData();
			bankCopy.Colours.AddRange(Colours);
			bankCopy.Images.AddRange(Images);

			// Create a copy of the image; we'll update colour indexes when adding, so we should not work on original.
			var imageCopy = new IndexedImageData(image);

			// Attempt to add the image to the bank; catch exception as that means we can't fit it.
			try
			{
				return bankCopy.AddImage(imageCopy, transparentColour);
			}
			catch
			{
				return null;
			}
		}

		/// <summary>
		/// Adds image and all of its distinct colours. Also re-maps the image to reuse pre-existing colours. Returns the number of newly added colours.
		/// </summary>
		/// <remarks>
		/// Note: this will throw exception if total number of colours will exceed maximum after adding the new colours from the image. It's expected outside code will ensure image can fit before adding it!
		/// </remarks>
		public int AddImage(IndexedImageData image, Argb32 transparentColour)
		{
			var originalColoursCount = Colours.Count;

			// Add the image.
			Images.Add(image);

			// Reuse or add all image colours.
			var transparentColourIndex = -1;
			for (int i = 0; i < image.Colours.Count; i++)
			{
				var imageColour = image.Colours[i];

				// Try to reuse existing colour, or add it as a new one if no match was found so far.
				var newIndex = Colours.AddIfDistinct(imageColour);

				// Remember transparent colour index for later processing.
				if (imageColour.IsTransparent) transparentColourIndex = newIndex;

				// Either way, we need to remap the image so it now points to the new index.
				image.Image.RemapColour((byte)i, (byte)newIndex);
			}

			// If this is the first image and it's fully opaque, insert transparent colour to the start of the palette. We want to do this before testing for maximum colours so we can catch this. Alternatively, if we have transparent colour but it's not at the start of the bank, move it there.
			if (transparentColourIndex < 0 && originalColoursCount == 0)
			{
				InsertTransparentColourToStart(transparentColour);
			}
			else if (transparentColourIndex > 0)
			{
				MoveTransparentColourToStart(transparentColourIndex);
			}

			// If the bank colours grew beyond maximum count, throw exception.
			if (Colours.Count > MaxColours) throw new InvalidDataException($"Colour bank has more than {MaxColours} colours after adding image from {image}");

			return Colours.Count - originalColoursCount;
		}

		private bool InsertTransparentColourToStart(Argb32 transparentColour)
		{
			// If we already have transparent colour at the start, we're done.
			if (Colours.Count > 0 && Colours[0].IsTransparent) return false;

			// Since we'll insert a colour to index 0, we need to remap all existing colours by one. We need to map this backwards to avoid filling in previous colours.
			for (int i = Colours.Count; i > 0; i--)
			{
				RemapImagesColours(i - 1, i);
			}

			// We always want to keep transparent colour at the first index of the bank.
			Colours.Insert(0, new IndexedData.Colour(transparentColour, true));

			return true;
		}

		private void MoveTransparentColourToStart(int transparentColourIndex)
		{
			// Move the colour.
			var imageTransparentColour = Colours[transparentColourIndex];
			Colours.RemoveAt(transparentColourIndex);
			Colours.Insert(0, imageTransparentColour);

			// Remap images. Since we add one by one, all images should already point to the same transparent colour index.
			RemapImagesColours((byte)transparentColourIndex, 0);
		}

		private void RemapImagesColours(int source, int destination)
		{
			foreach (var bankImage in Images)
			{
				bankImage.Image.RemapColour((byte)source, (byte)destination);
			}
		}
	}

	#endregion
}
