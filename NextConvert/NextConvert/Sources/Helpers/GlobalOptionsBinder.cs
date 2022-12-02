using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using System.CommandLine;
using System.CommandLine.Binding;

namespace NextConvert.Sources.Helpers;

/// <summary>
/// Manages global options.
/// </summary>
public class GlobalOptionsBinder : BinderBase<GlobalOptionsBinder.GlobalOptions>
{
	public static readonly Option<FileInfo?> SheetFilenameOption = new(name: "--out-sheet", description: "Generate sprite sheet image (bmp, png)");
	public static readonly Option<string?> SheetBackgroundOption = new(name: "--sheet-background", description: "Info sheet image background colour (optional)");
	public static readonly Option<int> SheetImagesPerRowOption = new(name: "--sheet-image-columns", description: "Number of sprite columns for info sheet", getDefaultValue: () => 16);
	public static readonly Option<int> SheetColoursPerRowOption = new(name: "--sheet-palette-columns", description: "Number of colour columns for info sheet", getDefaultValue: () => 16);
	public static readonly Option<int> SheetScaleOption = new(name: "--sheet-scale", description: "Info sheet image scale (1, 2, 3 etc) [optional]", getDefaultValue: () => 1);

	public static readonly Option<string?> TransparentOption = new(name: "--transparent", description: "Transparent colour (optional for transparent png)");
	public static readonly Option<string> KeepTransparentOption = new Option<string>(name: "--keep-transparent", description: "Specifies what kind of transparent images to keep").FromAmong("none", "boxed", "all");
	public static readonly Option<bool> IgnoreCopiesOption = new(name: "--ignore-copies", description: "Ignore copies, rotated and mirrored images", getDefaultValue: () => false);

	public static readonly Option<bool> Palette9BitOption = new(name: "--9bit-palette", description: "Use 9-bit palette instead of 8", getDefaultValue: () => false);
	public static readonly Option<bool> ExportPaletteCountOption = new(name: "--export-palette-count", description: "Export palette count to the first byte of the file", getDefaultValue: () => false);
	public static readonly Option<bool> ExportIndividualImagesOption = new(name: "--export-images", description: "Export individual images, one per each detected object", getDefaultValue: () => false);

	#region Overrides

	protected override GlobalOptions GetBoundValue(BindingContext bindingContext)
	{
		return new GlobalOptions
		{
			SheetStreamProvider = FileInfoStreamProvider.Create(bindingContext.ParseResult.GetValueForOption(SheetFilenameOption)),
			KeepTransparents = bindingContext.ParseResult.GetValueForOption(KeepTransparentOption)?.ToKeepTransparentType() ?? KeepTransparentType.None,
			TransparentColour = bindingContext.ParseResult.GetValueForOption(TransparentOption)?.ToColor() ?? Color.Transparent,
			SheetBackgroundColour = bindingContext.ParseResult.GetValueForOption(SheetBackgroundOption)?.ToColor(),
			SheetImagesPerRow = bindingContext.ParseResult.GetValueForOption(SheetImagesPerRowOption),
			SheetColoursPerRow = bindingContext.ParseResult.GetValueForOption(SheetColoursPerRowOption),
			SheetScale = bindingContext.ParseResult.GetValueForOption(SheetScaleOption),
			Palette9Bit = bindingContext.ParseResult.GetValueForOption(Palette9BitOption),
			ExportPaletteCount = bindingContext.ParseResult.GetValueForOption(ExportPaletteCountOption),
			ExportIndividualImages = bindingContext.ParseResult.GetValueForOption(ExportIndividualImagesOption),
			IgnoreCopies = bindingContext.ParseResult.GetValueForOption(IgnoreCopiesOption),
		};
	}

	#endregion

	#region Public

	public static void Register(Command command)
	{
		command.AddGlobalOption(SheetFilenameOption);
		command.AddGlobalOption(SheetBackgroundOption);
		command.AddGlobalOption(SheetImagesPerRowOption);
		command.AddGlobalOption(SheetColoursPerRowOption);
		command.AddGlobalOption(SheetScaleOption);

		command.AddGlobalOption(KeepTransparentOption);
		command.AddGlobalOption(TransparentOption);
		command.AddGlobalOption(Palette9BitOption);

		command.AddGlobalOption(ExportPaletteCountOption);
		command.AddGlobalOption(ExportIndividualImagesOption);
		command.AddGlobalOption(IgnoreCopiesOption);
	}

	#endregion

	#region Declarations

	public class GlobalOptions {
		public IStreamProvider? SheetStreamProvider { get; set; }
		public KeepTransparentType KeepTransparents { get; set; }
		public Argb32 TransparentColour { get; set; } = Color.Transparent;
		public Argb32? SheetBackgroundColour { get; set; }
		public int SheetImagesPerRow { get; set; }
		public int SheetColoursPerRow { get; set; }
		public int SheetScale { get; set; }
		public bool Palette9Bit { get; set; }
		public bool ExportPaletteCount { get; set; }
		public bool ExportIndividualImages { get; set; }
		public bool IgnoreCopies { get; set; }
	}

	#endregion
}

#region Declarations

public enum KeepTransparentType
{
	/// <summary>
	/// All fully transparent images will be removed.
	/// </summary>
	None,

	/// <summary>
	/// Transparent images within the bounding box of other images will be kept.
	/// </summary>
	Boxed,

	/// <summary>
	/// All transparent images will be kept.
	/// </summary>
	All
};

internal static class GlobalOptionsExtensions
{
	public static KeepTransparentType ToKeepTransparentType(this string value)
	{
		return Enum.TryParse(value: value, ignoreCase: true, out KeepTransparentType result) ? result : KeepTransparentType.None;
	}
}

#endregion
