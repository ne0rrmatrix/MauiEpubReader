using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Storage;
using FFImageLoading.Maui;
using MauiEpubReader.Database;
using MauiEpubReader.Interfaces;
using MauiEpubReader.Services;
using MauiEpubReader.ViewModels;
using MauiEpubReader.Views;
using MetroLog;
using MetroLog.Operators;
using MetroLog.Targets;

namespace MauiEpubReader;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>().UseMauiCommunityToolkitCore().UseMauiCommunityToolkit().UseFFImageLoading()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});
		var config = new LoggingConfiguration();
#if RELEASE
    config.AddTarget(
        LogLevel.Info, 
        LogLevel.Fatal, 
        new StreamingFileTarget(retainDays: 2);
#else
		// Will write logs to the Debug output
		config.AddTarget(
			LogLevel.Trace,
			LogLevel.Fatal,
			new TraceTarget());
#endif

		// will write logs to the console output (Logcat for android)
		config.AddTarget(
			LogLevel.Info,
			LogLevel.Fatal,
			new ConsoleTarget());

		config.AddTarget(
			LogLevel.Info,
			LogLevel.Fatal,
			new MemoryTarget(2048));

		// Register the FolderPicker as a singleton
		builder.Services.AddSingleton<IFolderPicker>(FolderPicker.Default);
		builder.Services.AddSingleton<IFilePicker>(FilePicker.Default);
		builder.Services.AddSingleton<Idb, Db>();
		LoggerFactory.Initialize(config);
		builder.Services.AddSingleton(LogOperatorRetriever.Instance);

		builder.Services.AddTransient<BookPage>();
		builder.Services.AddTransient<BookViewModel>();
		builder.Services.AddSingleton<LibraryPage>();
		builder.Services.AddSingleton<LibraryViewModel>();
		
		return builder.Build();
	}
}
