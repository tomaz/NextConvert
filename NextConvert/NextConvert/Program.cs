using NextConvert.Sources.Base;
using NextConvert.Sources.Helpers;
using NextConvert.Sources.Runners;

using SixLabors.ImageSharp;

using System.CommandLine;
using System.CommandLine.Parsing;

return CreateRootCommand().InvokeAsync(args).Result;

#region Command line arguments

FileInfo? CreateExistingFileTestParseArgument(ArgumentResult result)
{
	if (result.Tokens.Count == 0)
	{
		result.ErrorMessage = "Input file name is required!";
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
	var inputOption = new Option<FileInfo?>(name: "--input", description: "Input image [bmp, png]", parseArgument: CreateExistingFileTestParseArgument);
	var spritesOptions = new Option<FileInfo?>(name: "--sprites", description: "Output raw sprites file [optional]");
	var paletteOption = new Option<FileInfo?>(name: "--palette", description: "Output sprites palette file [optional]");
	var spritesheetOption = new Option<FileInfo?>(name: "--spritesheet", description: "Generate sprite sheet image [bmp, png]");
	var transparentOption = new Option<string?>(name: "--transparent", description: "Transparent colour [optional for transparent png]");
	var spritesPerRowOption = new Option<int>(name: "--columns", description: "Number of sprite columns for spritesheet", getDefaultValue: () => 16);

	var result = new Command("sprites", "Converts sprites source image")
	{
		inputOption, 
		spritesOptions, 
		paletteOption, 
		spritesheetOption,
		transparentOption,
		spritesPerRowOption,
	};

	result.SetHandler((input, sprites, palette, spritesheet, transparent, perRow) =>
	{
		// Run sprites runner.
		Run(() => new SpriteRunner
		{
			InputFilename = input,
			OutputSpritesFilename = sprites,
			PaletteFilename = palette,
			SpriteSheetFilename = spritesheet,
			TransparentColor = transparent?.ToColor() ?? Color.Transparent,
			SpritesPerRow = perRow,
		});
	},
	inputOption,
	spritesOptions,
	paletteOption,
	spritesheetOption,
	transparentOption,
	spritesPerRowOption);

	return result;
}

Command CreateRootCommand()
{
	var result = new RootCommand("Converter for ZX Spectrum Next cross-development formats.");

	result.AddCommand(CreateSpritesCommand());

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
