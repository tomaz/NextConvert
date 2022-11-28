using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using System.Diagnostics.CodeAnalysis;

namespace UnitTests.Helpers;

public class ImageComparer : IEqualityComparer<Image<Argb32>>
{
	public bool Equals(Image<Argb32>? a, Image<Argb32>? b)
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

	public int GetHashCode([DisallowNull] Image<Argb32> obj)
	{
		return obj.GetHashCode();
	}
}
