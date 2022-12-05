using NextConvert.Sources.Helpers;

namespace UnitTests.Helpers;

public class MemoryStreamProvider : IStreamProvider
{
	public MemoryStream Stream { get; } = new();

	public byte[] Data { get => Stream.ToArray(); }

	private string? ReportedExtension { get; }

	public MemoryStreamProvider(byte[]? data = null, string? reportedExtension = null)
	{
		if (data != null)
		{
			Stream.Write(data, 0, data.Length);
			Stream.Position = 0;
		}

		ReportedExtension = reportedExtension;
	}

	#region IStreamProvider

	public string? GetExtension()
	{
		return ReportedExtension;
	}

	public Stream GetStream(FileMode fileMode = FileMode.Create)
	{
		return Stream;
	}

	public Stream GetNumberedStream(int number, FileMode fileMode = FileMode.Create)
	{
		return Stream;
	}

	#endregion
}
