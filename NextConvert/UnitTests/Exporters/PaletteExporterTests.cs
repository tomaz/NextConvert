using UnitTests.Helpers;

namespace UnitTests.Exporters;

public class PaletteExporterTests
{
	[Theory]
	[InlineData(false, false, false)]
	[InlineData(false, false, true)]
	[InlineData(false, true, false)]
	[InlineData(false, true, true)]
	[InlineData(true, false, false)]
	[InlineData(true, false, true)]
	[InlineData(true, true, false)]
	[InlineData(true, true, true)]
	public void Sprites_ShouldExportPalette(bool is4BitImage, bool is9BitColour, bool isCountExported)
	{
		// setup
		var images = TestObjects.CreateSpritesImageSplitter().Split(ResourcesUtils.Sprites.SourceImage());
		var data = TestObjects.CreateSpritesPaletteMapper(is4BitImage).Map(images);
		var exporter = TestObjects.CreatePaletteExporter(data, is9BitColour, isCountExported);
		var streamProvider = new MemoryStreamProvider();

		// execute
		exporter.Export(streamProvider);

		// verify
		var expectedData = ResourcesUtils.Sprites.ExpectedPaletteData(new PaletteBuilder()
			.As4BitImage(is4BitImage)
			.As9Bit(is9BitColour)
			.CountExported(isCountExported)
			.Get());
		Assert.Equal(expectedData, streamProvider.Data);
	}

	[Theory]
	[InlineData(false, false)]
	[InlineData(false, true)]
	[InlineData(true, false)]
	[InlineData(true, true)]
	public void Tiles_ShouldExportPalette(bool is9BitColour, bool isCountExported)
	{
		// setup
		var images = TestObjects.CreateTilesImageSplitter().Split(ResourcesUtils.Tiles.SourceImage());
		var data = TestObjects.CreateTilesPaletteMapper().Map(images);
		var exporter = TestObjects.CreatePaletteExporter(data, is9BitColour, isCountExported);
		var streamProvider = new MemoryStreamProvider();

		// execute
		exporter.Export(streamProvider);

		// verify
		var expectedData = ResourcesUtils.Tiles.ExpectedPaletteData(new PaletteBuilder()
			.As9Bit(is9BitColour)
			.CountExported(isCountExported)
			.Get());
		Assert.Equal(expectedData, streamProvider.Data);
	}
}
