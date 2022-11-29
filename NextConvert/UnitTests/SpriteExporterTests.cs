using NextConvert.Sources.Helpers;

using UnitTests.Helpers;

namespace UnitTests;

public class SpriteExporterTests
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
	public void Sprites_ShouldExtractAllImages(KeepTransparentType keepTransparent, bool ignoreCopies, bool is4Bitimage)
	{
		// setup
		var images = TestUtils.CreateImageSplitter(keepTransparent, ignoreCopies).Split(ResourcesUtils.GetSpritesSourceImage());
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
