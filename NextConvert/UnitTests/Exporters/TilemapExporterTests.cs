using UnitTests.Helpers;

namespace UnitTests.Exporters;

public class TilemapExporterTests
{
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void ShouldExportDataWithoutDefinitions(bool useAttributeByte)
	{
		// setup
		var tilemap = TestObjects.CreateTilemapParser().Parse(ResourcesUtils.Tilemap.SourceTilemapStream());
		var exporter = TestObjects.CreateTilemapExporter(tilemap: tilemap, useAttribute: useAttributeByte);
		var stream = new MemoryStreamProvider();

		// execute
		exporter.Export(stream);

		// verify
		var expectedData = ResourcesUtils.Tilemap.ExpectedNextTilemapData(useAttribute: useAttributeByte);
		Assert.Equal(expectedData, stream.Data);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void ShouldExportDataWithDefinitions(bool useAttributeByte)
	{
		// setup
		var images = TestObjects.CreateTilesImageSplitter(ignoreCopies: true).Split(ResourcesUtils.Tiles.SourceImage());
		var definitions = TestObjects.CreateTilesPaletteMapper().Map(images);
		var tilemap = TestObjects.CreateTilemapParser().Parse(ResourcesUtils.Tilemap.SourceTilemapStream());
		var exporter = TestObjects.CreateTilemapExporter(tilemap: tilemap, definitions: definitions, useAttribute: useAttributeByte);
		var stream = new MemoryStreamProvider();

		// execute
		exporter.Export(stream);

		// verify
		var expectedData = ResourcesUtils.Tilemap.ExpectedNextTilemapData(definitions: definitions, useAttribute: useAttributeByte);
		Assert.Equal(expectedData, stream.Data);
	}
}
