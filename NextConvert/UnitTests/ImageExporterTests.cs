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
		var images = TestObjects.CreateSpritesImageSplitter(keepTransparent, ignoreCopies).Split(ResourcesUtils.Sprites.SourceImage());
		var data = TestObjects.CreateSpritesPaletteMapper(is4Bitimage).Map(images);
		var exporter = TestObjects.CreateSpriteExporter(data, is4Bitimage);
		var streamProvider = new MemoryStreamProvider();

		// execute
		exporter.Export(streamProvider);

		// verify
		var expectedData = ResourcesUtils.Sprites.ExpectedImageData(new ImagesBuilder()
			.FromImages(data.Images)
			.As4Bit(is4Bitimage)
			.Get());
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
		var images = TestObjects.CreateTilesImageSplitter(keepTransparent, ignoreCopies).Split(ResourcesUtils.Tiles.SourceImage());
		var data = TestObjects.CreateTilesPaletteMapper().Map(images);
		var exporter = TestObjects.CreateTilesExporter(data);
		var streamProvider = new MemoryStreamProvider();

		// execute
		exporter.Export(streamProvider);

		// verify
		var expectedData = ResourcesUtils.Tiles.ExpectedImageData(new ImagesBuilder()
			.FromImages(data.Images)
			.Transparents(keepTransparent)
			.IgnoringCopies(ignoreCopies)
			.As4Bit(true)
			.Get());
		Assert.Equal(expectedData, streamProvider.Data);
	}
}
