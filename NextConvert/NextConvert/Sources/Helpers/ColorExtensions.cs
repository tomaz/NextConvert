using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace NextConvert.Sources.Helpers;

public static class ColorExtensions
{
	/// <summary>
	/// Determines if the colour is dark (result is true) or light (result is false).
	/// </summary>
	public static bool IsDark(this Argb32 color)
	{
		var r = (double)color.R;
		var g = (double)color.G;
		var b = (double)color.B;

		var brightness = Math.Sqrt(0.299 * r * r + 0.587 * g * g + 0.114 * b * b);

		return brightness < 127.5;
	}

	/// <summary>
	/// Converts the given colour to semi-transparent using the given alpha.
	/// </summary>
	public static Argb32 WithAlpha(this Argb32 colour, byte alpha)
	{
		return new Argb32(colour.R, colour.G, colour.B, alpha);
	}

	/// <summary>
	/// Converts the given string to color.
	/// </summary>
	public static Argb32? ToColor(this string raw)
	{
		if (raw.StartsWith("#"))
		{
			// Handle hex strings.
			byte Component16(string component) => (byte)(int.Parse(component, System.Globalization.NumberStyles.HexNumber) * 16);
			byte Component256(string component) => byte.Parse(component, System.Globalization.NumberStyles.HexNumber);

			// Remove # prefix
			raw = raw[1..];

			switch (raw.Length)
			{
				case 3:
				{
					// RGB where each component is represented by single char with value between 0-F
					return new Argb32(
						r: Component16(raw[0..1]),
						g: Component16(raw[1..2]),
						b: Component16(raw[2..3])
					);
				}

				case 4:
				{
					// ARGB where each component is represented by a sincle char with value  between 0-F.
					return new Argb32(
						a: Component16(raw[0..1]),
						r: Component16(raw[1..2]),
						g: Component16(raw[2..3]),
						b: Component16(raw[3..4])
					);
				}

				case 6:
				{
					// RGB where each component is represented by 2 chars with value between 0-FF.
					return new Argb32(
						r: Component256(raw[0..2]),
						g: Component256(raw[2..4]),
						b: Component256(raw[4..6])
					);
				}

				case 8:
				{
					// ARGB where each component is represented by 2 chars with value between 0-FF.
					return new Argb32(
						a: Component256(raw[0..2]),
						r: Component256(raw[2..4]),
						g: Component256(raw[4..6]),
						b: Component256(raw[6..8])
					);
				}
			}
		}
		else
		{
			// Handle components separated by commas.
			byte Component(string component) => byte.Parse(component);

			var components = raw.Split(',');

			switch (components.Length)
			{
				case 3:
				{
					return new Argb32(
						r: Component(components[0]),
						g: Component(components[1]),
						b: Component(components[2])
					);
				}

				case 4:
				{
					return new Argb32(
						a: Component(components[0]),
						r: Component(components[1]),
						g: Component(components[2]),
						b: Component(components[3])
					);
				}
			}
		}

		throw new ArgumentException("Invalid color, use one of `R,G,B`, `A,R,G,B`, `#RGB`, `#ARGB`, `#RRGGBB` or `#AARRGGBB`");
	}
}
