using CommunityToolkit.Maui.Views;
using MauiEpubReader.Models;
using MauiEpubReader.Services;
using MauiEpubReader.ViewModels;
using MetroLog;
using SharpHook;
using Application = Microsoft.Maui.Controls.Application;

namespace MauiEpubReader.Views;

public partial class BookPage : ContentPage, IDisposable
{
	Label pageCountLabel = new();
	List<string> pages = [];
	int currentPageIndex = 0;
	Book? book;
	readonly TaskPoolGlobalHook hook;
	bool disposedValue;
	static readonly ILogger logger = LoggerFactory.GetLogger(nameof(BookPage));
	public BookPage(BookViewModel viewModel)
	{
		InitializeComponent();
		hook = new TaskPoolGlobalHook();
		hook.RunAsync();
		hook.KeyPressed += Hook_KeyPressed;
		BindingContext = viewModel;
	}

	void DisplayCurrentPage()
	{
		ArgumentNullException.ThrowIfNull(pages);
		ArgumentNullException.ThrowIfNull(book);
		if (currentPageIndex < pages.Count && pages.Count > 0)
		{
			int currentChapter = BookPage.GetChapterNumber(book.Chapters[book.CurrentChapter].Title);
			MainThread.BeginInvokeOnMainThread(() => {TextLabel.Text = pages[currentPageIndex]; pageCountLabel.Text = $"Chapter: {currentChapter}  Page:  {book.CurrentPage} - {book.NumberOfPages}";
				OnPropertyChanged(nameof(pageCountLabel));
			});
		}
		else
		{
			logger.Error("Invalid page index");
		}
	}
	void GotoPage(int page)
	{
		logger.Info($"Page: {page}");
		ArgumentNullException.ThrowIfNull(book);
		book.CurrentPage = page;
		var currenChapter = BookManagement.GetCurrrentChapter(book, page);

		if (currenChapter is not null)
		{
			book.CurrentChapter = book.Chapters.IndexOf(currenChapter);
			int currentChapter = BookPage.GetChapterNumber(book.Chapters[book.CurrentChapter].Title);
			logger.Info($"CurrentChapter: {currentChapter}");
			pages = LoadChapter(currenChapter);
			logger.Info($"Pages: {pages.Count}");
			currentPageIndex = book.CurrentPage - currenChapter.StartPage;
			DisplayCurrentPage();
			return;
		}
		logger.Error("CurrentChapter is null");
	}
	void UpdatePageData()
	{
		ArgumentNullException.ThrowIfNull(book);
		int tempPages = 0;		
		book.Chapters.ForEach(x =>
		{
			x.StartPage = tempPages;
			x.EndPage = LoadChapter(x).Count + x.StartPage;
			tempPages = x.EndPage;
			tempPages++;
		});
		book.NumberOfPages = tempPages;
	}
	void CreateToolBar()
	{
		Shell.Current.ToolbarItems.Clear();
		ArgumentNullException.ThrowIfNull(book);
		var count = 0;
		while (count < book.NumberOfPages)
		{
			var chapter = book.Chapters.Find(x => x.StartPage == count);
			if (chapter is not null)
			{
				var toolbarItem = new ToolbarItem
				{
					Text = chapter.Title,
					Order = ToolbarItemOrder.Secondary,
					Priority = count,
					Command = new Command(() =>
					{
						pages = LoadChapter(chapter);
						book.CurrentPage = chapter.StartPage;
						GotoPage(book.CurrentPage);
					})
				};
				Shell.Current.ToolbarItems.Add(toolbarItem);
			}
			count++;
		}
	}

	static int GetChapterNumber(string title)
	{
		var chapter = title.Replace("CHAPTER", "").Trim();
		if (int.TryParse(chapter, out int result))
		{
			return result;
		}
		return 0;
	}
	void OnTapGestureRecognizerTapped(object sender, TappedEventArgs e)
	{
		ArgumentNullException.ThrowIfNull(book);
		int currentChapter = BookPage.GetChapterNumber(book.Chapters[book.CurrentChapter].Title);
		pageCountLabel = new Label
		{
			FontSize = 12,
			FontAttributes = FontAttributes.Bold,
			Text = $"Chapter: {currentChapter}  Page:  {book.CurrentPage} - {book.NumberOfPages}",
			HorizontalTextAlignment = TextAlignment.Center,
			HorizontalOptions = LayoutOptions.Center
		};
		
		var stacklayout = new StackLayout
		{
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.End,
			WidthRequest = Width - 10,
			HeightRequest = 75
		};
		var slider = new Slider
		{
			Minimum = 0,
			Margin = new Thickness(5),
			Maximum = book.NumberOfPages - 1,
			Value = book.CurrentPage,
			WidthRequest = Width - 10,
			HeightRequest = 50,
			HorizontalOptions = LayoutOptions.Center,
		};
		slider.DragCompleted += slider_DragCompleted;
		stacklayout.Children.Add(pageCountLabel);
		stacklayout.Children.Add(slider);
		var popup = new Popup
		{
			Content = stacklayout,
			VerticalOptions = Microsoft.Maui.Primitives.LayoutAlignment.End,
			HorizontalOptions = Microsoft.Maui.Primitives.LayoutAlignment.Center
		};
		if (AppTheme.Dark == Application.Current?.PlatformAppTheme)
		{
			pageCountLabel.TextColor = Colors.Black;
			pageCountLabel.BackgroundColor = Colors.White;
			stacklayout.BackgroundColor = Colors.Gray;
			popup.Color = Colors.White;
		}
		else
		{
			pageCountLabel.TextColor = Colors.White;
			pageCountLabel.BackgroundColor = Colors.Black;
			stacklayout.BackgroundColor = Colors.Black;
			popup.Color = Colors.Black;
		}
		this.ShowPopup(popup);
	}
	List<string> LoadChapter(Chapter chapter)
	{
		if (chapter is null)
		{
			return [];
		}
		var pageHeight = this.Height - 70;
		var buttonHeight = 50; // Adjust based on your button height
		var lineHeight = 20; // Adjust based on your line height
		return StringPaginator.SplitTextIntoPages(chapter.PlainText, pageHeight, buttonHeight, lineHeight);
	}
	
	void slider_DragCompleted(object? sender, EventArgs e)
	{
		ArgumentNullException.ThrowIfNull(sender);
		ArgumentNullException.ThrowIfNull(book);
		var slider = (Slider)sender;
		book.CurrentPage = (int)slider.Value;
		GotoPage(book.CurrentPage);
	}
	void ContentPage_Loaded(object sender, EventArgs e)
	{
		if(BindingContext is not BookViewModel vm)
		{
			logger.Error("BindingContext is not BookViewModel");
			return;
		}
		book = vm.Book;
		UpdatePageData();
		CreateToolBar();
		GotoPage(1);
	}

	void SwipeGestureRecognizer_Right(object sender, SwipedEventArgs e)
	{
		ArgumentNullException.ThrowIfNull(book);
		if (book.CurrentPage + 1 >= book.NumberOfPages)
		{
			logger.Error("End of book");
			return;
		}
		book.CurrentPage++;
		GotoPage(book.CurrentPage);
	}

	void SwipeGestureRecognizer_Left(object sender, SwipedEventArgs e)
	{
		ArgumentNullException.ThrowIfNull(book);
		if(book.CurrentPage <= 0)
		{
			logger.Error("Start of book");
			return;
		}
		book.CurrentPage--;
		GotoPage(book.CurrentPage);
	}

	void Hook_KeyPressed(object? sender, KeyboardHookEventArgs e)
	{
		switch(e.Data.KeyCode)
		{
			case SharpHook.Native.KeyCode.VcRight:
				var temp = new SwipedEventArgs(this, SwipeDirection.Right);
				SwipeGestureRecognizer_Right(this, temp);
				break;
			case SharpHook.Native.KeyCode.VcLeft:
				temp = new SwipedEventArgs(this, SwipeDirection.Left);
				SwipeGestureRecognizer_Left(this, temp);
				break;
		}
	}
	protected override void OnDisappearing()
	{
		hook.Dispose();
		logger.Info("Dissapearing");
	}
	protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
	{
		base.OnNavigatedFrom(args);
		hook.Dispose();
		logger.Info("NavigatedFrom");
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				System.Diagnostics.Trace.TraceInformation("Disposing");
				hook.Dispose();
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

