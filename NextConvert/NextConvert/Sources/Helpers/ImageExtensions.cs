using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;

namespace NextConvert.Sources.Helpers;

public static class ImageExtensions
{
	/// <summary>
	/// Determines the format based on the file extension and saves the image or throws exception if format is not recognized, or saving fails.
	/// </summary>
	public static void Save<T>(this Image<T> image, IStreamProvider streamProvider, int? number = null) where T : unmanaged, IPixel<T>
	{
		Stream GetStream() => number == null ? streamProvider.GetStream() : streamProvider.GetNumberedStream(number!.Value);

		switch (streamProvider.GetExtension()?.Replace(".", "")?.ToLower())
		{
			case null:
				// Null is used mainly for unit tests to save to memory.
				image.SaveAsBmp(GetStream());
				break;
			case "jpg":
			case "jpeg":
				image.SaveAsJpeg(GetStream());
				break;
			case "bmp":
				image.SaveAsBmp(GetStream());
				break;
			case "png":
				image.SaveAsPng(GetStream());
				break;
			case "gif":
				image.SaveAsGif(GetStream());
				break;
			case "pbm":
				image.SaveAsPbm(GetStream());
				break;
			case "tga":
				image.SaveAsTga(GetStream());
				break;
			case "webp":
				image.SaveAsWebp(GetStream());
				break;
			default:
				throw new InvalidDataException($"Image format {streamProvider.GetExtension()} not supported, use one of `bmp`, `png`, `jpg`, `gif`, `pbm`, `tga`, `webp`");
		}
	}
}
