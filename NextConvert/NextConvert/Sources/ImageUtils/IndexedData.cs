using SixLabors.ImageSharp.PixelFormats;

using System.Data;

using static NextConvert.Sources.ImageUtils.IndexedData;

namespace NextConvert.Sources.ImageUtils;

/// <summary>
/// Represents palette and related list of indexed bitmaps.
/// </summary>
public class IndexedData
{
	/// <summary>
	/// Maximum number of colours per bank when 4-bit images are used.
	/// </summary>
	public const int MaxColoursPerBank = 16;

	/// <summary>
	/// The colour palette.
	/// </summary>
	public List<Colour> Colours { get; } = new();

	/// <summary>
	/// Images with indexes into the <see cref="Colours"/>.
	/// </summary>
	public List<Image> Images { get; } = new();

	#region Declarations

	public class Colour
	{
		public Argb32 AsArgb32 { get; set; }
		public byte As8BitColour { get; private set; }
		public byte[] As9BitColour { get; private set; }
		public bool IsTransparent { get; set; }

		#region Initialization & Disposal

		public Colour(Argb32 argb, bool isTransparent = false)
		{
			AsArgb32 = argb;
			As8BitColour = argb.As8BitColour();
			As9BitColour = argb.As9BitColour();
			IsTransparent = isTransparent;
		}

		#endregion

		#region Overrides

		public override string ToString() => $"0x{As9BitColour[0]:X2}{As9BitColour[1]:X2}{(IsTransparent ? "T" : "")}";

		#endregion

		#region Helpers

		public bool IsSameColour(Colour other)
		{
			// We compare by actual Next colours since only a subset of all possible colours from 8-bit ARGB palette is possible. Therefore some distinct colours from 8-bit palette may become the same colour on Next.
			if (As9BitColour[0] != other.As9BitColour[0]) return false;
			if (As9BitColour[1] != other.As9BitColour[1]) return false;
			return true;
		}

		#endregion
	}

	/// <summary>
	/// Image where each pixel is represented as an index into colour palette.
	/// </summary>
	public class Image
	{
		/// <summary>
		/// Underlying source image, can be used to get more information like source position etc.
		/// </summary>
		public ImageData SourceImage { get; }

		public int PaletteBankOffset { get; set; } = 0;
		public int Width { get => SourceImage.Width; }
		public int Height { get => SourceImage.Height; }

		private byte[,] data;

		#region Initialization & Disposal

		public Image(ImageData image)
		{
			SourceImage = image;
			data = new byte[image.Height, image.Width];
		}

		public Image(Image copy) : this(copy.SourceImage)
		{
			PaletteBankOffset = copy.PaletteBankOffset;

			for (int y = 0; y < copy.Height; y++)
			{
				for (int x = 0; x < copy.Width; x++)
				{
					data[y, x] = copy.data[y, x];
				}
			}
		}

		#endregion

		#region Overrides

		public override string ToString() => $"({SourceImage.Position.X},{SourceImage.Position.Y}) top-left pixel ({SourceImage.Position.X * Width},{SourceImage.Position.Y * Height})";

		#endregion

		#region Public

		/// <summary>
		/// Provides access to the underlying data.
		/// </summary>
		public byte this[int x, int y]
		{
			// Note: coordinates are reversed in underlying array so that we get line by line representation
			get => data[y, x];
			set => data[y, x] = value;
		}

		/// <summary>
		/// Remaps the image by changing the given source colour index into the given destination index.
		/// </summary>
		public void RemapColour(byte source, byte destination)
		{
			if (source == destination) return;

			for (int y = 0; y < Height; y++)
			{
				for (int x = 0; x < Width; x++)
				{
					if (this[x, y] == source)
					{
						this[x, y] = destination;
					}
				}
			}
		}

		#endregion
	}

	#endregion
}

#region Extensions

public static class IndexedDataExtensions
{
	/// <summary>
	/// Checks if the given colour is already present in the list; if so, it returns the index of the colour. Otherwise it adds the colour to the end of the list and returns the new index.
	/// </summary>
	/// <remarks>
	/// Note: distinct colours are determined based on ZX Next 3-bit per component palette. This ensures the colours will match exactly the ones on Next, however we can lose precision since for example 3-bit per RGB component colours take only a small subset of 8-bit RGB counterparts.
	/// </remarks>
	public static int AddIfDistinct(this List<Colour> colours, Argb32 colour, bool isTransparent = false)
	{
		return AddIfDistinct(colours, new Colour(colour, isTransparent));
	}

	/// <summary>
	/// Checks if the given colour is already present in the list; if so, it returns the index of the colour. Otherwise it adds the colour to the end of the list and returns the new index.
	/// </summary>
	public static int AddIfDistinct(this List<Colour> colours, Colour colour)
	{
		var index = FindMatching(colours, colour);
		if (index >= 0) return index;

		colours.Add(colour);

		return colours.Count - 1;
	}

	/// <summary>
	/// Finds a matching existing colour and returns its index, or -1 if the colour is distinct and no match was found.
	/// </summary>
	public static int FindMatching(this List<Colour> colours, Colour colour)
	{
		return colours.FindIndex(c => c.IsSameColour(colour));
	}

	internal static byte[] As9BitColour(this Argb32 colour)
	{
		var r = colour.R.Component(3);
		var g = colour.G.Component(3);
		var b = colour.B.Component(3);

		var rgb = (byte)(r << 5 | g << 2 | b >> 1);
		var xxb = (byte)(b & 0b00000001);

		return new byte[] { rgb, xxb };
	}

	internal static byte As8BitColour(this Argb32 colour)
	{
		var r = colour.R.Component(3);
		var g = colour.G.Component(3);
		var b = colour.B.Component(2);

		return (byte)(r << 5 | g << 2 | b);
	}

	internal static byte Component(this byte original, byte bits)
	{
		var multiple = (decimal)Math.Pow(2, bits) - 1;
		return (byte)Math.Round((decimal)original * multiple / 255);
	}
}

#endregion