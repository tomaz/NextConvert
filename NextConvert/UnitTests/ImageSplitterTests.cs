using UnitTests.Helpers;

namespace UnitTests;

public class ImageSplitterTests
{
	[Theory]
	[InlineData(false, false)]
	[InlineData(false, true)]
	[InlineData(true, false)]
	[InlineData(true, true)]
	public void Sprites_ShouldExtractAllImages(bool keepBoxedTransparents, bool ignoreCopies)
	{
		// setup
		var splitter = TestUtils.CreateImageSplitter(keepBoxedTransparents, ignoreCopies);

		// execute
		var images = splitter.Split(ResourcesUtils.GetSpritesSourceImage());

		// verify
		var expected = ResourcesUtils.GetSpriteResultImages(keepBoxedTransparents, ignoreCopies);
		Assert.Equal(expected, images, new ImageComparer());
	}
}
