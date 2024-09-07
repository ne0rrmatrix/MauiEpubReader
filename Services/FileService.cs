using MetroLog;

namespace MauiEpubReader.Services;

public static class FileService
{
	static readonly ILogger logger = LoggerFactory.GetLogger(nameof(FileService));

	public static void DeleteFile(string fileName)
	{
		try
		{
			if (File.Exists(fileName))
			{
				File.Delete(fileName);
				logger.Info($"Deleted file {fileName}");
			}
		}
		catch (Exception ex)
		{
			logger.Error($"Error deleting file: {fileName}, Messsage: {ex.Message}");
		}
	}

	/// <summary>
	/// Get file name for a string <see cref="string"/>
	/// </summary>
	/// <param name="name">A file name <see cref="string"/></param>
	/// <returns>Filename <see cref="string"/> with file extension</returns>
	public static string GetFileName(string name)
	{
		var filename = Path.GetFileName(name);
		return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), filename);
	}

	/// <summary>
	/// Save file to local storage
	/// </summary>
	/// <param name="result"></param>
	/// <returns></returns>
	public static async Task<string> SaveFile(FileResult result)
	{
		using Stream fileStream = await result.OpenReadAsync();
		using StreamReader reader = new(fileStream);
		string fileName = FileService.GetFileName(result.FileName);
		using FileStream output = File.Create(fileName);
		await fileStream.CopyToAsync(output);
		fileStream.Seek(0, SeekOrigin.Begin);
		FileStream.Synchronized(output);
		logger.Info($"File saved: {fileName}");
		return fileName;
	}
	public static async Task<string> SaveFile(Byte[] bytes, string fileName, CancellationToken cancellationToken)
	{
		var result = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), fileName);
		using var stream = new MemoryStream(bytes);
		if(File.Exists(result))
		{
			logger.Info($"File already exists: {result}");
			return string.Empty;
		}
		await File.WriteAllBytesAsync(result, bytes, cancellationToken).ConfigureAwait(false);
		logger.Info($"Saved file: {result}");
		return result;
	}
}
