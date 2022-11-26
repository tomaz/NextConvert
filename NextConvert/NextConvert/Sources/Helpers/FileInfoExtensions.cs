namespace NextConvert.Sources.Helpers;

public static class FileInfoExtensions
{
	/// <summary>
	/// Creates a new <see cref="FileInfo"/> with a different extension.
	/// </summary>
	public static FileInfo ReplaceExtension(this FileInfo file, string extension)
	{
		return new FileInfo(Path.ChangeExtension(file.FullName, extension));
	}
}
