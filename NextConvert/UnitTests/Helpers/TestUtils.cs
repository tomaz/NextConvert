using NextConvert.Sources.Exporters;
using NextConvert.Sources.Helpers;
using NextConvert.Sources.ImageUtils;

namespace UnitTests.Helpers;

public static class TestUtils
{
	public static ImageSplitter CreateImageSplitter(KeepTransparentType keepTransparents = KeepTransparentType.None, bool ignoreCopies = false)
	{
		return new ImageSplitter
		{
			TransparentColor = ResourcesUtils.GetSpritesTransparentColour(),
			KeepTransparents = keepTransparents,
			ItemWidth = 16,
			ItemHeight = 16,
			IgnoreCopies = ignoreCopies,
		};
	}

	public static PaletteMapper CreatePaletteMapper(bool is4Bit = false)
	{
		return new PaletteMapper
		{
			TransparentColour = ResourcesUtils.GetSpritesTransparentColour(),
			Is4BitPalette = is4Bit,
		};
	}

	public static PaletteExporter CreatePaletteExporter(IndexedData? data, bool is9Bit = false, bool isCountExported = false)
	{
		return new PaletteExporter
		{
			Data = data,
			IsPalette9Bit = is9Bit,
			IsPaletteCountExported = isCountExported,
		};
	}

	public static ImageExporter CreateSpriteExporter(IndexedData? data, bool is4Bit = false)
	{
		return new ImageExporter
		{
			Data = data,
			Is4BitColour = is4Bit,
		};
	}

	public static byte[] CreateExportedPaletteData(List<IndexedData.Colour> colours, bool is9Bit = false, bool isCountExported = false)
	{
		// Note: this method copies the functionality of the actual method that generates ouput (but it was created independently). Alternatively we could use pre-generated files in resources. However resources are harder to manage.
		var result = new List<byte>();

		if (isCountExported)
		{
			result.Add((byte)colours.Count);
		}

		foreach (var colour in colours)
		{
			if (is9Bit)
			{
				result.AddRange(colour.As9BitColour);
			}
			else
			{
				result.Add(colour.As8BitColour);
			}
		}

		return result.ToArray();
	}

	public static byte[] CreateExportedSpritesData(List<IndexedData.Image> images, bool is4Bit = false)
	{
		// Note: this method copies the functionality of the actual method that generates ouput (but it was created independently). Alternatively we could use pre-generated files in resources. However resources are harder to manage.
		var result = new List<byte>();

		foreach (var image in images)
		{
			for (int y = 0; y < image.Height; y++)
			{
				for (int x = 0; x < image.Width; x++)
				{
					result.Add(image[x, y]);
				}
			}
		}

		return result.ToArray();
	}
}
