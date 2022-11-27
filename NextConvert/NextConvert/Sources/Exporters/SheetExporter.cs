using NextConvert.Sources.Helpers;
using NextConvert.Sources.ImageUtils;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using System.Runtime.CompilerServices;

namespace NextConvert.Sources.Exporters;

/// <summary>
/// Exports images and data as a sheet - this is mainly debuggin aid that visually represents and explains exported data.
/// </summary>
public class SheetExporter
{
	public IndexedData? Data { get; set; }
	public Argb32 TransparentColour { get; set; } = Color.Transparent;

	public int ItemWidth { get; set; } = 16;
	public int ItemHeight { get; set; } = 16;
	public int ItemsPerRow { get; set; } = 16;
	public int ItemsSpacing { get; set; } = 2;

	public int ColourWidth { get; set; } = 16;
	public int ColourHeight { get; set; } = 16;
	public int ColoursPerRow { get; set; } = 16;
	public int ColoursSpacing { get; set; } = 2;

	public int Spacing { get; set; } = 16;
	
	public bool IsPaletteCountExported { get; set; } = true; 
	public bool IsPalette9Bit { get; set; } = true;
	public bool Is4BitColour { get; set; } = false;

	private int ItemsTotalWidth { get; set; } = 0;

	#region Public

	public void Export(FileInfo filename)
	{
		if (Data == null) throw new InvalidDataException("Data is required, make sure property is assigned");

		var imageWidth = CalculateImageWidth();
		var imageHeight = CalculateImageHeight();

		using (var image = new Image<Argb32>(width: imageWidth, height: imageHeight))
		{
			image.Mutate(context =>
			{
				context.Fill(TransparentColour, new RectangleF(0, 0, imageWidth, imageHeight));

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
		var x = Spacing;
		var y = Spacing;

		foreach (var image in Data!.Images)
		{
			for (int iy = 0; iy < image.Height; iy++)
			{
				for (int ix = 0; ix < image.Width; ix++)
				{
					var colourIndex = image[ix, iy];
					var colour = Data.Palette[colourIndex];

					context.SetPixel(colour.AsArgb32, x + ix, y + iy);
				}
			}

			x += ItemWidth + ItemsSpacing;

			if (++rowCounter >= ItemsPerRow)
			{
				rowCounter = 0;

				x = Spacing;
				y += ItemHeight + ItemsSpacing;
			}
		}
	}

	private void DrawPalette(IImageProcessingContext context)
	{
		var rowCounter = 0;
		var xStart = ItemsTotalWidth + Spacing * 2;

		var x = xStart;
		var y = Spacing;

		var framePen = new Pen(Color.Black, 1);

		foreach (var colour in Data!.Palette)
		{
			var frame = new RectangleF(x, y, ColourWidth, ColourHeight);

			context.Fill(colour.AsArgb32, frame);
			context.Draw(framePen, frame);

			x += ColourWidth + ColoursSpacing;

			if (++rowCounter >= ColoursPerRow)
			{
				rowCounter = 0;

				x = xStart;
				y += ColourHeight + ColoursSpacing;
			}
		}
	}

	#endregion

	#region Helpers

	private int CalculateImageWidth()
	{
		var itemColumns = Math.Min(ItemsPerRow, Data!.Images.Count);
		var itemColumnWidth = ItemWidth + ItemsSpacing;
		var itemsWidth = itemColumnWidth * itemColumns;

		var paletteColumns = Math.Min(ColoursPerRow, Data.Palette.Count);
		var paletteColumnWidth = ColourWidth + ColoursSpacing;
		var paletteWidth = paletteColumnWidth * paletteColumns;

		ItemsTotalWidth = itemsWidth;

		return itemsWidth + paletteWidth + Spacing * 3;
	}

	private int CalculateImageHeight()
	{
		var itemRows = (int)Math.Ceiling((double)Data!.Images.Count / (double)ItemsPerRow);
		var itemRowHeight = ItemHeight + ItemsSpacing;
		var itemsHeight = itemRowHeight * itemRows;

		var paletteRows = (int)Math.Ceiling((double)Data.Palette.Count / (double)ColoursPerRow);
		var paletteRowHeight = ColourHeight + ColoursSpacing;
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
