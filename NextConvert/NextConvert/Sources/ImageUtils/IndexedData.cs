using SixLabors.ImageSharp.PixelFormats;

namespace NextConvert.Sources.ImageUtils;

/// <summary>
/// Represents palette and related list of indexed bitmaps.
/// </summary>
public class IndexedData
{
	/// <summary>
	/// The palette.
	/// </summary>
	public List<Argb32> Palette { get; } = new();

	/// <summary>
	/// Images with indexes into the <see cref="Palette"/>.
	/// </summary>
	public List<IndexedImage> Images { get; } = new();

	#region Public

	/// <summary>
	/// Adds the given color to the end of the list if it's not present in the list yet.
	/// </summary>
	public int AddIfDistinct(Argb32 color)
	{
		var index = Palette.IndexOf(color);
		if (index >= 0) return index;
		
		Palette.Add(color);

		return Palette.Count - 1;
	}

	#endregion
}
