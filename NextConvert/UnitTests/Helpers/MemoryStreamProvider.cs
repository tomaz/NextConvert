using NextConvert.Sources.Helpers;

namespace UnitTests.Helpers;

public class MemoryStreamProvider : IStreamProvider
{
	public MemoryStream Stream { get; } = new();
	public byte[] Data { get => Stream.ToArray(); }

	private string? ReportedExtension { get; }

	public MemoryStreamProvider(string? reportedExtension = null)
	{
		ReportedExtension = reportedExtension;
	}

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
}
