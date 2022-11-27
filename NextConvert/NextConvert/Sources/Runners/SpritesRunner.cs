using NextConvert.Sources.Base;
using NextConvert.Sources.Exporters;
using NextConvert.Sources.Helpers;
using NextConvert.Sources.ImageUtils;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.PixelFormats;

namespace NextConvert.Sources.Runners;

public class SpriteRunner : BaseRunner
{
	private const int SpriteWidth = 16;
	private const int SpriteHeight = 16;

	public FileInfo? InputFilename { get; set; }
	public FileInfo? OutputSpritesFilename { get; set; }
	public FileInfo? PaletteFilename { get; set; }
	public bool IsSprite4Bit { get; set; } = false;		// not possible to change right now, but we use this option in several places, so keeping it as placeholder

	#region Overrides

	protected override void OnDescribe()
	{
		Log.Info("Converting sprites");

		Log.NewLine();
		Log.Verbose("Will parse:");
		Log.Verbose($"{InputFilename}");
		
		Log.NewLine();
		Log.Verbose("Will generate:");
		if (OutputSpritesFilename != null) Log.Verbose($"{OutputSpritesFilename}");
		if (PaletteFilename != null) Log.Verbose($"{PaletteFilename}");
		if (Globals.SheetFilename != null) Log.Verbose($"{Globals.SheetFilename}");
		
		Log.NewLine();
		Log.Verbose("Options:");
		Log.Verbose($"Transparent colour: {TransparentColor} (ARGB)");

		DescribeGlobals();

		Log.NewLine();
	}

	protected override void OnValidate()
	{
		base.OnValidate();

		// If both, output raw file or palette file are missing, there's no point in running anything.
		if (OutputSpritesFilename == null && PaletteFilename == null)
		{
			throw new ArgumentException("Either output or palette file should be provided");
		}

		// If transparent colour is missing and image is not png, ignore.
		if (PaletteFilename == null && InputFilename!.Extension.ToLower() != "png")
		{
			throw new ArgumentException("Transparent colour is required for non-transparent input file");
		}
	}

	protected override void OnRun()
	{
		Log.Verbose("Parsing sprites");
		var sprites = new ImageSplitter(SpriteWidth, SpriteHeight, Globals.KeepOriginalPositions).Images(InputFilename!.FullName, TransparentColor!.Value);
		Log.Info($"{sprites.Count} sprites detected");

		Log.Verbose("Mapping colours");
		var data = new PaletteMapper(IsSprite4Bit).Map(sprites, TransparentColor!.Value);
		Log.Info($"{data.Palette.Count} colours mapped");

		if (OutputSpritesFilename != null)
		{
			Log.NewLine();
			Log.Verbose("Exporting sprites");

			new ImageExporter
			{
				Data = data,
				Is4BitColour = IsSprite4Bit,
			}
			.Export(OutputSpritesFilename);

			Log.Info($"Exported {OutputSpritesFilename}");
		}

		if (PaletteFilename != null)
		{
			Log.NewLine();
			Log.Verbose("Exporting palette");

			new PaletteExporter
			{
				Data = data,
				IsPalette9Bit = Globals.Palette9Bit,
				IsPaletteCountExported = Globals.ExportPaletteCount,
			}
			.Export(PaletteFilename);

			Log.Info($"Exported {PaletteFilename}");
		}

		if (Globals.SheetFilename != null)
		{
			Log.NewLine();
			Log.Verbose("Exporting spritesheet");

			new SheetExporter
			{
				Data = data,

				BackgroundColour = Globals.SheetBackgroundColour!.Value,
				Scale = Globals.SheetScale,

				ItemWidth = SpriteWidth,
				ItemHeight = SpriteHeight,
				ItemsPerRow = Globals.SheetImagesPerRow,
				ColoursPerRow = Globals.SheetColoursPerRow,
				
				IsPalette9Bit = Globals.Palette9Bit,
				Is4BitColour = IsSprite4Bit
			}
			.Export(Globals.SheetFilename);

			Log.Info($"Exported {Globals.SheetFilename}");
		}
	}

	#endregion
}
