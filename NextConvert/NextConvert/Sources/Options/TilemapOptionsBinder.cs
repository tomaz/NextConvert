using System.CommandLine;
using System.CommandLine.Binding;
using NextConvert.Sources.Helpers;

namespace NextConvert.Sources.Options;

public class TilemapOptionsBinder : BinderBase<TilemapOptionsBinder.TilemapOptions>
{
    public static readonly Option<FileInfo?> InTilemapOption = new(name: "--in-tilemap", description: "Input tilemap file (gba, map, stm, txm, txt)", parseArgument: OptionsUtils.CreateExistingFileTestParseArgument) { IsRequired = true };
    public static readonly Option<FileInfo?> InTilesOption = new(name: "--in-tiles", description: "Input tile definitions file (bmp, png)");

    public static readonly Option<FileInfo?> OutTilemapOption = new(name: "--out-tilemap", description: "Output raw tilemap file");
    public static readonly Option<FileInfo?> OutTilesOption = new(name: "--out-tiles", description: "Output raw tile definitions file");
    public static readonly Option<FileInfo?> OutPaletteOption = new(name: "--out-palette", description: "Output raw palette file");

    public static readonly Option<int> TransparentTileOption = new(name: "--transparent-tile", description: "Transparent tile index", getDefaultValue: () => 0);
    public static readonly Option<bool> OptimizedOption = new(name: "--optimized", description: "Export optimized tilemap", getDefaultValue: () => false);
    public static readonly Option<bool> OneBytePerTileOption = new(name: "--one-byte", description: "Exclude tile attribute", getDefaultValue: () => false);

    #region Overrides

    protected override TilemapOptions GetBoundValue(BindingContext bindingContext)
    {
        return new TilemapOptions
        {
            InputTilemapStreamProvider = FileInfoStreamProvider.Create(bindingContext.ParseResult.GetValueForOption(InTilemapOption)),
            InputTilesStreamProvider = FileInfoStreamProvider.Create(bindingContext.ParseResult.GetValueForOption(InTilesOption)),

            OutputTilemapStreamProvider = FileInfoStreamProvider.Create(bindingContext.ParseResult.GetValueForOption(OutTilemapOption)),
            OutputTilesStreamProvider = FileInfoStreamProvider.Create(bindingContext.ParseResult.GetValueForOption(OutTilesOption)),
            OutputPaletteStreamProvider = FileInfoStreamProvider.Create(bindingContext.ParseResult.GetValueForOption(OutPaletteOption)),

            TransparentTileIndex = bindingContext.ParseResult.GetValueForOption(TransparentTileOption),
            UseOptimizedOutput = bindingContext.ParseResult.GetValueForOption(OptimizedOption),
            UseAttributeByte = !bindingContext.ParseResult.GetValueForOption(OneBytePerTileOption),
        };
    }

    #endregion

    #region Public

    public static void Register(Command command)
    {
        command.AddOption(InTilemapOption);
        command.AddOption(InTilesOption);

        command.AddOption(OutTilemapOption);
        command.AddOption(OutTilesOption);
        command.AddOption(OutPaletteOption);

        command.AddOption(TransparentTileOption);
        command.AddOption(OptimizedOption);
        command.AddOption(OneBytePerTileOption);
    }

    #endregion

    #region Declarations

    public class TilemapOptions
    {
        public IStreamProvider? InputTilemapStreamProvider { get; set; }
        public IStreamProvider? InputTilesStreamProvider { get; set; }
		public IStreamProvider? InputPaletteStreamProvider { get; set; }

        public IStreamProvider? OutputTilemapStreamProvider { get; set; }
        public IStreamProvider? OutputTilesStreamProvider { get; set; }
        public IStreamProvider? OutputPaletteStreamProvider { get; set; }

        public int TransparentTileIndex { get; set; } = 0;
        public bool UseAttributeByte { get; set; } = true;
        public bool UseOptimizedOutput { get; set; } = false;
    }

    #endregion
}
