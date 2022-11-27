using NextConvert.Sources.ImageUtils;

namespace NextConvert.Sources.Exporters;

/// <summary>
/// Exports palette.
/// </summary>
public class PaletteExporter
{
	public IndexedData? Data { get; set; }
	public bool IsPaletteCountExported { get; set; } = true;
	public bool IsPalette9Bit { get; set; } = true;


	#region Public

	public void Export(FileInfo filename)
	{
		if (Data == null) throw new InvalidDataException("Data is required, make sure property is assigned");

		using (var writer = new BinaryWriter(File.Open(filename.FullName, FileMode.Create)))
		{
			if (IsPaletteCountExported)
			{
				writer.Write((byte)Data.Palette.Count);
			}

			foreach (var colour in Data.Palette)
			{
				if (IsPalette9Bit)
				{
					var r = colour.R.Component(3);
					var g = colour.G.Component(3);
					var b = colour.B.Component(3);

					var rgb = (byte)(r << 5 | g << 2 | b >> 1);
					var xxb = (byte)(b & 0b00000001);

					writer.Write(rgb);
					writer.Write(xxb);
				}
				else
				{
					var r = colour.R.Component(3);
					var g = colour.G.Component(3);
					var b = colour.B.Component(2);

					var rgb = (byte)(r << 5 | g << 2 | b);

					writer.Write(rgb);
				}
			}
		}
	}

	#endregion
}

#region Extensions

internal static class PaletteExporterExtensions
{
	internal static byte Component(this byte original, byte bits)
	{
		var multiple = (decimal)Math.Pow(2, bits) - 1;
		return (byte)Math.Round((decimal)original * multiple / 255);
	}
}

#endregion
