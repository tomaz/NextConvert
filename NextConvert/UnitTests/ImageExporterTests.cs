using NextConvert.Sources.Helpers;

using UnitTests.Helpers;

namespace UnitTests;

public class ImageExporterTests
{
	[Theory]
	[InlineData(KeepTransparentType.All, false, false)]
	[InlineData(KeepTransparentType.All, false, true)]
	[InlineData(KeepTransparentType.All, true, false)]
	[InlineData(KeepTransparentType.All, true, true)]
	[InlineData(KeepTransparentType.None, false, false)]
	[InlineData(KeepTransparentType.None, false, true)]
	[InlineData(KeepTransparentType.None, true, false)]
	[InlineData(KeepTransparentType.None, true, true)]
	[InlineData(KeepTransparentType.Boxed, false, false)]
	[InlineData(KeepTransparentType.Boxed, false, true)]
	[InlineData(KeepTransparentType.Boxed, true, false)]
	[InlineData(KeepTransparentType.Boxed, true, true)]
	public void Sprites_ShouldWriteAllImages(KeepTransparentType keepTransparent, bool ignoreCopies, bool is4Bitimage)
	{
		// setup
		var images = TestUtils.CreateSpritesImageSplitter(keepTransparent, ignoreCopies).Split(ResourcesUtils.GetSpritesSourceImage());
		var data = TestUtils.CreateSpritesPaletteMapper(is4Bitimage).Map(images);
		var exporter = TestUtils.CreateSpriteExporter(data, is4Bitimage);
		var streamProvider = new MemoryStreamProvider();

		// execute
		exporter.Export(streamProvider);

		// verify
		var expectedData = TestUtils.CreateExportedImageData(data.Images, is4Bitimage);
		Assert.Equal(expectedData, streamProvider.Data);
	}

	[Theory]
	[InlineData(KeepTransparentType.All, false)]
	[InlineData(KeepTransparentType.All, true)]
	[InlineData(KeepTransparentType.None, false)]
	[InlineData(KeepTransparentType.None, true)]
	[InlineData(KeepTransparentType.Boxed, false)]
	[InlineData(KeepTransparentType.Boxed, true)]
	public void Tiles_ShouldWriteAllImages(KeepTransparentType keepTransparent, bool ignoreCopies)
	{
		// setup
		var images = TestUtils.CreateTilesImageSplitter(keepTransparent, ignoreCopies).Split(ResourcesUtils.GetTilesSourceImage());
		var data = TestUtils.CreateTilesPaletteMapper().Map(images);
		var exporter = TestUtils.CreateTilesExporter(data);
		var streamProvider = new MemoryStreamProvider();

		// execute
		exporter.Export(streamProvider);

		// verify
		var expectedData = TestUtils.CreateExportedImageData(data.Images, is4Bit: true);
		Assert.Equal(expectedData, streamProvider.Data);
	}
}
