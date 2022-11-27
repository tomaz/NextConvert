namespace NextConvert.Sources.ImageUtils;

/// <summary>
/// Image where each pixel is represented as an index into colour palette.
/// </summary>
public class IndexedImage
{
	private int[,] data;

	#region Initialization & Disposal

	public IndexedImage(int width, int height)
	{
		data = new int[height, width];
	}

	#endregion

	#region Public

	/// <summary>
	/// Provides access to the underlying data.
	/// </summary>
	public int this[int x, int y]
	{
		get => data[x, y];
		set => data[x, y] = value;
	}

	#endregion
}
