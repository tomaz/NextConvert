using NextConvert.Sources.Helpers;

using UnitTests.Helpers;

namespace UnitTests;

public class ImageSplitterTests
{
	[Theory]
	[InlineData(KeepTransparentType.All, false)]
	[InlineData(KeepTransparentType.All, true)]
	[InlineData(KeepTransparentType.None, false)]
	[InlineData(KeepTransparentType.None, true)]
	[InlineData(KeepTransparentType.Boxed, false)]
	[InlineData(KeepTransparentType.Boxed, true)]
	public void Sprites_ShouldExtractAllImages(KeepTransparentType keepTransparents, bool ignoreCopies)
	{
		// setup
		var splitter = TestObjects.CreateSpritesImageSplitter(keepTransparents, ignoreCopies);

		// execute
		var images = splitter.Split(ResourcesUtils.Sprites.SourceImage());

		// verify
		var expected = ResourcesUtils.Sprites.ExpectedSplitImages(new ImagesBuilder()
			.Transparents(keepTransparents)
			.IgnoringCopies(ignoreCopies)
			.Get());
		Assert.Equal(expected, images, new ImageComparer());
	}

	[Theory]
	[InlineData(KeepTransparentType.All, false)]
	[InlineData(KeepTransparentType.All, true)]
	[InlineData(KeepTransparentType.None, false)]
	[InlineData(KeepTransparentType.None, true)]
	[InlineData(KeepTransparentType.Boxed, false)]
	[InlineData(KeepTransparentType.Boxed, true)]
	public void Tiles_ShouldExtractAllImages(KeepTransparentType keepTransparents, bool ignoreCopies)
	{
		// setup
		var splitter = TestObjects.CreateTilesImageSplitter(keepTransparents, ignoreCopies);

		// execute
		var images = splitter.Split(ResourcesUtils.Tiles.SourceImage());

		// verify
		var expected = ResourcesUtils.Tiles.ExpectedSplitImages(new ImagesBuilder()
			.Transparents(keepTransparents)
			.IgnoringCopies(ignoreCopies)
			.Get());
		Assert.Equal(expected, images, new ImageComparer());
	}
}
