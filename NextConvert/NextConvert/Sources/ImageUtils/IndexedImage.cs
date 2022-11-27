namespace NextConvert.Sources.ImageUtils;

/// <summary>
/// Image where each pixel is represented as an index into colour palette.
/// </summary>
public class IndexedImage
{
	public int Width { get; private set; }
	public int Height { get; private set; }

	private byte[,] data;

	#region Initialization & Disposal

	public IndexedImage(int width, int height)
	{
		Width = width;
		Height = height;
		data = new byte[height, width];
	}

	#endregion

	#region Public

	/// <summary>
	/// Provides access to the underlying data.
	/// </summary>
	public byte this[int x, int y]
	{
		get => data[y, x];
		set => data[y, x] = value;
	}

	#endregion
}
