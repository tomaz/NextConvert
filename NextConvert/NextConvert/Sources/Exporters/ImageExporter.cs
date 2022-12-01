using NextConvert.Sources.Helpers;
using NextConvert.Sources.Utils;

namespace NextConvert.Sources.Exporters;

/// <summary>
/// Exports a list of images into a single raw file.
/// </summary>
public class ImageExporter
{
	public IndexedData? Data { get; set; }
	public bool Is4BitColour { get; set; } = false;

	#region Public

	public void Export(IStreamProvider streamProvider)
	{
		if (Data == null) throw new InvalidDataException("Data is required, make sure property is assigned");

		using (var writer = new BinaryWriter(streamProvider.GetStream()))
		{
			foreach (var image in Data.Images)
			{
				if (Is4BitColour)
				{
					Export4Bit(writer, image);
				}
				else
				{
					Export8Bit(writer, image);
				}
			}
		}
	}

	#endregion

	#region Helpers

	private void Export4Bit(BinaryWriter writer, IndexedData.Image image)
	{
		for (int y = 0; y < image.Height; y++)
		{
			for (int x = 0; x < image.Width; x += 2)
			{
				var p1 = image[x, y];
				var p2 = image[x + 1, y];
				var combined = (byte)((p1 << 4) | (p2 & 0x0F));
				writer.Write(combined);
			}
		}
	}

	private void Export8Bit(BinaryWriter writer, IndexedData.Image image)
	{
		for (int y = 0; y < image.Height; y++)
		{
			for (int x = 0; x < image.Width; x++)
			{
				writer.Write(image[x, y]);
			}
		}
	}

	#endregion
}
