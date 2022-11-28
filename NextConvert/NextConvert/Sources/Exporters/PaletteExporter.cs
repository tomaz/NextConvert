using NextConvert.Sources.Helpers;
using NextConvert.Sources.ImageUtils;

namespace NextConvert.Sources.Exporters;

/// <summary>
/// Exports palette.
/// </summary>
public class PaletteExporter
{
	public IndexedData? Data { get; set; }
	public bool IsPalette9Bit { get; set; } = true;
	public bool IsPaletteCountExported { get; set; } = true;

	#region Public

	public void Export(IStreamProvider streamProvider)
	{
		if (Data == null) throw new InvalidDataException("Data is required, make sure property is assigned");

		using (var writer = new BinaryWriter(streamProvider.GetStream()))
		{
			if (IsPaletteCountExported)
			{
				writer.Write((byte)Data.Colours.Count);
			}

			foreach (var colour in Data.Colours)
			{
				if (IsPalette9Bit)
				{
					foreach (var b in colour.As9BitColour)
					{
						writer.Write(b);
					}
				}
				else
				{
					writer.Write(colour.As8BitColour);
				}
			}
		}
	}

	#endregion
}
