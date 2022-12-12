using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;

using System.Reflection;

namespace NextConvert.Sources.Helpers;

/// <summary>
/// Font/text rendered.
/// </summary>
public class FontRenderer
{
	/// <summary>
	/// Determines if font rendering is enabled or not.
	/// </summary>
	public bool Enabled { get; private set; } = false;

	private Font? Font { get; set; }
	private TextOptions? Options { get; set; }

	#region Initialization & Disposal

	public FontRenderer(int scale)
	{
		// First try with the font from current location (so users can supply different font for each project).
		var filename = CheckFontOnPath("");
		if (filename == null)
		{
			// If font doesn't exist, check if it's present on the same path executable is. If still no go, exit
			filename = CheckFontOnPath(Assembly.GetExecutingAssembly().Location);
			if (filename == null) return;
		}

		var collection = new FontCollection();
		var family = collection.Add(filename);

		Font = family.CreateFont(9 * scale);
		Options = new TextOptions(Font);
		Enabled = true;
	}

	/// <summary>
	/// Checks if the font is present on the given path (folder only). If present, then the method returns full path with filename included, otherwise returns null
	/// </summary>
	private static string? CheckFontOnPath(string path)
	{
		var folder = Path.GetDirectoryName(path) ?? "";
		var filename = Path.Combine(folder, "font.ttf");

		if (!File.Exists(filename)) return null;

		return filename;
	}

	#endregion

	#region Helpers

	public Size MeasureText(string text)
	{
		if (Font == null) return Size.Empty;

		var bounds = TextMeasurer.MeasureBounds(text, Options!);

		return new Size(width: (int)Math.Ceiling(bounds.Width), (int)Math.Ceiling(bounds.Height));
	}

	public void DrawText(IImageProcessingContext context, string text, Color color, int x, int y)
	{
		if (Font == null) return;

		context.DrawText(text, Font, color, new PointF(x, y));
	}

	#endregion
}
