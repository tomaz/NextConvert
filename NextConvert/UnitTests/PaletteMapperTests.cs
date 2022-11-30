using NextConvert.Sources.Helpers;

using UnitTests.Helpers;

namespace UnitTests;

public class PaletteMapperTests
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
	public void Sprites_ShouldMapAllColours(KeepTransparentType keepTransparent, bool ignoreCopies, bool is4BitImage)
	{
		// setup
		var images = TestUtils.CreateSpritesImageSplitter(keepTransparent, ignoreCopies).Split(ResourcesUtils.GetSpritesSourceImage());
		var mapper = TestUtils.CreateSpritesPaletteMapper(is4BitImage);

		// execute
		var data = mapper.Map(images);

		// verify
		var expected = ResourcesUtils.GetSpriteResultsPalette(is4BitImage);
		Assert.Equal(expected, data.Colours, new IndexedColourComparer());
	}

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
	public void Sprites_ShouldMapAllImages(KeepTransparentType keepTransparent, bool ignoreCopies, bool is4BitImage)
	{
		// setup
		var images = TestUtils.CreateSpritesImageSplitter(keepTransparent, ignoreCopies).Split(ResourcesUtils.GetSpritesSourceImage());
		var mapper = TestUtils.CreateSpritesPaletteMapper(is4BitImage);

		// execute
		var data = mapper.Map(images);

		// verify
		// note: we only check for counts, there's no simple way of checking all image indexes apart from reimplementing the whole colour mapping in tests or relying on yet another set of files from resources. We are covering indexes when testing `SpritesRunner` anyway).
		// note: whether sprites are 4bit/8bit doesn't affect the number of sprites produced, only changes the palette.
		var expected = ResourcesUtils.GetSpriteResultImages(keepTransparent, ignoreCopies);
		Assert.Equal(expected.Count, data.Images.Count);
	}

	[Theory]
	[InlineData(KeepTransparentType.All, false)]
	[InlineData(KeepTransparentType.All, true)]
	[InlineData(KeepTransparentType.None, false)]
	[InlineData(KeepTransparentType.None, true)]
	[InlineData(KeepTransparentType.Boxed, false)]
	[InlineData(KeepTransparentType.Boxed, true)]
	public void Tiles_ShouldMapAllImages(KeepTransparentType keepTransparent, bool ignoreCopies)
	{
		// setup
		var images = TestUtils.CreateTilesImageSplitter(keepTransparent, ignoreCopies).Split(ResourcesUtils.GetTilesSourceImage());
		var mapper = TestUtils.CreateTilesPaletteMapper();

		// execute
		var data = mapper.Map(images);

		// verify
		// note: we only check for counts, there's no simple way of checking all image indexes apart from reimplementing the whole colour mapping in tests or relying on yet another set of files from resources. We are covering indexes when testing `SpritesRunner` anyway).
		// note: whether sprites are 4bit/8bit doesn't affect the number of sprites produced, only changes the palette.
		var expected = ResourcesUtils.GetTileResultImages(keepTransparent, ignoreCopies);
		Assert.Equal(expected.Count, data.Images.Count);
	}

	[Theory]
	[InlineData(KeepTransparentType.All, false)]
	[InlineData(KeepTransparentType.All, true)]
	[InlineData(KeepTransparentType.None, false)]
	[InlineData(KeepTransparentType.None, true)]
	[InlineData(KeepTransparentType.Boxed, false)]
	[InlineData(KeepTransparentType.Boxed, true)]
	public void Tiles_ShouldMapAllColours(KeepTransparentType keepTransparent, bool ignoreCopies)
	{
		// setup
		var images = TestUtils.CreateTilesImageSplitter(keepTransparent, ignoreCopies).Split(ResourcesUtils.GetTilesSourceImage());
		var mapper = TestUtils.CreateTilesPaletteMapper();

		// execute
		var data = mapper.Map(images);

		// verify
		var expected = ResourcesUtils.GetTileResultsPalette();
		Assert.Equal(expected, data.Colours, new IndexedColourComparer());
	}
}
