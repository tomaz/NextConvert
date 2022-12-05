using NextConvert.Sources.Data;

using System.Diagnostics.CodeAnalysis;

namespace UnitTests.Helpers;

public class TilemapComparer : IEqualityComparer<TilemapData>
{
	public bool Equals(TilemapData? a, TilemapData? b)
	{
		if (a == null && b == null) return true;
		if (a == null || b == null) return false;

		if (a.Width != b.Width) return false;
		if (a.Height != b.Height) return false;

		for (int y = 0; y < a.Height; y++)
		{
			for (int x = 0; x < a.Width; x++)
			{
				var at = a[x, y];
				var bt = b[x, y];

				if (at.Index != bt.Index) return false;
				if (at.FlippedX != bt.FlippedX) return false;
				if (at.FlippedY != bt.FlippedY) return false;
				if (at.RotatedClockwise != bt.RotatedClockwise) return false;
			}
		}

		return true;
	}

	public int GetHashCode([DisallowNull] TilemapData obj)
	{
		return obj.GetHashCode();
	}
}
