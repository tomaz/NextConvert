using NextConvert.Sources.Helpers;
using NextConvert.Sources.ImageUtils;

using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces.Conversion;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Text;
using System.Xml;

namespace NextConvert.Sources.Exporters;

/// <summary>
/// Exports images and data as a sheet - this is mainly debuggin aid that visually represents and explains exported data.
/// </summary>
public class SheetExporter
{
	#region General parameters

	public IndexedData? Data { get; set; }
	public Argb32 BackgroundColour { get; set; } = Color.Transparent;

	public bool IsPaletteCountExported { get; set; } = true;
	public bool IsPalette9Bit { get; set; } = true;
	public bool Is4BitColour { get; set; } = false;

	#endregion

	#region General rendering properties

	/// <summary>
	/// Spacing between different groups of elements as well as padding around the image.
	/// </summary>
	public int Spacing { get; set; } = 16;

	#endregion

	#region Image rendering properties

	/// <summary>
	/// Width of the images.
	/// </summary>
	public int ItemWidth { get; set; } = 16;

	/// <summary>
	/// Height of the images.
	/// </summary>
	public int ItemHeight { get; set; } = 16;

	/// <summary>
	/// Number of images per row, before moving to next row.
	/// </summary>
	public int ItemsPerRow { get; set; } = 16;

	#endregion

	#region Palette rendering properties

	/// <summary>
	/// Width of a colour rectangle.
	/// </summary>
	public int ColourWidth { get; set; } = 16;

	/// <summary>
	/// Height of the colour rectangle.
	/// </summary>
	public int ColourHeight { get; set; } = 16;

	/// <summary>
	/// Number of colours per row, before moving to next row.
	/// </summary>
	public int ColoursPerRow { get; set; } = 8;

	/// <summary>
	/// Specifies whether each colour should also include it's binary value.
	/// </summary>
	public bool ColourShowValue { get; set; } = true;

	#endregion

	#region Private properties

	private FontRenderer FontRenderer { get; set; } = new();

	private Argb32 BorderColour { get; set; }
	private Argb32 IdentifierColour { get; set; }
	private Argb32 IdentifierBackgroundColour { get; set; }

	private int ItemSpacing { get; set; } = 1;
	private int ColourSpacing { get; set; } = 1;
	private int ColourTextWidth { get; set; } = 0;
	private Size ColourByteSize { get; set; } = Size.Empty;

	private int ItemsTotalWidth { get; set; } = 0;
	private int HeaderHeight { get; set; } = 0;

	#endregion

	#region Public

	public void Export(FileInfo filename)
	{
		if (Data == null) throw new InvalidDataException("Data is required, make sure property is assigned");

		var imageWidth = CalculateImageWidth();
		var imageHeight = CalculateImageHeight();

		PrepareColours();

		using (var image = new Image<Argb32>(width: imageWidth, height: imageHeight))
		{
			image.Mutate(context =>
			{
				context.Fill(BackgroundColour, new RectangleF(0, 0, imageWidth, imageHeight));

				DrawImages(context);
				DrawPalette(context);
			});

			image.Save(filename);
		}
	}

	#endregion

	#region Drawing

	private void DrawImages(IImageProcessingContext context)
	{
		var rowCounter = 0;
		var index = 0;

		var x = Spacing;
		var y = Spacing;

		void DrawBorder()
		{
			context.Draw(BorderColour, 1, new RectangleF(
				x: x,
				y: y,
				width: ItemWidth + ItemSpacing,
				height: HeaderHeight + ItemHeight + ItemSpacing));
		}

		void DrawIdentifier(int index)
		{
			// Fill in the background behind index text.
			context.Fill(IdentifierBackgroundColour, new RectangleF(
				x: x + 1,
				y: y + 1,
				width: ItemWidth,
				height: HeaderHeight));

			// Render image index.
			FontRenderer.DrawText(
				context: context, 
				text: index.ToString(), 
				color: IdentifierColour, 
				x: x + 2, 
				y: y + 1);
		}

		void DrawImage(IndexedImage image)
		{
			// Render image itself.
			for (int iy = 0; iy < image.Height; iy++)
			{
				for (int ix = 0; ix < image.Width; ix++)
				{
					var colourIndex = image[ix, iy];
					var colour = Data!.Palette[colourIndex];

					context.SetPixel(colour.AsArgb32,
						x + ix + 1,
						y + iy + HeaderHeight + 1);
				}
			}
		}

		foreach (var image in Data!.Images)
		{
			// Draw the image.
			DrawBorder();
			DrawIdentifier(index);
			DrawImage(image);

			// Update for next image.
			index++;
			x += ItemWidth + ItemSpacing;

			// Move to next row if needed.
			if (++rowCounter >= ItemsPerRow)
			{
				rowCounter = 0;

				x = Spacing;
				y += ItemHeight + ItemSpacing + HeaderHeight;
			}
		}
	}

	private void DrawPalette(IImageProcessingContext context)
	{
		var rowCounter = 0;
		var index = 0;

		var xStart = ItemsTotalWidth + Spacing * 2;
		var x = xStart;
		var y = Spacing;

		void DrawBorder()
		{
			context.Draw(BorderColour, 1, new RectangleF(
				x: x,
				y: y,
				width: ColourWidth + ColourSpacing,
				height: HeaderHeight + ColourHeight + ColourSpacing));
		}

		void DrawIdentifier(int index)
		{
			// Fill in the background behind index text.
			context.Fill(IdentifierBackgroundColour, new RectangleF(
				x: x + 1,
				y: y + 1,
				width: ColourWidth,
				height: HeaderHeight));

			// Render colour index.
			FontRenderer.DrawText(
				context: context,
				text: index.ToString(),
				color: IdentifierColour,
				x: x + 2,
				y: y + 1);
		}

		void DrawColour(IndexedData.Colour colour)
		{
			context.Fill(colour.AsArgb32, new RectangleF(
				x: x + 1,
				y: HeaderHeight + y + 1,
				width: ColourWidth,
				height: ColourHeight));

			if (colour.IsTransparent)
			{
				var centerX = x + ColourWidth / 2;
				var centerY = y + HeaderHeight + ColourHeight / 2 + 1;
				var offset = ColourHeight / 5;

				context.DrawLines(
					color: BorderColour,
					thickness: 1,
					points: new PointF[] {
						new PointF(centerX - offset, centerY - offset),
						new PointF(centerX + offset, centerY + offset)
					});

				context.DrawLines(
					color: BorderColour,
					thickness: 1,
					points: new PointF[] {
						new PointF(centerX - offset, centerY + offset),
						new PointF(centerX + offset, centerY - offset)
					});
			}
		}

		void DrawValue(IndexedData.Colour colour)
		{
			if (!ColourShowValue) return;

			var left = x + ColourWidth + ColourSpacing * 2 + 2;
			var top = y + HeaderHeight + (ColourHeight - (int)ColourByteSize.Height) / 2;

			if (IsPalette9Bit)
			{
				var values = colour.As9BitColour;

				FontRenderer.DrawText(
					context: context,
					text: values[0].ToString("X2"),
					color: BorderColour,
					x: left,
					y: top);

				FontRenderer.DrawText(
					context: context,
					text: values[1].ToString("X2"),
					color: BorderColour,
					x: left + (int)ColourByteSize.Width + ColourSpacing * 2,
					y: top);
			}
			else
			{
				FontRenderer.DrawText(
					context: context,
					text: colour.As8BitColour.ToString("X2"),
					color: BorderColour,
					x: left,
					y: top);
			}
		}

		foreach (var colour in Data!.Palette)
		{
			// Draw the colour
			DrawBorder();
			DrawIdentifier(index);
			DrawColour(colour);
			DrawValue(colour);

			// Update for next colour
			index++;
			x += ColourWidth + ColourSpacing + ColourTextWidth;

			// Move to next row if needed.
			if (++rowCounter >= ColoursPerRow)
			{
				rowCounter = 0;

				x = xStart;
				y += ColourHeight + ColourSpacing + HeaderHeight;
			}
		}
	}

	#endregion

	#region Helpers

	private void PrepareColours()
	{
		BorderColour = BackgroundColour.IsDark() ? Color.White : Color.DarkSlateGray;
		IdentifierColour = BorderColour;
		IdentifierBackgroundColour = BorderColour.WithAlpha(40);
	}

	private int CalculateImageWidth()
	{
		int HexValueTextWidth()
		{
			if (!ColourShowValue) return 0;

			ColourByteSize = FontRenderer.MeasureText("88");
			if (ColourByteSize == Size.Empty) return 0;

			var byteWidth = ColourByteSize.Width;
			var textWidth = IsPalette9Bit ? byteWidth * 2 + ColourSpacing * 2 : byteWidth;
			return textWidth + ColourSpacing * 8;
		}

		ColourTextWidth = HexValueTextWidth();

		var itemColumns = Math.Min(ItemsPerRow, Data!.Images.Count);
		var itemColumnWidth = ItemWidth + ItemSpacing;
		var itemsWidth = itemColumnWidth * itemColumns;

		var paletteColumns = Math.Min(ColoursPerRow, Data.Palette.Count);
		var paletteColumnWidth = ColourWidth + ColourSpacing + ColourTextWidth;
		var paletteWidth = paletteColumnWidth * paletteColumns;

		ItemsTotalWidth = itemsWidth;

		return itemsWidth + paletteWidth + Spacing * 3;
	}

	private int CalculateImageHeight()
	{
		HeaderHeight = FontRenderer.Enabled ? 12 : 0;

		var itemRows = (int)Math.Ceiling((double)Data!.Images.Count / (double)ItemsPerRow);
		var itemRowHeight = ItemHeight + ItemSpacing + HeaderHeight;
		var itemsHeight = itemRowHeight * itemRows;

		var paletteRows = (int)Math.Ceiling((double)Data.Palette.Count / (double)ColoursPerRow);
		var paletteRowHeight = ColourHeight + ColourSpacing + HeaderHeight;
		var paletteHeight = paletteRowHeight * paletteRows;

		return Math.Max(itemsHeight, paletteHeight) + Spacing * 2;
	}

	#endregion
}

#region Extensions

internal static class SheetExporterExtensions
{
	/// <summary>
	/// Determines the format based on the file extension and saves the image or throws exception if format is not recognized, or saving fails.
	/// </summary>
	internal static void Save<T>(this Image<T> image, FileInfo filename) where T : unmanaged, IPixel<T>
	{
		switch (filename.Extension.ToLower().Replace(".", ""))
		{
			case "bmp":
				image.SaveAsBmp(filename.FullName);
				break;
			case "png":
				image.SaveAsPng(filename.FullName);
				break;
			case "jpg":
			case "jpeg":
				image.SaveAsJpeg(filename.FullName);
				break;
			case "gif":
				image.SaveAsGif(filename.FullName);
				break;
			case "pbm":
				image.SaveAsPbm(filename.FullName);
				break;
			case "tga":
				image.SaveAsTga(filename.FullName);
				break;
			case "webp":
				image.SaveAsWebp(filename.FullName);
				break;
			default:
				throw new InvalidDataException($"Image format {filename.Extension} not supported, use one of `bmp`, `png`, `jpg`, `gif`, `pbm`, `tga`, `webp`");
		}
	}
}

#endregion
