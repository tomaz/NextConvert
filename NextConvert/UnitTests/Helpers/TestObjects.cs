using NextConvert.Sources.Data;
using NextConvert.Sources.Exporters;
using NextConvert.Sources.Options;
using NextConvert.Sources.Utils;

namespace UnitTests.Helpers;

public static class TestObjects
{
	#region Sprites

	public static ImageSplitter CreateSpritesImageSplitter(KeepTransparentType keepTransparents = KeepTransparentType.None, bool ignoreCopies = false)
	{
		return new ImageSplitter
		{
			TransparentColour = ResourcesUtils.Sprites.TransparentColour(),
			KeepTransparents = keepTransparents,
			ItemWidth = 16,
			ItemHeight = 16,
			IgnoreCopies = ignoreCopies,
		};
	}

	public static PaletteMapper CreateSpritesPaletteMapper(bool is4Bit = false)
	{
		return new PaletteMapper
		{
			TransparentColour = ResourcesUtils.Sprites.TransparentColour(),
			Is4BitPalette = is4Bit,
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

	#endregion

	#region Tiles

	public static ImageSplitter CreateTilesImageSplitter(KeepTransparentType keepTransparents = KeepTransparentType.None, bool ignoreCopies = false)
	{
		return new ImageSplitter
		{
			TransparentColour = ResourcesUtils.Tiles.TransparentColour(),
			KeepTransparents = keepTransparents,
			ItemWidth = 8,
			ItemHeight = 8,
			IgnoreCopies = ignoreCopies,
		};
	}

	public static PaletteMapper CreateTilesPaletteMapper()
	{
		return new PaletteMapper
		{
			TransparentColour = ResourcesUtils.Tiles.TransparentColour(),
			Is4BitPalette = true,
		};
	}

	public static ImageExporter CreateTilesExporter(IndexedData? data)
	{
		return new ImageExporter
		{
			Data = data,
			Is4BitColour = true,
		};
	}

	#endregion

	#region General

	public static PaletteExporter CreatePaletteExporter(IndexedData? data, bool is9Bit = false, bool isCountExported = false)
	{
		return new PaletteExporter
		{
			Data = data,
			IsPalette9Bit = is9Bit,
			IsPaletteCountExported = isCountExported,
		};
	}

	#endregion
}
