using NextConvert.Sources.Base;
using NextConvert.Sources.Helpers;

using SixLabors.ImageSharp;

namespace NextConvert.Sources.Sprites;

public class SpriteRunner : BaseRunner
{
	public FileInfo? Input { get; set; }
	public FileInfo? Sprites { get; set; }
	public FileInfo? Palette { get; set; }
	public FileInfo? SpriteSheet { get; set; }
	public Color? TransparentColor { get; set; }
	public int SpritesPerRow { get; set; }

	#region Overrides

	protected override void OnDescribe()
	{
		Log.Info("Converting sprites");

		Log.NewLine();
		Log.Verbose("Will parse:");
		Log.Verbose($"{Input}");
		
		Log.NewLine();
		Log.Verbose("Will generate:");
		if (Sprites != null) Log.Verbose($"{Sprites}");
		if (Palette != null) Log.Verbose($"{Palette}");
		if (SpriteSheet != null) Log.Verbose($"{SpriteSheet}");
		
		Log.NewLine();
		Log.Verbose("Options:");
		Log.Verbose($"Transparent color: {TransparentColor} (RGBA)");
		Log.Verbose($"Output sprites per row: {SpritesPerRow}");
		
		Log.NewLine();
	}

	protected override void OnValidate()
	{
		base.OnValidate();

		// If both, output raw file or palette file are missing, there's no point in running anything.
		if (Sprites == null && Palette == null)
		{
			throw new ArgumentException("Either output or palette file should be provided");
		}

		// If transparent colour is missing and image is not png, ignore.
		if (Palette == null && Input!.Extension.ToLower() != "png")
		{
			throw new ArgumentException("Transparent colour is required for non-transparent input file");
		}
	}

	protected override void OnRun()
	{
		var sprites = new ImageSplitter(16, 16).Items(Input!.FullName, TransparentColor!.Value);
		Log.Info($"{sprites.Count()} sprites detected");
	}

	#endregion
}
