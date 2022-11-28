using SixLabors.ImageSharp.PixelFormats;

using System.CommandLine;
using System.CommandLine.Binding;

namespace NextConvert.Sources.Helpers;

/// <summary>
/// Manages global options.
/// </summary>
public class GlobalOptionsBinder : BinderBase<GlobalOptionsBinder.GlobalOptions>
{
	public static readonly Option<FileInfo?> SheetFilenameOption = new(name: "--sheet", description: "Generate sprite sheet image [bmp, png]");
	public static readonly Option<string?> SheetBackgroundOption = new(name: "--sheet-background", description: "Info sheet image background colour [optional]");
	public static readonly Option<int> SheetImagesPerRowOption = new(name: "--sheet-image-columns", description: "Number of sprite columns for info sheet", getDefaultValue: () => 16);
	public static readonly Option<int> SheetColoursPerRowOption = new(name: "--sheet-palette-columns", description: "Number of colour columns for info sheet", getDefaultValue: () => 16);
	public static readonly Option<int> SheetScaleOption = new(name: "--sheet-scale", description: "Info sheet image scale (1, 2, 3 etc) [optional]", getDefaultValue: () => 1);

	public static readonly Option<bool> Palette9BitOption = new(name: "--9-bit-palette", description: "Use 9-bit palette instead of 8", getDefaultValue: () => false);
	public static readonly Option<bool> ExportPaletteCountOption = new(name: "--export-palette-count", description: "Export palette count to the first byte of the file", getDefaultValue: () => false);
	public static readonly Option<bool> IgnoreCopiesOption = new(name: "--ignore-copies", description: "Ignore copies, rotated and mirrored images", getDefaultValue: () => false);
	public static readonly Option<bool> KeepBoxedTransparentsOption = new(name: "--keep-boxed-transparents", description: "Keep transparent images in the middle of image block", getDefaultValue: () => false);

	#region Overrides

	protected override GlobalOptions GetBoundValue(BindingContext bindingContext)
	{
		return new GlobalOptions
		{
			SheetStreamProvider = FileInfoStreamProvider.Create(bindingContext.ParseResult.GetValueForOption(SheetFilenameOption)),
			SheetBackgroundColour = bindingContext.ParseResult.GetValueForOption(SheetBackgroundOption)?.ToColor(),
			SheetImagesPerRow = bindingContext.ParseResult.GetValueForOption(SheetImagesPerRowOption),
			SheetColoursPerRow = bindingContext.ParseResult.GetValueForOption(SheetColoursPerRowOption),
			SheetScale = bindingContext.ParseResult.GetValueForOption(SheetScaleOption),
			Palette9Bit = bindingContext.ParseResult.GetValueForOption(Palette9BitOption),
			ExportPaletteCount = bindingContext.ParseResult.GetValueForOption(ExportPaletteCountOption),
			IgnoreCopies = bindingContext.ParseResult.GetValueForOption(IgnoreCopiesOption),
			KeepBoxedTransparents = bindingContext.ParseResult.GetValueForOption(KeepBoxedTransparentsOption)
		};
	}

	#endregion

	#region Declarations

	public class GlobalOptions {
		public IStreamProvider? SheetStreamProvider { get; set; }
		public Argb32? SheetBackgroundColour { get; set; }
		public int SheetImagesPerRow { get; set; }
		public int SheetColoursPerRow { get; set; }
		public int SheetScale { get; set; }
		public bool Palette9Bit { get; set; }
		public bool ExportPaletteCount { get; set; }
		public bool IgnoreCopies { get; set; }
		public bool KeepBoxedTransparents { get; set; }
	}

	#endregion
}
