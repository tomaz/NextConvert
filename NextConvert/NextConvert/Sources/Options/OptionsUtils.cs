using System.CommandLine.Parsing;

namespace NextConvert.Sources.Options;

public static class OptionsUtils
{
	public static  FileInfo? CreateExistingFileTestParseArgument(ArgumentResult result)
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


}
