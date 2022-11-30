using NextConvert.Sources.Helpers;
using NextConvert.Sources.ImageUtils;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace UnitTests.Helpers;

public static class ResourcesUtils
{
	#region Sprites

	public enum SpriteImage
	{
		Chequered,
		One,
		Two,
		Three,
		OneRotatedCCW90,
		OneRotated180,
		OneRotatedCW90,
		OneMirroredHorizontally,
		WhiteBox,
		RedBox,
		Four
	}

	public static Argb32 GetSpritesTransparentColour() => new(a: 255, r: 235, g: 51, b: 255);

	public static Argb32 GetSpritesSheetBackgroundColour() => new(a: 255, r: 1, g: 30, b: 43);

	public static Image<Argb32> GetSpritesSourceImage() => Image.Load<Argb32>(Resources.Project_Sprites_Image);

	public static ImageData GetSpriteResultImage(SpriteImage? index)
	{
		var image = index switch
		{
			null => Image.Load<Argb32>(Resources.export_sprites_image_transparent),
			SpriteImage.Chequered => Image.Load<Argb32>(Resources.export_sprites_image0),
			SpriteImage.One => Image.Load<Argb32>(Resources.export_sprites_image1),
			SpriteImage.Two => Image.Load<Argb32>(Resources.export_sprites_image2),
			SpriteImage.Three => Image.Load<Argb32>(Resources.export_sprites_image3),
			SpriteImage.OneRotatedCCW90 => Image.Load<Argb32>(Resources.export_sprites_image4),
			SpriteImage.OneRotated180 => Image.Load<Argb32>(Resources.export_sprites_image5),
			SpriteImage.OneRotatedCW90 => Image.Load<Argb32>(Resources.export_sprites_image6),
			SpriteImage.OneMirroredHorizontally => Image.Load<Argb32>(Resources.export_sprites_image7),
			SpriteImage.WhiteBox => Image.Load<Argb32>(Resources.export_sprites_image8),
			SpriteImage.RedBox => Image.Load<Argb32>(Resources.export_sprites_image9),
			SpriteImage.Four => Image.Load<Argb32>(Resources.export_sprites_image10),
			_ => throw new ArgumentException($"Index {index} is not valid!"),
		};

		return new ImageData
		{
			Image = image,
			Position = Point.Empty,
			IsTransparent = (index == null)
		};
	}

	public static List<ImageData> GetSpriteResultImages(KeepTransparentType keepTransparents, bool ignoreCopies)
	{
		return keepTransparents switch
		{
			KeepTransparentType.None when ignoreCopies => new()
			{
				// line 1 (second 1 is copy)
				GetSpriteResultImage(SpriteImage.Chequered),
				GetSpriteResultImage(SpriteImage.One),
				GetSpriteResultImage(SpriteImage.Two),
				GetSpriteResultImage(SpriteImage.Three),
				// line 2 (all 1-es are copies)
				// line 3
				GetSpriteResultImage(SpriteImage.WhiteBox),
				GetSpriteResultImage(SpriteImage.RedBox),
				GetSpriteResultImage(SpriteImage.Four),
				// line 4 (only transparents)
			},

			KeepTransparentType.None when !ignoreCopies => new()
			{
				// line 1
				GetSpriteResultImage(SpriteImage.Chequered),
				GetSpriteResultImage(SpriteImage.One),
				GetSpriteResultImage(SpriteImage.Two),
				GetSpriteResultImage(SpriteImage.Three),
				GetSpriteResultImage(SpriteImage.One),
				// line 2
				GetSpriteResultImage(SpriteImage.OneRotatedCCW90),
				GetSpriteResultImage(SpriteImage.OneRotated180),
				GetSpriteResultImage(SpriteImage.OneRotatedCW90),
				GetSpriteResultImage(SpriteImage.OneMirroredHorizontally),
				GetSpriteResultImage(SpriteImage.OneRotated180),
				// line 3
				GetSpriteResultImage(SpriteImage.WhiteBox),
				GetSpriteResultImage(SpriteImage.RedBox),
				GetSpriteResultImage(SpriteImage.Four),
				// line 4 (only transparents)
			},

			KeepTransparentType.All when ignoreCopies => new()
			{
				// line 1 (second 1 is copy)
				GetSpriteResultImage(SpriteImage.Chequered),
				GetSpriteResultImage(SpriteImage.One),
				GetSpriteResultImage(SpriteImage.Two),
				GetSpriteResultImage(SpriteImage.Three),
				GetSpriteResultImage(null),
				// line 2 (all 1-es are copies)
				GetSpriteResultImage(null),
				// line 3
				GetSpriteResultImage(null),
				GetSpriteResultImage(SpriteImage.WhiteBox),
				GetSpriteResultImage(SpriteImage.RedBox),
				GetSpriteResultImage(SpriteImage.Four),
				GetSpriteResultImage(null),
				GetSpriteResultImage(null),
				// line 4
				GetSpriteResultImage(null),
				GetSpriteResultImage(null),
				GetSpriteResultImage(null),
				GetSpriteResultImage(null),
				GetSpriteResultImage(null),
				GetSpriteResultImage(null),
			},

			KeepTransparentType.All when !ignoreCopies => new()
			{
				// line 1 (second 1 is copy)
				GetSpriteResultImage(SpriteImage.Chequered),
				GetSpriteResultImage(SpriteImage.One),
				GetSpriteResultImage(SpriteImage.Two),
				GetSpriteResultImage(SpriteImage.Three),
				GetSpriteResultImage(SpriteImage.One),
				GetSpriteResultImage(null),
				// line 2 (all 1-es are copies)
				GetSpriteResultImage(SpriteImage.OneRotatedCCW90),
				GetSpriteResultImage(SpriteImage.OneRotated180),
				GetSpriteResultImage(SpriteImage.OneRotatedCW90),
				GetSpriteResultImage(SpriteImage.OneMirroredHorizontally),
				GetSpriteResultImage(SpriteImage.OneRotated180),
				GetSpriteResultImage(null),
				// line 3
				GetSpriteResultImage(null),
				GetSpriteResultImage(SpriteImage.WhiteBox),
				GetSpriteResultImage(SpriteImage.RedBox),
				GetSpriteResultImage(SpriteImage.Four),
				GetSpriteResultImage(null),
				GetSpriteResultImage(null),
				// line 4
				GetSpriteResultImage(null),
				GetSpriteResultImage(null),
				GetSpriteResultImage(null),
				GetSpriteResultImage(null),
				GetSpriteResultImage(null),
				GetSpriteResultImage(null),
			},

			KeepTransparentType.Boxed when ignoreCopies => new()
			{
				// line 1 (second 1 is copy)
				GetSpriteResultImage(SpriteImage.Chequered),
				GetSpriteResultImage(SpriteImage.One),
				GetSpriteResultImage(SpriteImage.Two),
				GetSpriteResultImage(SpriteImage.Three),
				// line 2 (all 1-es are copies)
				// line 3
				GetSpriteResultImage(null),
				GetSpriteResultImage(SpriteImage.WhiteBox),
				GetSpriteResultImage(SpriteImage.RedBox),
				GetSpriteResultImage(SpriteImage.Four),
				// line 4
			},

			KeepTransparentType.Boxed when !ignoreCopies => new()
			{
				// line 1 (second 1 is copy)
				GetSpriteResultImage(SpriteImage.Chequered),
				GetSpriteResultImage(SpriteImage.One),
				GetSpriteResultImage(SpriteImage.Two),
				GetSpriteResultImage(SpriteImage.Three),
				GetSpriteResultImage(SpriteImage.One),
				// line 2 (all 1-es are copies)
				GetSpriteResultImage(SpriteImage.OneRotatedCCW90),
				GetSpriteResultImage(SpriteImage.OneRotated180),
				GetSpriteResultImage(SpriteImage.OneRotatedCW90),
				GetSpriteResultImage(SpriteImage.OneMirroredHorizontally),
				GetSpriteResultImage(SpriteImage.OneRotated180),
				// line 3
				GetSpriteResultImage(null),
				GetSpriteResultImage(SpriteImage.WhiteBox),
				GetSpriteResultImage(SpriteImage.RedBox),
				GetSpriteResultImage(SpriteImage.Four),
				// line 4
			},

			_ => new(),
		};
	}

	public static List<IndexedData.Colour> GetSpriteResultsPalette(bool is4Bit)
	{
		var data = is4Bit ? Resources.export_sprites_sheet_4bit : Resources.export_sprites_sheet_8bit;
		return GetResultsPalette(
			data: data, 
			is4Bit: is4Bit, 
			background: GetSpritesSheetBackgroundColour(),
			transparent: GetSpritesTransparentColour(),
			colourDelta: new Point(34, 29));
	}

	#endregion

	#region Tiles

	public enum TileImage
	{
		GrayHorizontalGradient,
		GrayVerticalGradient,
		RedGradient,
		YellowGradient
	}

	public static Argb32 GetTilesTransparentColour() => new(a: 255, r: 255, g: 0, b: 255);

	public static Argb32 GetTileSheetBackgroundColour() => GetSpritesSheetBackgroundColour();

	public static Image<Argb32> GetTilesSourceImage() => Image.Load<Argb32>(Resources.Project_Tiles_Image);

	public static ImageData GetTileResultImage(TileImage? index)
	{
		var image = index switch
		{
			null => Image.Load<Argb32>(Resources.export_tiles_image0),
			TileImage.GrayHorizontalGradient => Image.Load<Argb32>(Resources.export_tiles_image1),
			TileImage.GrayVerticalGradient => Image.Load<Argb32>(Resources.export_tiles_image2),
			TileImage.RedGradient => Image.Load<Argb32>(Resources.export_tiles_image3),
			TileImage.YellowGradient => Image.Load<Argb32>(Resources.export_tiles_image4),
			_ => throw new ArgumentException($"Index {index} is not valid!"),
		};

		return new ImageData
		{
			Image = image,
			Position = Point.Empty,
			IsTransparent = (index == null)
		};
	}

	public static List<ImageData> GetTileResultImages(KeepTransparentType keepTransparents, bool ignoreCopies)
	{
		return keepTransparents switch
		{
			KeepTransparentType.None when ignoreCopies => new()
			{
				GetTileResultImage(TileImage.GrayHorizontalGradient),
				GetTileResultImage(TileImage.RedGradient),
				GetTileResultImage(TileImage.YellowGradient),
			},

			KeepTransparentType.None when !ignoreCopies => new()
			{
				GetTileResultImage(TileImage.GrayHorizontalGradient),
				GetTileResultImage(TileImage.GrayVerticalGradient),
				GetTileResultImage(TileImage.RedGradient),
				GetTileResultImage(TileImage.YellowGradient),
			},

			KeepTransparentType.All when ignoreCopies => new()
			{
				GetTileResultImage(null),
				GetTileResultImage(TileImage.GrayHorizontalGradient),
				GetTileResultImage(TileImage.RedGradient),
				GetTileResultImage(TileImage.YellowGradient),
			},

			KeepTransparentType.All when !ignoreCopies => new()
			{
				GetTileResultImage(null),
				GetTileResultImage(TileImage.GrayHorizontalGradient),
				GetTileResultImage(TileImage.GrayVerticalGradient),
				GetTileResultImage(TileImage.RedGradient),
				GetTileResultImage(TileImage.YellowGradient),
			},

			KeepTransparentType.Boxed when ignoreCopies => new()
			{
				GetTileResultImage(null),
				GetTileResultImage(TileImage.GrayHorizontalGradient),
				GetTileResultImage(TileImage.RedGradient),
				GetTileResultImage(TileImage.YellowGradient),
			},

			KeepTransparentType.Boxed when !ignoreCopies => new()
			{
				GetTileResultImage(null),
				GetTileResultImage(TileImage.GrayHorizontalGradient),
				GetTileResultImage(TileImage.GrayVerticalGradient),
				GetTileResultImage(TileImage.RedGradient),
				GetTileResultImage(TileImage.YellowGradient),
			},

			_ => new()
		};
	}

	public static List<IndexedData.Colour> GetTileResultsPalette()
	{
		// Tiles palette includes unused colours at the end, so we must manually restrict the count.
		return GetResultsPalette(
			data: Resources.export_tiles_sheet, 
			is4Bit: true, 
			background: GetTileSheetBackgroundColour(),
			transparent: GetTilesTransparentColour(),
			colourDelta: new Point(35, 29),
			usedColoursCount: 24);
	}

	#endregion

	#region Helpers

	private static List<IndexedData.Colour> GetResultsPalette(
		byte[] data,
		bool is4Bit,
		Argb32 background,
		Argb32 transparent,
		Point colourDelta,
		int? usedColoursCount = null)
	{
		var result = new List<IndexedData.Colour>();

		// We use colours from the sprite sheet to prepare the palette. Note this requires that both sheet images use the same setup when creating: 5 sprites per row, 8 colours per row etc.
		var image = Image.Load<Argb32>(data);
		var point = new Point(118, 29);

		for (int y = 0; y < 8; y++)
		{
			for (int x = 0; x < 8; x++)
			{
				var p = new Point(
					x: point.X + x * colourDelta.X,
					y: point.Y + y * colourDelta.Y);

				// Exit if reached below last pixel row.
				if (point.Y >= image.Height) return result;

				// Set transparent flag; for 8-bit it's just the first colour while for 4-bit every 16th (so every second line since we expect 8 columns in each row).
				var isTransparent = is4Bit ? (x == 0 && y % 2 == 0) : (x == 0 && y == 0);

				// Set used flag; this depends on whether we cross used colours count.
				var isUsed = (usedColoursCount == null || result.Count <= usedColoursCount);

				// Get colour at the pixel, or if unused, set it to transparent (patterns rendered on unused colours are sometimes starting from (0,0) relative to rect). If we reached background, exit.
				var colour = image[p.X, p.Y];
				if (colour == background) return result;
				if (!isUsed) colour = new Argb32(a: transparent.A, r: transparent.R, g: transparent.G, b: transparent.B);

				// Add the colour.
				result.Add(new IndexedData.Colour(colour, isTransparent, isUsed));
			}
		}

		return result;
	}

	#endregion
}
