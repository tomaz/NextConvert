using NextConvert.Sources.Helpers;

using SixLabors.ImageSharp.PixelFormats;

using System.Diagnostics;

namespace NextConvert.Sources.Base;

/// <summary>
/// Base class for our runners. A runner is an object that knows how to convert an input to the corresponding output. Any command line parameters are expected to be passed to constructor, or directly to subclass properties, then call <see cref="Run"/> to operate on them.
/// </summary>
public abstract class BaseRunner
{
	public GlobalOptionsBinder.GlobalOptions Globals { get; set; } = new();

	#region Subclass

	/// <summary>
	/// Called before validation; subclass should describe the parameters.
	/// </summary>
	protected abstract void OnDescribe();

	/// <summary>
	/// Called during validation, subclass should validate all of its parameters.
	/// </summary>
	protected virtual void OnValidate()
	{
		// Default implementation doesn't perform any validation.
	}

	/// <summary>
	/// Runs the converter.
	/// </summary>
	protected abstract void OnRun();

	#endregion

	#region Public

	/// <summary>
	/// Invokes the runner.
	/// </summary>
	public void Run()
	{
		var watch = Stopwatch.StartNew();

		if (Globals.SheetBackgroundColour == null) Globals.SheetBackgroundColour = Globals.TransparentColour;

		OnDescribe();
		OnValidate();
		OnRun();

		watch.Stop();

		Log.Info($"{watch.ElapsedMilliseconds}ms");
	}

	#endregion

	#region Helpers

	/// <summary>
	/// Runs the given task with default logging.
	/// </summary>
	protected static T RunTask<T>(string onStartMessage, Func<T, string> onEndMessage, Func<T> task) 
	{
		Log.Verbose(onStartMessage);

		T result = task();

		Log.Info(onEndMessage(result));

		return result;
	}
	
	/// <summary>
	/// Runs the given task with default logging - variant that doesn't produce result.
	/// </summary>
	protected static void RunTask(string onStartMessage, Func<string> onEndMessage, Action task)
	{
		Log.Verbose(onStartMessage);

		task();

		Log.Info(onEndMessage());
	}

	/// <summary>
	/// This should be called at the end of the <see cref="OnDescribe"/> in subclass to include description of global parameters.
	/// </summary>
	protected void DescribeGlobals()
	{
		Log.Verbose($"Transparent colour: {Globals.TransparentColour} (ARGB)");
		Log.Verbose($"Bits per colour: {(Globals.Palette9Bit ? 9 : 8)}");
		Log.Verbose($"Export palette count: {Globals.ExportPaletteCount}");
		Log.Verbose($"Keep transparents: {Globals.KeepTransparents}");
		Log.Verbose($"Ingore copies/mirrors/rotations: {Globals.IgnoreCopies}");

		if (Globals.SheetStreamProvider != null)
		{
			Log.NewLine();
			Log.Verbose("Sheet options:");
			Log.Verbose($"Background colour: {Globals.SheetBackgroundColour} (ARGB)");
			Log.Verbose($"Sprite columns: {Globals.SheetImagesPerRow}");
			Log.Verbose($"Colour columns: {Globals.SheetColoursPerRow}");
			Log.Verbose($"Scale: {Globals.SheetScale}x");
		}
	}

	#endregion
}
