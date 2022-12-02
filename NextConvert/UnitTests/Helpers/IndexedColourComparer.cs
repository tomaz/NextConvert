using NextConvert.Sources.Data;
using System.Diagnostics.CodeAnalysis;

namespace UnitTests.Helpers;

public class IndexedColourComparer : IEqualityComparer<IndexedData.Colour>
{
	public bool Equals(IndexedData.Colour? a, IndexedData.Colour? b)
	{
		if (a == null && b == null) return false;
		if (a == null || b == null) return false;
		if (a.IsTransparent != b.IsTransparent) return false;
		return a.AsArgb32 == b.AsArgb32;
	}

	public int GetHashCode([DisallowNull] IndexedData.Colour obj)
	{
		return obj.GetHashCode();
	}
}
