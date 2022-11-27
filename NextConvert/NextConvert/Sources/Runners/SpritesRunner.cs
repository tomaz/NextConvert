using NextConvert.Sources.Base;
using NextConvert.Sources.Exporters;
using NextConvert.Sources.Helpers;
using NextConvert.Sources.ImageUtils;

using SixLabors.ImageSharp;

namespace NextConvert.Sources.Runners;

public class SpriteRunner : BaseRunner
{
	public FileInfo? InputFilename { get; set; }
	public FileInfo? OutputSpritesFilename { get; set; }
	public FileInfo? PaletteFilename { get; set; }
	public FileInfo? SpriteSheetFilename { get; set; }
	public Color? TransparentColor { get; set; }
	public int SpritesPerRow { get; set; }
	public bool IsSprite4Bit { get; set; } = false;
	public bool IsPalette9Bit { get; set; } = true;
	public bool IsPaletteCountExported { get; set; } = true;

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
		if (SpriteSheetFilename != null) Log.Verbose($"{SpriteSheetFilename}");
		
		Log.NewLine();
		Log.Verbose("Options:");
		Log.Verbose($"Sprite type: {(IsSprite4Bit ? 4 : 8)}bit");
		Log.Verbose($"Bits per colour: {(IsPalette9Bit ? 9 : 8)}");
		Log.Verbose($"Transparent colour: {TransparentColor} (RGBA)");
		Log.Verbose($"Spritesheet columns: {SpritesPerRow}");
		
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
		var sprites = new ImageSplitter(16, 16).Images(InputFilename!.FullName, TransparentColor!.Value);
		Log.Info($"{sprites.Count} sprites detected");

		Log.Verbose("Mapping colours");
		var data = new PaletteMapper(IsSprite4Bit).Map(sprites);
		Log.Info($"{data.Palette.Count} colours mapped");

		Log.NewLine();

		if (PaletteFilename != null)
		{
			Log.Verbose("Exporting palette");

			new PaletteExporter
			{
				Data = data,
				IsPalette9Bit = IsPalette9Bit,
				IsPaletteCountExported = IsPaletteCountExported
			}
			.Export(PaletteFilename);

			Log.Info($"Exported {PaletteFilename}");
		}
	}

	#endregion
}
