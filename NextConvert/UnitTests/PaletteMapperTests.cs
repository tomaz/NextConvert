using UnitTests.Helpers;

namespace UnitTests;

public class PaletteMapperTests
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
	public void Sprites_ShouldMapAllColours(bool keepBoxedTransparents, bool ignoreCopies, bool is4BitImage)
	{
		// setup
		var images = TestUtils.CreateImageSplitter(keepBoxedTransparents, ignoreCopies).Split(ResourcesUtils.GetSpritesSourceImage());
		var mapper = TestUtils.CreatePaletteMapper(is4BitImage);

		// execute
		var data = mapper.Map(images);

		// verify
		var expected = ResourcesUtils.GetSpriteResultsPalette(is4BitImage);
		Assert.Equal(expected, data.Colours, new IndexedColourComparer());
	}

	[Theory]
	[InlineData(false, false, false)]
	[InlineData(false, false, true)]
	[InlineData(false, true, false)]
	[InlineData(false, true, true)]
	[InlineData(true, false, false)]
	[InlineData(true, false, true)]
	[InlineData(true, true, false)]
	[InlineData(true, true, true)]
	public void Sprites_ShouldMapAllImages(bool keepBoxedTransparents, bool ignoreCopies, bool is4BitImage)
	{
		// setup
		var images = TestUtils.CreateImageSplitter(keepBoxedTransparents, ignoreCopies).Split(ResourcesUtils.GetSpritesSourceImage());
		var mapper = TestUtils.CreatePaletteMapper(is4BitImage);

		// execute
		var data = mapper.Map(images);

		// verify
		// note: we only check for counts, there's no simple way of checking all image indexes apart from reimplementing the whole colour mapping in tests or relying on yet another set of files from resources. We are covering indexes when testing `SpritesRunner` anyway).
		// note: whether sprites are 4bit/8bit doesn't affect the number of sprites produced, only changes the palette.
		var expected = ResourcesUtils.GetSpriteResultImages(keepBoxedTransparents, ignoreCopies);
		Assert.Equal(expected.Count, data.Images.Count);
	}
}
