using NextConvert.Sources.Data;
using NextConvert.Sources.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace UnitTests.Helpers;

public static class ResourcesUtils
{
	static ResourcesUtils()
	{
		Sprites = new SpritesDataProvider();
		Tiles = new TilesDataProvider();
	}

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

	public static SpritesDataProvider Sprites { get; }

	public class SpritesDataProvider : BaseDataProvider<SpriteImage?>
	{
		#region Overrides

		public override List<ImageData> ExpectedSplitImages(ImagesBuilder filter)
		{
			return filter.KeepTransparents switch
			{
				KeepTransparentType.None when filter.IgnoreCopies => new()
				{
					// line 1 (second 1 is copy)
					ResultImage(SpriteImage.Chequered),
					ResultImage(SpriteImage.One),
					ResultImage(SpriteImage.Two),
					ResultImage(SpriteImage.Three),
					// line 2 (all 1-es are copies)
					// line 3
					ResultImage(SpriteImage.WhiteBox),
					ResultImage(SpriteImage.RedBox),
					ResultImage(SpriteImage.Four),
					// line 4 (only transparents)
				},

				KeepTransparentType.None when !filter.IgnoreCopies => new()
				{
					// line 1
					ResultImage(SpriteImage.Chequered),
					ResultImage(SpriteImage.One),
					ResultImage(SpriteImage.Two),
					ResultImage(SpriteImage.Three),
					ResultImage(SpriteImage.One),
					// line 2
					ResultImage(SpriteImage.OneRotatedCCW90),
					ResultImage(SpriteImage.OneRotated180),
					ResultImage(SpriteImage.OneRotatedCW90),
					ResultImage(SpriteImage.OneMirroredHorizontally),
					ResultImage(SpriteImage.OneRotated180),
					// line 3
					ResultImage(SpriteImage.WhiteBox),
					ResultImage(SpriteImage.RedBox),
					ResultImage(SpriteImage.Four),
					// line 4 (only transparents)
				},

				KeepTransparentType.All when filter.IgnoreCopies => new()
				{
					// line 1 (second 1 is copy)
					ResultImage(SpriteImage.Chequered),
					ResultImage(SpriteImage.One),
					ResultImage(SpriteImage.Two),
					ResultImage(SpriteImage.Three),
					ResultImage(null),
					// line 2 (all 1-es are copies)
					ResultImage(null),
					// line 3
					ResultImage(null),
					ResultImage(SpriteImage.WhiteBox),
					ResultImage(SpriteImage.RedBox),
					ResultImage(SpriteImage.Four),
					ResultImage(null),
					ResultImage(null),
					// line 4
					ResultImage(null),
					ResultImage(null),
					ResultImage(null),
					ResultImage(null),
					ResultImage(null),
					ResultImage(null),
				},

				KeepTransparentType.All when !filter.IgnoreCopies => new()
				{
					// line 1 (second 1 is copy)
					ResultImage(SpriteImage.Chequered),
					ResultImage(SpriteImage.One),
					ResultImage(SpriteImage.Two),
					ResultImage(SpriteImage.Three),
					ResultImage(SpriteImage.One),
					ResultImage(null),
					// line 2 (all 1-es are copies)
					ResultImage(SpriteImage.OneRotatedCCW90),
					ResultImage(SpriteImage.OneRotated180),
					ResultImage(SpriteImage.OneRotatedCW90),
					ResultImage(SpriteImage.OneMirroredHorizontally),
					ResultImage(SpriteImage.OneRotated180),
					ResultImage(null),
					// line 3
					ResultImage(null),
					ResultImage(SpriteImage.WhiteBox),
					ResultImage(SpriteImage.RedBox),
					ResultImage(SpriteImage.Four),
					ResultImage(null),
					ResultImage(null),
					// line 4
					ResultImage(null),
					ResultImage(null),
					ResultImage(null),
					ResultImage(null),
					ResultImage(null),
					ResultImage(null),
				},

				KeepTransparentType.Boxed when filter.IgnoreCopies => new()
				{
					// line 1 (second 1 is copy)
					ResultImage(SpriteImage.Chequered),
					ResultImage(SpriteImage.One),
					ResultImage(SpriteImage.Two),
					ResultImage(SpriteImage.Three),
					// line 2 (all 1-es are copies)
					// line 3
					ResultImage(null),
					ResultImage(SpriteImage.WhiteBox),
					ResultImage(SpriteImage.RedBox),
					ResultImage(SpriteImage.Four),
					// line 4
				},

				KeepTransparentType.Boxed when !filter.IgnoreCopies => new()
				{
					// line 1 (second 1 is copy)
					ResultImage(SpriteImage.Chequered),
					ResultImage(SpriteImage.One),
					ResultImage(SpriteImage.Two),
					ResultImage(SpriteImage.Three),
					ResultImage(SpriteImage.One),
					// line 2 (all 1-es are copies)
					ResultImage(SpriteImage.OneRotatedCCW90),
					ResultImage(SpriteImage.OneRotated180),
					ResultImage(SpriteImage.OneRotatedCW90),
					ResultImage(SpriteImage.OneMirroredHorizontally),
					ResultImage(SpriteImage.OneRotated180),
					// line 3
					ResultImage(null),
					ResultImage(SpriteImage.WhiteBox),
					ResultImage(SpriteImage.RedBox),
					ResultImage(SpriteImage.Four),
					// line 4
				},

				_ => new(),
			};
		}

		public override List<IndexedData.Colour> ExpectedPaletteColours(PaletteBuilder builder)
		{
			if (!builder.Is4BitImage)
			{
				// For 8-bit palette the function should be able to deduct all colours without any custom handlers.
				return GetResultsPalette(
					data: Resources.export_sprites_sheet_8bit);
			}
			else
			{
				// In 4bit palette every 16th colour is transparent. And all following colours are unused:
				// - y=1: last 4
				// - y=3: last 3
				// - y=4: last 1
				// - y=5: whole row
				return GetResultsPalette(
					data: Resources.export_sprites_sheet_4bit,
					isUsedHandler: (x, y, count) => y switch
					{
						1 => x < 4,
						3 => x < 5,
						4 => x < 7,
						5 => false,
						_ => true,
					});

			}
		}

		protected override Image<Argb32> OnExpectedImage(SpriteImage? kind) => kind switch
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
			_ => throw new ArgumentException($"{kind} is not valid!"),
		};

		protected override byte[] OnSourceImageData() => Resources.Project_Sprites_Image;

		#endregion
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

	public static TilesDataProvider Tiles { get; }

	public class TilesDataProvider : BaseDataProvider<TileImage?>
	{
		#region Overrides

		public override List<ImageData> ExpectedSplitImages(ImagesBuilder filter)
		{
			return filter.KeepTransparents switch
			{
				KeepTransparentType.None when filter.IgnoreCopies => new()
				{
					ResultImage(TileImage.GrayHorizontalGradient),
					ResultImage(TileImage.RedGradient),
					ResultImage(TileImage.YellowGradient),
				},

				KeepTransparentType.None when !filter.IgnoreCopies => new()
				{
					ResultImage(TileImage.GrayHorizontalGradient),
					ResultImage(TileImage.GrayVerticalGradient),
					ResultImage(TileImage.RedGradient),
					ResultImage(TileImage.YellowGradient),
				},

				KeepTransparentType.All when filter.IgnoreCopies => new()
				{
					ResultImage(null),
					ResultImage(TileImage.GrayHorizontalGradient),
					ResultImage(TileImage.RedGradient),
					ResultImage(TileImage.YellowGradient),
				},

				KeepTransparentType.All when !filter.IgnoreCopies => new()
				{
					ResultImage(null),
					ResultImage(TileImage.GrayHorizontalGradient),
					ResultImage(TileImage.GrayVerticalGradient),
					ResultImage(TileImage.RedGradient),
					ResultImage(TileImage.YellowGradient),
				},

				KeepTransparentType.Boxed when filter.IgnoreCopies => new()
				{
					ResultImage(null),
					ResultImage(TileImage.GrayHorizontalGradient),
					ResultImage(TileImage.RedGradient),
					ResultImage(TileImage.YellowGradient),
				},

				KeepTransparentType.Boxed when !filter.IgnoreCopies => new()
				{
					ResultImage(null),
					ResultImage(TileImage.GrayHorizontalGradient),
					ResultImage(TileImage.GrayVerticalGradient),
					ResultImage(TileImage.RedGradient),
					ResultImage(TileImage.YellowGradient),
				},

				_ => new()
			};
		}

		public override List<IndexedData.Colour> ExpectedPaletteColours(PaletteBuilder builder)
		{
			// Tiles are 4bit therefore every 16th colour is transparent. All colours in the last row are unused.
			return GetResultsPalette(
				data: Resources.export_tiles_sheet,
				isUsedHandler: (x, y, count) => y < 3);
		}

		protected override Image<Argb32> OnExpectedImage(TileImage? index) => index switch
		{
			null => Image.Load<Argb32>(Resources.export_tiles_image0),
			TileImage.GrayHorizontalGradient => Image.Load<Argb32>(Resources.export_tiles_image1),
			TileImage.GrayVerticalGradient => Image.Load<Argb32>(Resources.export_tiles_image2),
			TileImage.RedGradient => Image.Load<Argb32>(Resources.export_tiles_image3),
			TileImage.YellowGradient => Image.Load<Argb32>(Resources.export_tiles_image4),
			_ => throw new ArgumentException($"Index {index} is not valid!"),
		};

		protected override byte[] OnSourceImageData() => Resources.Project_Tiles_Image;

		#endregion
	}

	#endregion

	#region Declarations

	public abstract class BaseDataProvider<T>
	{
		#region Subclass

		/// <summary>
		/// Returns transparent colour for source images.
		/// </summary>
		public virtual Argb32 TransparentColour() => new(a: 255, r: 255, g: 0, b: 255);

		/// <summary>
		/// Returns background colour on generated sheet images.
		/// </summary>
		public virtual Argb32 SheetBackgroundColour() => new(a: 255, r: 1, g: 30, b: 43);

		/// <summary>
		/// Returns source image from resources.
		/// </summary>
		public virtual Image<Argb32> SourceImage() => Image.Load<Argb32>(OnSourceImageData());

		/// <summary>
		/// Generates expected palette raw data. Uses <see cref="ExpectedPaletteColours(PaletteBuilder)"/> under the hood.
		/// </summary>
		public virtual byte[] ExpectedPaletteData(PaletteBuilder builder)
		{
			var colours = ExpectedPaletteColours(builder);

			// Note: this method copies the functionality of the actual method that generates ouput (but it was created independently). Alternatively we could use pre-generated files in resources. However resources are harder to manage.
			var result = new List<byte>();

			if (builder.IsCountExported)
			{
				result.Add((byte)colours.Count);
			}

			foreach (var colour in colours)
			{
				if (builder.Is9Bit)
				{
					result.AddRange(colour.As9BitColour);
				}
				else
				{
					result.Add(colour.As8BitColour);
				}
			}

			return result.ToArray();
		}

		/// <summary>
		/// Generates expected raw image data.
		/// </summary>
		public virtual byte[] ExpectedImageData(ImagesBuilder builder)
		{
			// Note: this method copies the functionality of the actual method that generates ouput (but it was created independently). Alternatively we could use pre-generated files in resources. However resources are harder to manage.
			var result = new List<byte>();

			if (builder.Images != null)
			{
				foreach (var image in builder.Images)
				{
					if (builder.Is4Bit)
					{
						for (int y = 0; y < image.Height; y++)
						{
							for (int x = 0; x < image.Width; x += 2)
							{
								var p1 = image[x, y];
								var p2 = image[x + 1, y];
								var combined = (byte)((p1 << 4) | (p2 & 0x0F));
								result.Add(combined);
							}
						}
					}
					else
					{
						for (int y = 0; y < image.Height; y++)
						{
							for (int x = 0; x < image.Width; x++)
							{
								result.Add(image[x, y]);
							}
						}
					}
				}
			}

			return result.ToArray();
		}

		/// <summary>
		/// Generates all images expected to be returned from <see cref="ImageSplitter"/>.
		/// </summary>
		public abstract List<ImageData> ExpectedSplitImages(ImagesBuilder filter);

		/// <summary>
		/// Generates expected palette colours list.
		/// </summary>
		public abstract List<IndexedData.Colour> ExpectedPaletteColours(PaletteBuilder builder);

		/// <summary>
		/// Specifies whether the given image kind is fully transparent or not.
		/// </summary>
		/// <param name="kind"></param>
		/// <returns></returns>
		protected virtual bool IsTransparent(T? kind) => kind == null;

		/// <summary>
		/// Returns the source image from resources.
		/// </summary>
		protected abstract byte[] OnSourceImageData();

		/// <summary>
		/// Returns result image for the given kind expected to be returned from <see cref="ImageSplitter"/>.
		/// </summary>
		protected abstract Image<Argb32> OnExpectedImage(T? kind);

		#endregion

		#region Helpers

		/// <summary>
		/// Asks subclass to provide requested image via <see cref="OnExpectedImage(T?)"/> then packs it into <see cref="ImageData"/> class.
		/// </summary>
		protected ImageData ResultImage(T? kind)
		{
			var image = OnExpectedImage(kind);

			return new ImageData
			{
				Image = image,
				Position = Point.Empty,
				IsTransparent = (IsTransparent(kind))
			};
		}

		protected List<IndexedData.Colour> GetResultsPalette(
			byte[] data,
			Func<int, int, int, bool>? isUsedHandler = null)
		{
			var result = new List<IndexedData.Colour>();

			// We use colours from the sprite sheet to prepare the palette. Note this requires that all sheet images use the same setup when creating: 5 sprites per row, 8 colours per row, no text etc.
			var image = Image.Load<Argb32>(data);
			var point = new Point(118, 17);
			var pointDelta = new Point(18, 17);

			var background = SheetBackgroundColour();
			var transparent = TransparentColour();

			for (int y = 0; y < 8; y++)
			{
				for (int x = 0; x < 8; x++)
				{
					var p = new Point(
						x: point.X + x * pointDelta.X,
						y: point.Y + y * pointDelta.Y);

					// Exit if reached below last pixel row.
					if (point.Y >= image.Height) return result;

					// Get colour at the pixel. If reached background, exit. 
					var colour = image[p.X, p.Y];
					if (colour == background) return result;

					// Determine if colour is transparent. By default use our transparent colour, but also ask caller if more refined handling is needed.
					var isTransparent = colour == transparent;

					// Determine if colour is used. If handler is provided, use it, otherwise assume it's used. If colour is not used, we should replace it with transparent and set the flag. Also important to not use transparent flag for unused colours (even though we set it to transparent). This ensures test comparisons will match the actual values.
					var isUsed = isUsedHandler == null || isUsedHandler(x, y, result.Count);
					if (!isUsed)
					{
						isTransparent = false;
						colour = new Argb32(a: transparent.A, r: transparent.R, g: transparent.G, b: transparent.B);
					}

					// Add the colour.
					result.Add(new IndexedData.Colour(colour, isTransparent, isUsed));
				}
			}

			return result;
		}

		#endregion
	}

	#endregion
}

#region Builders

public class ImagesBuilder
{
	public List<IndexedData.Image>? Images { get; set; } = null;
	public KeepTransparentType KeepTransparents { get; private set; } = KeepTransparentType.None;
	public bool Is4Bit { get; private set; } = false;
	public bool IgnoreCopies { get; private set; } = false;

	public ImagesBuilder FromImages(List<IndexedData.Image>? images)
	{
		Images = images;
		return this;
	}

	public ImagesBuilder Transparents(KeepTransparentType type)
	{
		KeepTransparents = type;
		return this;
	}

	public ImagesBuilder As4Bit(bool is4Bit)
	{
		Is4Bit = is4Bit;
		return this;
	}

	public ImagesBuilder IgnoringCopies(bool ignore)
	{
		IgnoreCopies = ignore;
		return this;
	}

	public ImagesBuilder Get()
	{
		return this;
	}
}

public class PaletteBuilder
{
	public bool IsCountExported { get; private set; } = false;
	public bool Is9Bit { get; private set; } = false;
	public bool Is4BitImage { get; private set; } = false;

	public PaletteBuilder CountExported(bool isCountExported)
	{
		IsCountExported = isCountExported;
		return this;
	}

	public PaletteBuilder As9Bit(bool is9Bit)
	{
		Is9Bit = is9Bit;
		return this;
	}

	public PaletteBuilder As4BitImage(bool is4Bit)
	{
		Is4BitImage = is4Bit;
		return this;
	}

	public PaletteBuilder Get()
	{
		return this;
	}
}

#endregion

