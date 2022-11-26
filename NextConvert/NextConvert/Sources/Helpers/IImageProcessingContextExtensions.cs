using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;

namespace NextConvert.Sources.Helpers;

public static class IImageProcessingContextExtensions
{
	public static void SetPixel(this IImageProcessingContext context, Color color, int x, int y)
	{
		context.Fill(color, new RectangleF(x, y, 1, 1));
	}
}
