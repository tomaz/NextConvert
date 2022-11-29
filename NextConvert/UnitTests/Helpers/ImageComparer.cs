using NextConvert.Sources.ImageUtils;

using System.Diagnostics.CodeAnalysis;

namespace UnitTests.Helpers;

public class ImageComparer : IEqualityComparer<ImageData>
{
	public bool Equals(ImageData? a, ImageData? b)
	{
		if (a == null && b == null) return true;
		if (a == null || b == null) return false;

		if (a.Width != b.Width) return false;
		if (a.Height != b.Height) return false;

		for (int y = 0; y < a.Height; y++)
		{
			for (int x = 0; x < a.Width; x++)
			{
				if (a[x, y] != b[x, y]) return false;
			}
		}

		return true;
	}

	public int GetHashCode([DisallowNull] ImageData obj)
	{
		return obj.GetHashCode();
	}
}
