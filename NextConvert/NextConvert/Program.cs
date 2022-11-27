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
	var inputOption = new Option<FileInfo?>(name: "--input", description: "Input image [bmp, png]", parseArgument: CreateExistingFileTestParseArgument);
	var transparentOption = new Option<string?>(name: "--transparent", description: "Transparent colour [optional for transparent png]");
	var spritesOptions = new Option<FileInfo?>(name: "--sprites", description: "Output raw sprites file [optional]");
	var paletteOption = new Option<FileInfo?>(name: "--palette", description: "Output sprites palette file [optional]");
	var sheetOption = new Option<FileInfo?>(name: "--sheet", description: "Generate sprite sheet image [bmp, png]");
	var sheetBackgroundOption = new Option<string?>(name: "--sheet-colour", description: "Sheet image background colour [optional]");
	var sheetScaleOption = new Option<int?>(name: "--sheet-scale", description: "Sheet image scale (1, 2, 3 etc) [optional]");
	var spritesPerRowOption = new Option<int>(name: "--columns", description: "Number of sprite columns for spritesheet", getDefaultValue: () => 16);

	var result = new Command("sprites", "Converts sprites source image")
	{
		inputOption,
		transparentOption,
		spritesOptions, 
		paletteOption, 
		sheetOption,
		sheetScaleOption,
		sheetBackgroundOption,
		spritesPerRowOption,
	};

	result.SetHandler((input, transparent, sprites, palette, sheet, sheetScale, sheetBackground, perRow) =>
	{
		// Run sprites runner.
		Run(() => new SpriteRunner
		{
			InputFilename = input,
			OutputSpritesFilename = sprites,
			PaletteFilename = palette,
			InfoSheetFilename = sheet,
			InfoSheetScale = sheetScale ?? 1,
			InfoSheetBackgroundColour = sheetBackground?.ToColor(),
			TransparentColor = transparent?.ToColor() ?? Color.Transparent,
			SpritesPerRow = perRow,
		});
	},
	inputOption,
	transparentOption,
	spritesOptions,
	paletteOption,
	sheetOption,
	sheetScaleOption,
	sheetBackgroundOption,
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
