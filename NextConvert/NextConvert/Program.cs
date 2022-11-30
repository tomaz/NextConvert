﻿using NextConvert.Sources.Base;
using NextConvert.Sources.Helpers;
using NextConvert.Sources.Runners;

using System.CommandLine;
using System.CommandLine.Parsing;

return CreateRootCommand().InvokeAsync(args).Result;

#region Command line arguments

FileInfo? CreateExistingFileTestParseArgument(ArgumentResult result)
{
	if (result.Tokens.Count == 0)
	{
		result.ErrorMessage = "Input filename is required!";
		return null;
	}

	var filename = result.Tokens.First().Value;

	if (!File.Exists(filename))
	{
		result.ErrorMessage = $"{filename} doesn't exist!";
		return null;
	}

	return new FileInfo(filename);
}

Command CreateSpritesCommand()
{
	var inputOption = new Option<FileInfo?>(name: "--input", description: "Input image [bmp, png]", parseArgument: CreateExistingFileTestParseArgument) { IsRequired = true };
	var spritesOptions = new Option<FileInfo?>(name: "--sprites", description: "Output raw sprites file [optional]");
	var paletteOption = new Option<FileInfo?>(name: "--palette", description: "Output palette file [optional]");
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
		// Run sprites runner.
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
	var inputOption = new Option<FileInfo?>(name: "--input", description: "Input image [bmp, png]", parseArgument: CreateExistingFileTestParseArgument) { IsRequired = true };
	var spritesOptions = new Option<FileInfo?>(name: "--tiles", description: "Output raw tiles file [optional]");
	var paletteOption = new Option<FileInfo?>(name: "--palette", description: "Output palette file [optional]");

	var result = new Command("tiles", "Converts tile definitions source image into Next hardware format")
	{
		inputOption,
		spritesOptions,
		paletteOption,
	};

	result.SetHandler((input, sprites, palette, globalOptions) =>
	{
		// Run sprites runner.
		Run(() => new TilesRunner
		{
			Globals = globalOptions,
			InputStreamProvider = FileInfoStreamProvider.Create(input),
			OutputTilesStreamProvider = FileInfoStreamProvider.Create(sprites),
			OutputPaletteStreamProvider = FileInfoStreamProvider.Create(palette),
		});
	},
	inputOption,
	spritesOptions,
	paletteOption,
	new GlobalOptionsBinder());

	return result;
}

Command CreateRootCommand()
{
	var result = new RootCommand("Converter for ZX Spectrum Next cross-development formats.");

	result.AddGlobalOption(GlobalOptionsBinder.SheetFilenameOption);
	result.AddGlobalOption(GlobalOptionsBinder.SheetBackgroundOption);
	result.AddGlobalOption(GlobalOptionsBinder.SheetImagesPerRowOption);
	result.AddGlobalOption(GlobalOptionsBinder.SheetColoursPerRowOption);
	result.AddGlobalOption(GlobalOptionsBinder.SheetScaleOption);

	result.AddGlobalOption(GlobalOptionsBinder.KeepTransparentOption);
	result.AddGlobalOption(GlobalOptionsBinder.TransparentOption);
	result.AddGlobalOption(GlobalOptionsBinder.Palette9BitOption);
	result.AddGlobalOption(GlobalOptionsBinder.ExportPaletteCountOption);
	result.AddGlobalOption(GlobalOptionsBinder.ExportIndividualImagesOption);
	result.AddGlobalOption(GlobalOptionsBinder.IgnoreCopiesOption);

	result.AddCommand(CreateSpritesCommand());
	result.AddCommand(CreateTilesCommand());

	return result;
}

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
