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
	var inputOption = new Option<FileInfo?>(name: "--in-sprites", description: "Input image file (bmp, png)", parseArgument: OptionsUtils.CreateExistingFileTestParseArgument) { IsRequired = true };
	var spritesOptions = new Option<FileInfo?>(name: "--out-sprites", description: "Output raw sprites file (optional)");
	var paletteOption = new Option<FileInfo?>(name: "--out-palette", description: "Output palette file (optional)");
	var is4BitOption = new Option<bool>(name: "--4bit", description: "Generate 4-bit sprites", getDefaultValue: () => false);

	var result = new Command("sprites", "Converts sprites source image into next hardware format")
	{
		inputOption,
		spritesOptions,
		paletteOption,
		is4BitOption,
	};

	result.SetHandler((input, sprites, palette, is4Bit, globalOptions) =>
	{
		Run(() => new SpriteRunner
		{
			Globals = globalOptions,
			InputStreamProvider = FileInfoStreamProvider.Create(input),
			OutputSpritesStreamProvider = FileInfoStreamProvider.Create(sprites),
			OutputPaletteStreamProvider = FileInfoStreamProvider.Create(palette),
			IsSprite4Bit = is4Bit,
		});
	},
	inputOption,
	spritesOptions,
	paletteOption,
	is4BitOption,
	new GlobalOptionsBinder());

	return result;
}

Command CreateTilesCommand()
{
	var inputOption = new Option<FileInfo?>(name: "--in-tiles", description: "Input image (bmp, png)", parseArgument: OptionsUtils.CreateExistingFileTestParseArgument) { IsRequired = true };
	var tilesOption = new Option<FileInfo?>(name: "--out-tiles", description: "Output raw tiles file (optional)");
	var paletteOption = new Option<FileInfo?>(name: "--out-palette", description: "Output palette file (optional)");

	var result = new Command("tiles", "Converts tile definitions source image into Next hardware format")
	{
		inputOption,
		tilesOption,
		paletteOption,
	};

	result.SetHandler((input, sprites, palette, globalOptions) =>
	{
		Run(() => new TilesRunner
		{
			Globals = globalOptions,
			InputStreamProvider = FileInfoStreamProvider.Create(input),
			OutputTilesStreamProvider = FileInfoStreamProvider.Create(sprites),
			OutputPaletteStreamProvider = FileInfoStreamProvider.Create(palette),
		});
	},
	inputOption,
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
