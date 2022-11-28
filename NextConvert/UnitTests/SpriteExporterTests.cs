using UnitTests.Helpers;

namespace UnitTests;

public class SpriteExporterTests
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
	public void Sprites_ShouldExtractAllImages(bool keepBoxedTransparents, bool ignoreCopies, bool is4Bitimage)
	{
		// setup
		var images = TestUtils.CreateImageSplitter(keepBoxedTransparents, ignoreCopies).Split(ResourcesUtils.GetSpritesSourceImage());
		var data = TestUtils.CreatePaletteMapper(is4Bitimage).Map(images);
		var exporter = TestUtils.CreateSpriteExporter(data);
		var streamProvider = new MemoryStreamProvider();

		// execute
		exporter.Export(streamProvider);

		// verify
		var expectedData = TestUtils.CreateExportedSpritesData(data.Images, is4Bitimage);
		Assert.Equal(expectedData, streamProvider.Data);
	}
}
