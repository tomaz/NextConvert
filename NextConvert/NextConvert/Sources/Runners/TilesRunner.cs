using NextConvert.Sources.Base;
using NextConvert.Sources.Data;
using NextConvert.Sources.Exporters;
using NextConvert.Sources.Helpers;
using NextConvert.Sources.Utils;

namespace NextConvert.Sources.Runners;

public class TilesRunner : BaseRunner
{
	public const int TileWidth = 8;
	public const int TileHeight = 8;

	public IStreamProvider? InputStreamProvider { get; set; }
	public IStreamProvider? OutputTilesStreamProvider { get; set; }
	public IStreamProvider? OutputPaletteStreamProvider { get; set; }

	#region Overrides

	protected override void OnDescribe()
	{
		Log.Info("Converting tiles");

		Log.NewLine();
		Log.Verbose("Will parse:");
		Log.Verbose($"{InputStreamProvider}");

		Log.NewLine();
		Log.Verbose("Will generate:");
		if (OutputTilesStreamProvider != null) Log.Verbose($"{OutputTilesStreamProvider}");
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
		if (OutputTilesStreamProvider == null && OutputPaletteStreamProvider == null && Globals.SheetStreamProvider == null)
		{
			throw new ArgumentException("Either output or palette file should be provided");
		}
	}

	protected override void OnRun()
	{
		var images = SplitImage();
		var data = MapImages(images);

		if (OutputTilesStreamProvider != null)
		{
			CreateTilesFile(data);
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
		onStartMessage: "Parsing tiles",
		onEndMessage: (result) => $"{result.Count} tiles detected",
		task: () => new ImageSplitter
		{
			TransparentColour = Globals.TransparentColour,
			KeepTransparents = Globals.KeepTransparents,
			ItemWidth = TileWidth,
			ItemHeight = TileHeight,
			IgnoreCopies = Globals.IgnoreCopies,
		}
		.Images(InputStreamProvider!)
	);

	private IndexedData MapImages(List<ImageData> images) => RunTask(
		onStartMessage: "Mapping colours",
		onEndMessage: (data) => $"{data.Colours.Count} colours mapped",
		task: () => new PaletteMapper
		{
			TransparentColour = Globals.TransparentColour,
			Is4BitPalette = true
		}
		.Map(images)
	);

	private void CreateTilesFile(IndexedData data) => RunTask(
		onStartMessage: "Exporting sprites",
		onEndMessage: () => $"Exported {OutputTilesStreamProvider}",
		task: () => new ImageExporter
		{
			Data = data,
			Is4BitColour = true,
		}
		.Export(OutputTilesStreamProvider!)
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
		onStartMessage: "Exporting tile-sheet",
		onEndMessage: () => $"Exported {Globals.SheetStreamProvider}",
		task: () => new SheetExporter
		{
			Data = data,

			TransparentColour = Globals.TransparentColour,
			BackgroundColour = Globals.SheetBackgroundColour!.Value,
			Scale = Globals.SheetScale,

			ItemWidth = TileWidth * 2,		// we need more space than tile itself otherwise indexes will not fit
			ItemHeight = TileHeight * 2,	// same with height
			ItemsPerRow = Globals.SheetImagesPerRow,
			ColoursPerRow = Globals.SheetColoursPerRow,

			IsPalette9Bit = Globals.Palette9Bit,
			Is4BitColour = true,

			ExportIndividualImages = Globals.ExportIndividualImages,
		}
		.Export(Globals.SheetStreamProvider!)
	);

	#endregion
}
