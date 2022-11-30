namespace NextConvert.Sources.Helpers;

public interface IStreamProvider
{
	Stream GetStream(FileMode fileMode = FileMode.Create);

	Stream GetNumberedStream(int number, FileMode fileMode = FileMode.Create);

	string? GetExtension();
}

public class FileInfoStreamProvider : IStreamProvider
{
	private FileInfo Info { get; set; }

	#region Initialization & Disposal

	public static IStreamProvider? Create(FileInfo? info) => info != null ? new FileInfoStreamProvider(info) : null;

	private FileInfoStreamProvider(FileInfo info)
	{
		Info = info;
	}

	#endregion

	#region IStreamProvider

	public Stream GetStream(FileMode mode)
	{
		return File.Open(Info.FullName, mode);
	}

	public Stream GetNumberedStream(int number, FileMode mode)
	{
		var directory = Info.DirectoryName;

		var name = Path.GetFileNameWithoutExtension(Info.Name);
		var extension = Info.Extension;
		var filename = $"{name}{number}{extension}";

		if (directory != null)
		{
			return File.Open(Path.Combine(directory, filename), mode);
		}
		else
		{
			return File.Open(filename, mode);
		}
	}

	public string? GetExtension()
	{
		return Path.GetExtension(Info.FullName);
	}

	#endregion

	#region Overrides

	public override string ToString() => Info.FullName;

	#endregion
}
