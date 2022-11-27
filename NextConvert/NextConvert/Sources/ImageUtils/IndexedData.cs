using SixLabors.ImageSharp.PixelFormats;

using static NextConvert.Sources.ImageUtils.IndexedData;

namespace NextConvert.Sources.ImageUtils;

/// <summary>
/// Represents palette and related list of indexed bitmaps.
/// </summary>
public class IndexedData
{
	/// <summary>
	/// The palette.
	/// </summary>
	public List<Colour> Palette { get; } = new();

	/// <summary>
	/// Images with indexes into the <see cref="Palette"/>.
	/// </summary>
	public List<IndexedImage> Images { get; } = new();

	#region Public

	/// <summary>
	/// Adds the given color to the end of the list if it's not present in the list yet.
	/// </summary>
	public int AddIfDistinct(Argb32 colour, bool isTransparent)
	{
		var index = Palette.FindIndex(c => c.AsArgb32 == colour);
		if (index >= 0) return index;
		
		Palette.Add(new Colour(colour, isTransparent));

		return Palette.Count - 1;
	}

	#endregion

	#region Declarations

	public class Colour
	{
		public bool IsTransparent { get; set; }

		public Argb32 AsArgb32 { get; set; }
		public byte As8BitColour { get; private set; }
		public byte[] As9BitColour { get; private set; }

		public Colour(Argb32 argb, bool isTransparent = false)
		{
			IsTransparent = isTransparent;
			AsArgb32 = argb;
			As8BitColour = argb.As8BitColour();
			As9BitColour = argb.As9BitColour();
		}
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