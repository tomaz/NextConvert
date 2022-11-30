using UnitTests.Helpers;

namespace UnitTests;

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
		var images = TestUtils.CreateSpritesImageSplitter().Split(ResourcesUtils.GetSpritesSourceImage());
		var data = TestUtils.CreateSpritesPaletteMapper(is4BitImage).Map(images);
		var exporter = TestUtils.CreatePaletteExporter(data, is9BitColour, isCountExported);
		var streamProvider = new MemoryStreamProvider();

		// execute
		exporter.Export(streamProvider);

		// verify
		var expectedPalette = ResourcesUtils.GetSpriteResultsPalette(is4BitImage);
		var expectedData = TestUtils.CreateExportedPaletteData(expectedPalette, is9BitColour, isCountExported);
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
		var images = TestUtils.CreateTilesImageSplitter().Split(ResourcesUtils.GetTilesSourceImage());
		var data = TestUtils.CreateTilesPaletteMapper().Map(images);
		var exporter = TestUtils.CreatePaletteExporter(data, is9BitColour, isCountExported);
		var streamProvider = new MemoryStreamProvider();

		// execute
		exporter.Export(streamProvider);

		// verify
		var expectedPalette = ResourcesUtils.GetTileResultsPalette();
		var expectedData = TestUtils.CreateExportedPaletteData(expectedPalette, is9BitColour, isCountExported);
		Assert.Equal(expectedData, streamProvider.Data);
	}
}
