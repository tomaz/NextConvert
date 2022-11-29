using NextConvert.Sources.Base;
using NextConvert.Sources.Exporters;
using NextConvert.Sources.Helpers;
using NextConvert.Sources.ImageUtils;

namespace NextConvert.Sources.Runners;

public class SpriteRunner : BaseRunner
{
	private const int SpriteWidth = 16;
	private const int SpriteHeight = 16;

	public IStreamProvider? InputStreamProvider { get; set; }
	public IStreamProvider? OutputSpritesStreamProvider { get; set; }
	public IStreamProvider? OutputPaletteStreamProvider { get; set; }

	public bool IsSprite4Bit { get; set; } = false;

	#region Overrides

	protected override void OnDescribe()
	{
		Log.Info("Converting sprites");

		Log.NewLine();
		Log.Verbose("Will parse:");
		Log.Verbose($"{InputStreamProvider}");
		
		Log.NewLine();
		Log.Verbose("Will generate:");
		if (OutputSpritesStreamProvider != null) Log.Verbose($"{OutputSpritesStreamProvider}");
		if (OutputPaletteStreamProvider != null) Log.Verbose($"{OutputPaletteStreamProvider}");
		if (Globals.SheetStreamProvider != null) Log.Verbose($"{Globals.SheetStreamProvider}");
		
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
			TransparentColor = Globals.TransparentColour,
			ItemWidth = SpriteWidth,
			ItemHeight = SpriteHeight,
			IgnoreCopies = Globals.IgnoreCopies,
			KeepBoxedTransparents = Globals.KeepBoxedTransparents,
		}
		.Images(InputStreamProvider!)
	);

	private IndexedData MapImages(List<ImageData> images) => RunTask(
		onStartMessage: "Mapping colours",
		onEndMessage: (data) => $"{data.Colours.Count} colours mapped",
		task: () => new PaletteMapper
		{
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

			ItemWidth = SpriteWidth,
			ItemHeight = SpriteHeight,
			ItemsPerRow = Globals.SheetImagesPerRow,
			ColoursPerRow = Globals.SheetColoursPerRow,

			IsPalette9Bit = Globals.Palette9Bit,
			Is4BitColour = IsSprite4Bit
		}
		.Export(Globals.SheetStreamProvider!)
	);

	#endregion
}
