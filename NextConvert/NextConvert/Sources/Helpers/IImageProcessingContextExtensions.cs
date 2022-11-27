using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;

namespace NextConvert.Sources.Helpers;

public static class IImageProcessingContextExtensions
{
	/// <summary>
	/// Draws a pixel into the given image processing context.
	/// </summary>
	public static void SetPixel(this IImageProcessingContext context, Color colour, int x, int y)
	{
		context.Fill(colour, new RectangleF(x, y, 1, 1));
	}
}
