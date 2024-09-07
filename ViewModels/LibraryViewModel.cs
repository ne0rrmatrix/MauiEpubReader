using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EpubCore;
using MauiEpubReader.Interfaces;
using MauiEpubReader.Models;
using MauiEpubReader.Services;
using MetroLog;

namespace MauiEpubReader.ViewModels;

public partial class LibraryViewModel : ObservableObject
{
	[ObservableProperty]
	bool isBusy;
	static readonly string[] epub = [".epub", ".epub"];
	[ObservableProperty]
	List<Book> books;
	readonly Idb idb;
	static readonly ILogger logger = LoggerFactory.GetLogger(nameof(LibraryViewModel));
	public LibraryViewModel(Idb idb)
	{
		this.idb = idb;
		isBusy = false;
		books = [];
		ThreadPool.QueueUserWorkItem(async (state) => await LoadLibrary().ConfigureAwait(false));
	}

	[RelayCommand]
	public static async Task GotoBookPage(Book Book)
	{
		var navigationParams = new Dictionary<string, object>
		{
			{ "Book", Book }
		};
		await Shell.Current.GoToAsync($"//BookPage", navigationParams).WaitAsync(CancellationToken.None).ConfigureAwait(false);
	}
	[RelayCommand]
	public async Task Add()
	{
		var customFileType = new FilePickerFileType(
				new Dictionary<DevicePlatform, IEnumerable<string>>
				{
					{ DevicePlatform.iOS, epub },
                    { DevicePlatform.Android, epub },
                    { DevicePlatform.WinUI, epub },
                    { DevicePlatform.Tizen, epub },
					{ DevicePlatform.macOS, epub },
                });

		var result = await PickAndShow(new PickOptions
		{
			FileTypes = customFileType,
			PickerTitle = "Please select a epub book"
		});
		if (result is not null)
		{
			var filePath = await FileService.SaveFile(result).ConfigureAwait(false);
			var temp = EpubReader.Read(filePath);
			var book = await BookManagement.Parse(temp, filePath);
			if (book is not null)
			{
				logger.Info("Saving book");
				await idb.SaveBook(book, CancellationToken.None).ConfigureAwait(false);
				logger.Info("Book saved");
				IsBusy = true;
				await LoadLibrary().ConfigureAwait(false);
				return;
			}
			else
			{
				logger.Error("Error parsing book");
			}
			return;
		}
		logger.Error("Error saving book");
	}
	
	[RelayCommand]
	public async Task RemoveBook(Book book)
	{
		if (book is not null)
		{
			logger.Info("Removing book");
			FileService.DeleteFile(book.CoverImage);
			FileService.DeleteFile(book.FileName);
			await idb.RemoveBook(book.Id, CancellationToken.None).ConfigureAwait(false);
			Books.Remove(book);
			logger.Info("Book removed from library.");
			if (Books.Count == 0)
			{
				IsBusy = false;
			}
			OnPropertyChanged(nameof(Books));
		}
	}
	public async Task<List<Book>> GetBooks()
	{
		return await idb.GetBooks(CancellationToken.None).ConfigureAwait(false);
	}

	public async Task<int> UpdateBook(Book book)
	{
		var existingBook = Books.Find(x => x.Id == book.Id);
		if (existingBook is not null && book is not null)
		{
			Books.Remove(existingBook);
			Books.Add(book);
			return await idb.UpdateBook(book, CancellationToken.None).ConfigureAwait(false);
		}
		return 0;
	}
	async Task LoadLibrary()
	{
		var temp = await idb.GetBooks(CancellationToken.None).ConfigureAwait(false);
		if (temp is not null)
		{
			MainThread.BeginInvokeOnMainThread(() => { Books = temp; IsBusy = true; });
			logger.Info("LoadLibrary");
			return;
		}
		logger.Error("Error loading library");
	}
	public static async Task<FileResult?> PickAndShow(PickOptions options)
	{
		try
		{
			return await FilePicker.PickAsync(options).WaitAsync(CancellationToken.None).ConfigureAwait(false);

		}
		catch (Exception ex)
		{
			logger.Error($"Exception choosing file: {ex.Message}");
			return null;
		}
	}
}
