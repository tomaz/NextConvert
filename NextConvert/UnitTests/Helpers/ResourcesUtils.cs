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

	public static Image<Argb32> GetSpriteResultImage(SpriteImage? index)
	{
		return index switch
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
	}

	public static List<Image<Argb32>> GetSpriteResultImages(bool keepBoxedTransparents, bool ignoreCopies)
	{
		if (!keepBoxedTransparents && !ignoreCopies)
		{
			return new List<Image<Argb32>>()
			{
				GetSpriteResultImage(SpriteImage.Chequered),
				GetSpriteResultImage(SpriteImage.One),
				GetSpriteResultImage(SpriteImage.Two),
				GetSpriteResultImage(SpriteImage.Three),
				GetSpriteResultImage(SpriteImage.One),
				GetSpriteResultImage(SpriteImage.OneRotatedCCW90),
				GetSpriteResultImage(SpriteImage.OneRotated180),
				GetSpriteResultImage(SpriteImage.OneRotatedCW90),
				GetSpriteResultImage(SpriteImage.OneMirroredHorizontally),
				GetSpriteResultImage(SpriteImage.OneRotated180),
				GetSpriteResultImage(SpriteImage.WhiteBox),
				GetSpriteResultImage(SpriteImage.RedBox),
				GetSpriteResultImage(SpriteImage.Four),
			};
		}
		else if (!keepBoxedTransparents && ignoreCopies)
		{
			return new List<Image<Argb32>>()
			{
				GetSpriteResultImage(SpriteImage.Chequered),
				GetSpriteResultImage(SpriteImage.One),
				GetSpriteResultImage(SpriteImage.Two),
				GetSpriteResultImage(SpriteImage.Three),
				GetSpriteResultImage(SpriteImage.WhiteBox),
				GetSpriteResultImage(SpriteImage.RedBox),
				GetSpriteResultImage(SpriteImage.Four),
			};
		}
		else if (keepBoxedTransparents && !ignoreCopies)
		{
			return new List<Image<Argb32>>()
			{
				GetSpriteResultImage(SpriteImage.Chequered),
				GetSpriteResultImage(SpriteImage.One),
				GetSpriteResultImage(SpriteImage.Two),
				GetSpriteResultImage(SpriteImage.Three),
				GetSpriteResultImage(SpriteImage.One),
				GetSpriteResultImage(SpriteImage.OneRotatedCCW90),
				GetSpriteResultImage(SpriteImage.OneRotated180),
				GetSpriteResultImage(SpriteImage.OneRotatedCW90),
				GetSpriteResultImage(SpriteImage.OneMirroredHorizontally),
				GetSpriteResultImage(SpriteImage.OneRotated180),
				GetSpriteResultImage(null),
				GetSpriteResultImage(SpriteImage.WhiteBox),
				GetSpriteResultImage(SpriteImage.RedBox),
				GetSpriteResultImage(SpriteImage.Four),
			};
		}
		else
		{
			return new List<Image<Argb32>>()
			{
				GetSpriteResultImage(SpriteImage.Chequered),
				GetSpriteResultImage(SpriteImage.One),
				GetSpriteResultImage(SpriteImage.Two),
				GetSpriteResultImage(SpriteImage.Three),
				GetSpriteResultImage(null),
				GetSpriteResultImage(SpriteImage.WhiteBox),
				GetSpriteResultImage(SpriteImage.RedBox),
				GetSpriteResultImage(SpriteImage.Four),
			};
		}
	}

	public static List<IndexedData.Colour> GetSpriteResultsPalette(bool is4Bit)
	{
		var result = new List<IndexedData.Colour>();

		// We use colours from the sprite sheet to prepare the palette.
		var image = Image.Load<Argb32>(Resources.export_sprites_sheet);
		var background = GetSpritesSheetBackgroundColour();
		var point = new Point(118, 29);
		var delta = new Point(34, 29);

		for (int y = 0; y < 5; y++)
		{
			for (int x = 0; x < 8; x++)
			{
				var p = new Point(
					x: point.X + x * delta.X,
					y: point.Y + y * delta.Y);

				var colour = image[p.X, p.Y];
				var transparent = x == 0 && y == 0;

				if (colour == background) break;
				result.Add(new IndexedData.Colour(colour, transparent));
			}
		}

		return result;
	}

	#endregion
}
