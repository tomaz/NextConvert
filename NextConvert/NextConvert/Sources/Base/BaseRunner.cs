using NextConvert.Sources.Helpers;

using SixLabors.ImageSharp.PixelFormats;

using System.Diagnostics;
using System.Runtime.Serialization;

namespace NextConvert.Sources.Base;

/// <summary>
/// Base class for our runners. A runner is an object that knows how to convert an input to the corresponding output. Any command line parameters are expected to be passed to constructor, or directly to subclass properties, then call <see cref="Run"/> to operate on them.
/// </summary>
public abstract class BaseRunner
{
	public FileInfo? InfoSheetFilename { get; set; }
	public Argb32? InfoSheetBackgroundColour { get; set; }
	public Argb32? TransparentColor { get; set; }

	#region Public

	/// <summary>
	/// Invokes the runner.
	/// </summary>
	public void Run()
	{
		var watch = Stopwatch.StartNew();

		if (InfoSheetBackgroundColour == null) InfoSheetBackgroundColour = TransparentColor;

		OnDescribe();
		OnValidate();
		OnRun();

		watch.Stop();

		Log.Info($"{watch.ElapsedMilliseconds}ms");
	}

	#endregion

	#region Overrides

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
}
