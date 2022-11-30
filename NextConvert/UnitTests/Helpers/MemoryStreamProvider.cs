using NextConvert.Sources.Helpers;

namespace UnitTests.Helpers;

public class MemoryStreamProvider : IStreamProvider
{
	public MemoryStream Stream { get; } = new();
	public byte[] Data { get => Stream.ToArray(); }

	public string? GetExtension()
	{
		return null;
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
