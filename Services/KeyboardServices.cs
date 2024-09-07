using SharpHook;
namespace MauiEpubReader.Services;
public class KeyboardServices : IKeyboardServices, IDisposable
{
	readonly TaskPoolGlobalHook hook;
	bool disposedValue;

	public IGlobalHook Hook { get => hook; }
	public KeyboardServices()
	{
		hook = new TaskPoolGlobalHook();
		_ = InitKeyboardooks();
	}
	async Task InitKeyboardooks()
	{
		await hook.RunAsync();
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				hook?.Dispose();
			}

			disposedValue = true;
		}
	}
	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}

public interface IKeyboardServices
{
	IGlobalHook Hook { get; }
}