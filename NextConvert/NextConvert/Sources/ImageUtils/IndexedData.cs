using SixLabors.ImageSharp.PixelFormats;

namespace NextConvert.Sources.ImageUtils;

/// <summary>
/// Represents palette and related list of indexed bitmaps.
/// </summary>
public class IndexedData
{
	/// <summary>
	/// The colour palette.
	/// </summary>
	public List<Colour> Colours { get; } = new();

	/// <summary>
	/// Images with indexes into the <see cref="Colours"/>.
	/// </summary>
	public List<Image> Images { get; } = new();

	#region Public

	/// <summary>
	/// Adds the given color to the end of the list if it's not present in the list yet. Either way, returns the index of existing colour if match was found, or the index of newly added colour if distinct.
	/// </summary>
	public int AddIfDistinct(Argb32 colour, bool isTransparent)
	{
		var indexedColour = new Colour(colour, isTransparent);

		var index = Colours.FindIndex(c => c.IsSameColour(indexedColour));
		if (index >= 0) return index;
		
		Colours.Add(indexedColour);

		return Colours.Count - 1;
	}

	#endregion

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
		public int Width { get; private set; }
		public int Height { get; private set; }

		private byte[,] data;

		#region Initialization & Disposal

		public Image(int width, int height)
		{
			Width = width;
			Height = height;
			data = new byte[height, width];
		}

		#endregion

		#region Public

		/// <summary>
		/// Provides access to the underlying data.
		/// </summary>
		public byte this[int x, int y]
		{
			get => data[y, x];
			set => data[y, x] = value;
		}

		#endregion
	}

	#endregion
}

#region Extensions

internal static class IndexedDataExtensions
{
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