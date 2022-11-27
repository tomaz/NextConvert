using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;

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

	public FontRenderer()
	{
		var filename = "font.ttf";

		// If font is not present, we will ignore rendering.
		if (!File.Exists(filename)) return;

		var collection = new FontCollection();
		var family = collection.Add(filename);

		Font = family.CreateFont(9);
		Options = new TextOptions(Font);
		Enabled = true;
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
