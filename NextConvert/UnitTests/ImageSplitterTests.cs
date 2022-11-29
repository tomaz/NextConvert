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
		var splitter = TestUtils.CreateImageSplitter(keepTransparents, ignoreCopies);

		// execute
		var images = splitter.Split(ResourcesUtils.GetSpritesSourceImage());

		// verify
		var expected = ResourcesUtils.GetSpriteResultImages(keepTransparents, ignoreCopies);
		Assert.Equal(expected, images, new ImageComparer());
	}
}
