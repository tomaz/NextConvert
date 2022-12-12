using NextConvert.Sources.Data;
using NextConvert.Sources.Helpers;

using SixLabors.ImageSharp.PixelFormats;

namespace NextConvert.Sources.Utils;

/// <summary>
/// Maps colours from objects and prepares palette for export.
/// </summary>
public class PaletteMapper
{
	public IStreamProvider? InputPaletteStreamProvider { get; set; }
	public Argb32 TransparentColour { get; set; }
	public bool Is4BitPalette { get; set; }

	#region Public

	public IndexedData Map(IEnumerable<ImageData> images)
	{
		// Note: custom palette support is currently preliminary, don't have enough data, just what I use for my projects, therefore it's not covered with unit tests.
		return InputPaletteStreamProvider switch
		{
			null => Is4BitPalette ? Colours4Bit(images) : Colours8Bit(images),
			_ => Is4BitPalette ? ColoursCustom4Bit(images) : ColoursCustom8Bit(images),
		};
	}

	#endregion

	#region Parsing

	private IndexedData ColoursCustom8Bit(IEnumerable<ImageData> images)
	{
		Log.Verbose("Mapping with custom palette");

		var result = new IndexedData();
		var isTransparentSet = false;

		// Read all colours from the provided palette file.
		using (var reader = new BinaryReader(InputPaletteStreamProvider!.GetStream(FileMode.Open)))
		{
			while (reader.BaseStream.Position < reader.BaseStream.Length)
			{
				// Each colour uses 3 bytes representing RGB components.
				var r = reader.ReadByte();
				var g = reader.ReadByte();
				var b = reader.ReadByte();

				var argb = new Argb32(r: r, g: g, b: b);

				var colour = new IndexedData.Colour(
					argb: argb,
					isTransparent: !isTransparentSet && argb == TransparentColour);

				// Add the colour. Note we don't care if colour is repeated.
				result.Colours.Add(colour);

				// There can only be 1 transparent colour.
				if (colour.IsTransparent) isTransparentSet = true;
			}
		}

		Log.Verbose($"Detected {result.Colours.Count} total colours");

		// Map all pixels.
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
						// For now, just find the first matching colour.
						var colour = new IndexedData.Colour(row[x]);
						var colourIndex = (byte)result.Colours.FindMatching(colour);
						indexedImage[x, y] = colourIndex;
					}
				}
			});
		}

		return result;
	}

	private IndexedData ColoursCustom4Bit(IEnumerable<ImageData> images)
	{
		// Similar to mapped 4-bit palette, this is also more complex operation composed of several steps.

		List<int> MapImageColoursToBanks(IndexedData.Image image)
		{
			var result = new List<int>();

			for (var y = 0; y < image.Height; y++)
			{
				for (var x = 0; x < image.Width; x++)
				{
					var colourIndex = image[x, y];
					var bank = colourIndex / IndexedBankData.MaxColours;

					if (!result.Contains(bank))
					{
						result.Add(bank);
					}
				}
			}

			return result;
		}

		IndexedData.Image? RemapColoursToBank(IndexedData data, IndexedData.Image image, int testBank)
		{
			// Prepare bank start index within the global palette.
			var bankStartIndex = testBank * IndexedBankData.MaxColours;

			// Prepare image copy that will hold remapped colours.
			var copy = new IndexedData.Image(image);
			var mapping = new Dictionary<byte, byte>();

			// Test all pixels and build up remapping.
			for (int y = 0; y < copy.Height; y++)
			{
				for (int x = 0; x < copy.Width; x++)
				{
					var imageColourIndex = copy[x, y];
					var foundMatch = false;

					if (!mapping.ContainsKey(imageColourIndex))
					{
						// If this is the first time we encounter given colour, see if we can find a match in given palette bank.
						for (int i = 0; i < IndexedBankData.MaxColours; i++)
						{
							// Prepare global colour index and bail out in case we don't have all bank colours (only applicable for the last bank).
							var index = i + bankStartIndex;
							if (index >= data.Colours.Count) break;

							// Prepare both colours.
							var imageColour = data.Colours[imageColourIndex];
							var bankColour = data.Colours[index];

							// If both colours are transparent, we take them as a match even though the colour itself may be different.
							if ((imageColour.IsTransparent && bankColour.IsTransparent) || imageColour.IsSameColour(bankColour))
							{
								mapping[imageColourIndex] = (byte)index;
								copy[x, y] = (byte)index;
								foundMatch = true;
								break;
							}
						}
					}
					else
					{
						// If we already hapen the colour mapped, just reuse the same value.
						copy[x, y] = mapping[imageColourIndex];
						foundMatch = true;
					}

					// If we couldn't find any match, exit.
					if (!foundMatch) return null;
				}
			}

			// Arriving here means we can all colours of the image within the given bank.
			return copy;
		}

		// First step is to mark all transparent colours. On 4-bit palette, each 16-colour bank has one colour transparent and it needs to be the same bank index as in all other banks. However there is complication that source palette doesn't use the same ARGB colour for transparent. Therefore we determine the index of the colour within the bank that should be transparent from the indicated transparent colour and then set the same colour transparent in all banks. At the same time we also compare and warn if newly marked transparent colour is different. That doesn't affect the outcome - Next hardware will still render it transparent, it's just visual as it can very easily be mistaken and used as opaque colour in image editor.
		void MapTransparentColoursForAllBanks(IndexedData data)
		{
			// For 4-bit palette, each 16-colour bank needs to have the transparent colour set. We do it by finding the detected transparent colour index within its bank and set the same index for each bank.
			var globalTransparentIndex = data.Colours.FindIndex(colour => colour.IsTransparent);
			Log.Verbose($"Found transparent colour on index {globalTransparentIndex}");

			var transparentColour = data.Colours[globalTransparentIndex];
			var bankTransparentIndex = globalTransparentIndex % IndexedBankData.MaxColours;
			for (int i = 0; i < data.Colours.Count; i += IndexedBankData.MaxColours)
			{
				var colour = data.Colours[i];

				if (!colour.IsTransparent)
				{
					colour.IsTransparent = true;
					Log.Verbose($"Making colour {i} transparent");

					// Warn in case transparent colour doesn't match; the results will be the same in runtime, but it's better to have source image look closer to what it should be during runtime to avoid potentially hard to debug issues.
					if (!transparentColour.IsSameColour(colour))
					{
						Log.Warning($"Colour {i} will be transparent but doesn't match (expected {transparentColour.AsArgb32}, actual {colour.AsArgb32})");
					}
				}
			}
		}

		// Second step is to go through all images and determine number of banks they use. If more than 1, we attempt to find better bank that can fully fit the image. If that's not possible, we'll bail out with error.
		void MapImagesToBanks(IndexedData data)
		{
			// For each image we prepare the list of banks it uses.
			for (var i = 0; i < data.Images.Count; i++)
			{
				var image = data.Images[i];

				// Get the list of all banks this image occupies. If all pixels are present within the same bank, we're done, we just need to adjust colours for the matched bank.
				var banks = MapImageColoursToBanks(image);
				if (banks.Count == 1)
				{
					image.AdjustColoursFor4BitPaletteBank(banks[0]);
					continue;
				}

				var foundMatch = false;

				// Otherwise we need to try to remap if possible.
				Log.Verbose($"Image {image} has pixels from {banks.Count} banks, attempting to remap");
				foreach (var bank in banks)
				{
					// Attempt to remap to given bank. If successful, this will return a copy of the image.
					var remappedImage = RemapColoursToBank(data, image, bank);
					if (remappedImage == null) continue;

					// If we could remap, replace original with the copy and adjust colours for the new bank.
					Log.Verbose($"Remapped colours to bank {bank}");
					data.Images.Insert(i, remappedImage);
					data.Images.RemoveAt(i + 1);

					remappedImage.AdjustColoursFor4BitPaletteBank(bank);

					foundMatch = true;

					break;
				}

				if (!foundMatch) throw new InvalidDataException($"Can't remap image {image} to fit 4-bit palette");
			}
		}

		// Initially we perform the usual 8-bit parsing.
		var result = ColoursCustom8Bit(images);
		MapTransparentColoursForAllBanks(result);
		MapImagesToBanks(result);
		return result;
	}

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

						var colourIndex = (byte)result.Colours.AddIfDistinct(
							colour: colour, 
							isTransparent: colour == TransparentColour
						);

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
