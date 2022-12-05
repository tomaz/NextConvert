using NextConvert.Sources.Options;
using UnitTests.Helpers;

namespace UnitTests.Parsers;

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
		var images = TestObjects.CreateSpritesImageSplitter(keepTransparent, ignoreCopies).Split(ResourcesUtils.Sprites.SourceImage());
		var mapper = TestObjects.CreateSpritesPaletteMapper(is4BitImage);

		// execute
		var data = mapper.Map(images);

		// verify
		var expected = ResourcesUtils.Sprites.ExpectedPaletteColours(new PaletteBuilder()
			.As4BitImage(is4BitImage)
			.Get());
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
		var images = TestObjects.CreateSpritesImageSplitter(keepTransparent, ignoreCopies).Split(ResourcesUtils.Sprites.SourceImage());
		var mapper = TestObjects.CreateSpritesPaletteMapper(is4BitImage);

		// execute
		var data = mapper.Map(images);

		// verify
		// note: we only check for counts, there's no simple way of checking all image indexes apart from reimplementing the whole colour mapping in tests or relying on yet another set of files from resources. We are covering indexes when testing `SpritesRunner` anyway).
		// note: whether sprites are 4bit/8bit doesn't affect the number of sprites produced, only changes the palette.
		var expected = ResourcesUtils.Sprites.ExpectedSplitImages(new ImagesBuilder()
			.Transparents(keepTransparent)
			.IgnoringCopies(ignoreCopies)
			.Get());
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
		var images = TestObjects.CreateTilesImageSplitter(keepTransparent, ignoreCopies).Split(ResourcesUtils.Tiles.SourceImage());
		var mapper = TestObjects.CreateTilesPaletteMapper();

		// execute
		var data = mapper.Map(images);

		// verify
		var expected = ResourcesUtils.Tiles.ExpectedPaletteColours(new PaletteBuilder().Get());
		Assert.Equal(expected, data.Colours, new IndexedColourComparer());
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
		var images = TestObjects.CreateTilesImageSplitter(keepTransparent, ignoreCopies).Split(ResourcesUtils.Tiles.SourceImage());
		var mapper = TestObjects.CreateTilesPaletteMapper();

		// execute
		var data = mapper.Map(images);

		// verify
		// note: we only check for counts, there's no simple way of checking all image indexes apart from reimplementing the whole colour mapping in tests or relying on yet another set of files from resources. We are covering indexes when testing `SpritesRunner` anyway).
		// note: whether sprites are 4bit/8bit doesn't affect the number of sprites produced, only changes the palette.
		var expected = ResourcesUtils.Tiles.ExpectedSplitImages(new ImagesBuilder()
			.Transparents(keepTransparent)
			.IgnoringCopies(ignoreCopies)
			.Get());
		Assert.Equal(expected.Count, data.Images.Count);
	}
}
