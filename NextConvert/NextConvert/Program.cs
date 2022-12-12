using NextConvert.Sources.Base;
using NextConvert.Sources.Helpers;
using NextConvert.Sources.Options;
using NextConvert.Sources.Runners;

using System.CommandLine;
using System.CommandLine.Parsing;

return CreateRootCommand().InvokeAsync(args).Result;

#region Command line arguments

Command CreateSpritesCommand()
{
	var inputSpritesOption = new Option<FileInfo?>(name: "--in-sprites", description: "Input image file (bmp, png)", parseArgument: OptionsUtils.CreateExistingFileTestParseArgument) { IsRequired = true };
	var inputPaletteOption = new Option<FileInfo?>(name: "--in-palette", description: "Input palette (pal, optional, format is 3-RGB bytes per colour)");
	var spritesOptions = new Option<FileInfo?>(name: "--out-sprites", description: "Output raw sprites file (optional)");
	var paletteOption = new Option<FileInfo?>(name: "--out-palette", description: "Output palette file (optional)");
	var is4BitOption = new Option<bool>(name: "--4bit", description: "Generate 4-bit sprites", getDefaultValue: () => false);

	var result = new Command("sprites", "Converts sprites source image into next hardware format")
	{
		inputSpritesOption,
		inputPaletteOption,
		spritesOptions,
		paletteOption,
		is4BitOption,
	};

	result.SetHandler((inSprites, inPalette, sprites, palette, is4Bit, globalOptions) =>
	{
		Run(() => new SpriteRunner
		{
			Globals = globalOptions,
			InputStreamProvider = FileInfoStreamProvider.Create(inSprites),
			OutputSpritesStreamProvider = FileInfoStreamProvider.Create(sprites),
			OutputPaletteStreamProvider = FileInfoStreamProvider.Create(palette),
			IsSprite4Bit = is4Bit,
		});
	},
	inputSpritesOption,
	inputPaletteOption,
	spritesOptions,
	paletteOption,
	is4BitOption,
	new GlobalOptionsBinder());

	return result;
}

Command CreateTilesCommand()
{
	var inputTilesOption = new Option<FileInfo?>(name: "--in-tiles", description: "Input image (bmp, png)", parseArgument: OptionsUtils.CreateExistingFileTestParseArgument) { IsRequired = true };
	var inputPaletteOption = new Option<FileInfo?>(name: "--in-palette", description: "Input palette (pal, optional, format is 3-RGB bytes per colour)");
	var tilesOption = new Option<FileInfo?>(name: "--out-tiles", description: "Output raw tiles file (optional)");
	var paletteOption = new Option<FileInfo?>(name: "--out-palette", description: "Output palette file (optional)");

	var result = new Command("tiles", "Converts tile definitions source image into Next hardware format")
	{
		inputTilesOption,
		inputPaletteOption,
		tilesOption,
		paletteOption,
	};

	result.SetHandler((inTiles, inPalette, sprites, palette, globalOptions) =>
	{
		Run(() => new TilesRunner
		{
			Globals = globalOptions,
			InputStreamProvider = FileInfoStreamProvider.Create(inTiles),
			InputPaletteStreamProvider = FileInfoStreamProvider.Create(inPalette),
			OutputTilesStreamProvider = FileInfoStreamProvider.Create(sprites),
			OutputPaletteStreamProvider = FileInfoStreamProvider.Create(palette),
		});
	},
	inputTilesOption,
	inputPaletteOption,
	tilesOption,
	paletteOption,
	new GlobalOptionsBinder());

	return result;
}

Command CreateTilemapCommand()
{
	var result = new Command("tilemap", "Converts tilemap source into Next hardware format");
	TilemapOptionsBinder.Register(result);

	result.SetHandler((tilemapOptions, globalOptions) =>
	{
		Run(() => new TilemapRunner
		{
			Globals = globalOptions,
			Options = tilemapOptions,
		});
	},
	new TilemapOptionsBinder(),
	new GlobalOptionsBinder());

	return result;
}

Command CreateRootCommand()
{
	var result = new RootCommand("Converter for ZX Spectrum Next raw formats.");

	GlobalOptionsBinder.Register(result);

	result.AddCommand(CreateSpritesCommand());
	result.AddCommand(CreateTilesCommand());
	result.AddCommand(CreateTilemapCommand());

	return result;
}

#endregion

#region Helpers

void Run(Func<BaseRunner> creator)
{
	// The purpose of this function is to catch all creation exception as well as runtime ones.
	try
	{
		creator().Run();
	}
	catch (Exception e)
	{
		Log.Error(e.Message);
	}
}

#endregion
