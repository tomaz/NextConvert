using NextConvert.Sources.Base;
using NextConvert.Sources.Data;
using NextConvert.Sources.Exporters;
using NextConvert.Sources.Helpers;
using NextConvert.Sources.Utils;

namespace NextConvert.Sources.Runners;

public class SpriteRunner : BaseRunner
{
	public IStreamProvider? InputStreamProvider { get; set; }
	public IStreamProvider? InputPaletteStreamProvider { get; set; }
	public IStreamProvider? OutputSpritesStreamProvider { get; set; }
	public IStreamProvider? OutputPaletteStreamProvider { get; set; }

	public bool IsSprite4Bit { get; set; } = false;
	public int SpriteSize { get; set; } = 16;

	#region Overrides

	protected override void OnDescribe()
	{
		Log.Info("Converting sprites");

		Log.NewLine();
		Log.Verbose("Will parse:");
		Log.Verbose($"{InputStreamProvider}");
		if (InputPaletteStreamProvider != null) Log.Verbose($"{InputPaletteStreamProvider}");

		Log.NewLine();
		Log.Verbose("Will generate:");
		if (OutputSpritesStreamProvider != null) Log.Verbose($"{OutputSpritesStreamProvider}");
		if (OutputPaletteStreamProvider != null) Log.Verbose($"{OutputPaletteStreamProvider}");
		if (Globals.SheetStreamProvider != null) Log.Verbose($"{Globals.SheetStreamProvider}");
		
		Log.NewLine();
		Log.Verbose($"4-bit sprites: {IsSprite4Bit}");
		Log.Verbose($"Sprite size: {SpriteSize}x{SpriteSize}");

		Log.NewLine();
		DescribeGlobals();

		Log.NewLine();
	}

	protected override void OnValidate()
	{
		base.OnValidate();

		// At least some output should be provided in order to make sense in running anything.
		if (OutputSpritesStreamProvider == null && OutputPaletteStreamProvider == null && Globals.SheetStreamProvider == null)
		{
			throw new ArgumentException("Either output or palette file should be provided");
		}
	}

	protected override void OnRun()
	{
		var images = SplitImage();
		var data = MapImages(images);

		if (OutputSpritesStreamProvider != null)
		{
			CreateSpritesFile(data);
		}

		if (OutputPaletteStreamProvider != null)
		{
			CreatePaletteFile(data);
		}

		if (Globals.SheetStreamProvider != null)
		{
			CreateInfoSheet(data);
		}
	}

	#endregion

	#region Helpers

	private List<ImageData> SplitImage() => RunTask(
		onStartMessage: "Parsing sprites",
		onEndMessage: (result) => $"{result.Count} sprites detected",
		task: () => new ImageSplitter
		{
			TransparentColour = Globals.TransparentColour,
			ItemWidth = SpriteSize,
			ItemHeight = SpriteSize,
			IgnoreCopies = Globals.IgnoreCopies,
			KeepTransparents = Globals.KeepTransparents,
		}
		.Images(InputStreamProvider!)
	);

	private IndexedData MapImages(List<ImageData> images) => RunTask(
		onStartMessage: "Mapping colours",
		onEndMessage: (data) => $"{data.Colours.Count} colours mapped",
		task: () => new PaletteMapper
		{
			InputPaletteStreamProvider = InputPaletteStreamProvider,
			TransparentColour = Globals.TransparentColour,
			Is4BitPalette = IsSprite4Bit
		}
		.Map(images)
	);

	private void CreateSpritesFile(IndexedData data) => RunTask(
		onStartMessage: "Exporting sprites",
		onEndMessage: () => $"Exported {OutputSpritesStreamProvider}",
		task: () => new ImageExporter
		{
			Data = data,
			Is4BitColour = IsSprite4Bit,
		}
		.Export(OutputSpritesStreamProvider!)
	);

	private void CreatePaletteFile(IndexedData data) => RunTask(
		onStartMessage: "Exporting palette",
		onEndMessage: () => $"Exported {OutputPaletteStreamProvider}",
		task: () => new PaletteExporter
		{
			Data = data,
			IsPalette9Bit = Globals.Palette9Bit,
			IsPaletteCountExported = Globals.ExportPaletteCount,
		}
		.Export(OutputPaletteStreamProvider!)
	);

	private void CreateInfoSheet(IndexedData data) => RunTask(
		onStartMessage: "Exporting spritesheet",
		onEndMessage: () => $"Exported {Globals.SheetStreamProvider}",
		task: () => new SheetExporter
		{
			Data = data,

			TransparentColour = Globals.TransparentColour,
			BackgroundColour = Globals.SheetBackgroundColour!.Value,
			Scale = Globals.SheetScale,

			ItemWidth = SpriteSize >= 16 ? SpriteSize : 16,	// if size is below 16px, we run out of space for rendering indexes
			ItemHeight = SpriteSize >= 16 ? SpriteSize : 16,
			ItemsPerRow = Globals.SheetImagesPerRow,
			ColoursPerRow = Globals.SheetColoursPerRow,

			IsPalette9Bit = Globals.Palette9Bit,
			Is4BitColour = IsSprite4Bit,

			ExportIndividualImages = Globals.ExportIndividualImages,
		}
		.Export(Globals.SheetStreamProvider!)
	);

	#endregion
}
