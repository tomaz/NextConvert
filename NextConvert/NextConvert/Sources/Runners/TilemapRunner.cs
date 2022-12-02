using NextConvert.Sources.Base;
using NextConvert.Sources.Data;
using NextConvert.Sources.Exporters;
using NextConvert.Sources.Helpers;
using NextConvert.Sources.Options;
using NextConvert.Sources.Utils;

namespace NextConvert.Sources.Runners;

public class TilemapRunner : BaseRunner
{
	public TilemapOptionsBinder.TilemapOptions? Options { get; set; }

	#region Overrides

	protected override void OnDescribe()
	{
		Log.Info("Converting tilemap");

		Log.NewLine();
		Log.Verbose("Will parse:");
		if (Options?.InputTilesStreamProvider != null) Log.Verbose($"{Options?.InputTilesStreamProvider}");
		if (Options?.InputTilemapStreamProvider != null) Log.Verbose($"{Options?.InputTilemapStreamProvider}");

		Log.NewLine();
		Log.Verbose("Will generate:");
		if (Options?.OutputTilemapStreamProvider != null) Log.Verbose($"{Options?.OutputTilemapStreamProvider}");
		if (Options?.OutputTilesStreamProvider != null) Log.Verbose($"{Options?.OutputTilesStreamProvider}");
		if (Options?.OutputPaletteStreamProvider != null) Log.Verbose($"{Options?.OutputPaletteStreamProvider}");
		if (Globals.SheetStreamProvider != null) Log.Verbose($"{Globals.SheetStreamProvider}");

		Log.NewLine();
		Log.Verbose($"Transparent tile index: {Options?.TransparentTileIndex}");
		Log.Verbose($"Optimized tilemap: {Options?.UseOptimizedOutput}");
		Log.Verbose($"Bytes per tile: {(Options?.UseAttributeByte == true ? 2 : 1)}");

		Log.NewLine();
		DescribeGlobals();

		Log.NewLine();
	}

	protected override void OnValidate()
	{
		base.OnValidate();

		if (Options?.InputTilesStreamProvider == null)
		{
			Log.Warning("IMPORTANT: it's recommended to parse tile definitions (--in-tiles) at the same time to correctly map palette offsets!");
		}
	}

	protected override void OnRun()
	{
		IndexedData? tilesData = null;
		TilemapData? tilemapData = null;

		// First parse tiles; if available this is prerequisite for tilemap palette offsets.
		if (Options?.InputTilesStreamProvider != null)
		{
			var images = SplitImage();
			tilesData = MapImages(images);
		}

		// Then parse tilemap.
		if (Options?.InputTilemapStreamProvider != null)
		{
			tilemapData = ParseTilemap();
		}

		// If tilemap data was created, export it.
		if (tilemapData != null)
		{
			if (Options?.OutputTilemapStreamProvider != null)
			{
				CreateTilemapFile(tilemapData, tilesData);
			}
		}

		// If tiles data was created, export all related files.
		if (tilesData != null)
		{
			if (Options?.OutputTilesStreamProvider != null)
			{
				CreateTilesFile(tilesData);
			}

			if (Options?.OutputPaletteStreamProvider != null)
			{
				CreatePaletteFile(tilesData);
			}

			if (Globals.SheetStreamProvider != null)
			{
				CreateInfoSheet(tilesData);
			}
		}
	}

	#endregion

	#region Helpers

	private TilemapData ParseTilemap() => RunTask(
		onStartMessage: "Parsing tilemap",
		onEndMessage: (result) => "Parsed",
		task: () => new TilemapParser().Parse(Options?.InputTilemapStreamProvider!)
	);

	private void CreateTilemapFile(TilemapData tilemap, IndexedData? definitions) => RunTask(
		onStartMessage: "Exporting tilemap",
		onEndMessage: () => $"Exported {Options?.OutputTilemapStreamProvider}",
		task: () => new TilemapExporter
		{
			Tilemap = tilemap,
			Definitions = definitions,
			TransparentTileIndex = Options?.TransparentTileIndex ?? 0,
			UseAttributeByte = Options?.UseAttributeByte == true,
			UseOptimizedOutput = Options?.UseOptimizedOutput == true,
		}
		.Export(Options?.OutputTilemapStreamProvider!)
	);

	private List<ImageData> SplitImage() => RunTask(
		onStartMessage: "Parsing tiles",
		onEndMessage: (result) => $"{result.Count} tiles detected",
		task: () => new ImageSplitter
		{
			TransparentColour = Globals.TransparentColour,
			KeepTransparents = Globals.KeepTransparents,
			ItemWidth = TilesRunner.TileWidth,
			ItemHeight = TilesRunner.TileHeight,
			IgnoreCopies = Globals.IgnoreCopies,
		}
		.Images(Options?.InputTilesStreamProvider!)
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
		onEndMessage: () => $"Exported {Options?.OutputTilesStreamProvider}",
		task: () => new ImageExporter
		{
			Data = data,
			Is4BitColour = true,
		}
		.Export(Options?.OutputTilesStreamProvider!)
	);

	private void CreatePaletteFile(IndexedData data) => RunTask(
		onStartMessage: "Exporting palette",
		onEndMessage: () => $"Exported {Options?.OutputPaletteStreamProvider}",
		task: () => new PaletteExporter
		{
			Data = data,
			IsPalette9Bit = Globals.Palette9Bit,
			IsPaletteCountExported = Globals.ExportPaletteCount,
		}
		.Export(Options?.OutputPaletteStreamProvider!)
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

			ItemWidth = TilesRunner.TileWidth * 2,      // we need more space than tile itself otherwise indexes will not fit
			ItemHeight = TilesRunner.TileHeight * 2,    // same with height
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
