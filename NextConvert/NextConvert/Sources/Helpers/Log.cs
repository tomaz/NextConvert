namespace NextConvert.Sources.Helpers;

/// <summary>
/// Wrapper for <see cref="Console"/>. Automates console colors.
/// </summary>
public static class Log
{
	/// <summary>
	/// Writes the given error level message.
	/// </summary>
	public static void Error(string message)
	{
		WriteLine(null, ConsoleColor.Red, message);
	}

	/// <summary>
	/// Writes the given info level message.
	/// </summary>
	public static void Info(string message)
	{
		WriteLine(null, ConsoleColor.White, message);
	}

	/// <summary>
	/// Writes the given info verbose level message.
	/// </summary>
	public static void Verbose(string message)
	{
		WriteLine(null, ConsoleColor.DarkGray, message);
	}

	/// <summary>
	/// Writes an empty line.
	/// </summary>
	public static void NewLine()
	{
		Console.WriteLine();
	}

	/// <summary>
	/// Writes the given text using given background and foreground colors, then restores both colors to originals.
	/// </summary>
	/// <param name="background">Background color or null to use current one.</param>
	/// <param name="foreground">Foreground color or null to use current one.</param>
	/// <param name="message">Message to write.</param>
	public static void Write(ConsoleColor? background, ConsoleColor? foreground, string message)
	{
		WithColors(background, foreground, () => Console.Write(message));
	}

	/// <summary>
	/// Writes the given text and new line using given background and foreground colors, then restores both colors to originals.
	/// </summary>
	/// <param name="background">Background color or null to use current one.</param>
	/// <param name="foreground">Foreground color or null to use current one.</param>
	/// <param name="message">Message to write.</param>
	public static void WriteLine(ConsoleColor? background, ConsoleColor? foreground, string message)
	{
		WithColors(background, foreground, () => Console.WriteLine(message));
	}

	#region Helpers

	private static void WithColors(ConsoleColor? background, ConsoleColor? foreground, Action action)
	{
		var originalForeground = Console.ForegroundColor;
		var originalBackground = Console.BackgroundColor;

		if (foreground != null) Console.ForegroundColor = foreground.Value;
		if (background != null) Console.BackgroundColor = background.Value;

		action();

		Console.ForegroundColor = originalForeground;
		Console.BackgroundColor = originalBackground;
	}

	#endregion
}
