using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Input;
using MauiEpubReader.Models;
using MauiEpubReader.Services;
using MauiEpubReader.ViewModels;
using MetroLog;
using SharpHook;

namespace MauiEpubReader.Views;

public partial class BookPage : ContentPage
{
	int chapterOffset = 0;
	Label pageCountLabel = new();
	List<string> pages = [];
	int currentPageIndex = 0;
	Book? book;
	readonly IKeyboardServices? keyboardServices;
	static readonly ILogger logger = LoggerFactory.GetLogger(nameof(BookPage));
	public BookPage(BookViewModel viewModel, IKeyboardServices keyboardServices)
	{
		InitializeComponent();
		this.keyboardServices = keyboardServices;
		BindingContext = viewModel;
		ArgumentNullException.ThrowIfNull(keyboardServices);
		keyboardServices.Hook.KeyPressed += Hook_KeyPressed;
	}

	void DisplayCurrentPage()
	{
		ArgumentNullException.ThrowIfNull(pages);
		ArgumentNullException.ThrowIfNull(book);
		if (currentPageIndex < pages.Count && pages.Count > 0)
		{
			MainThread.BeginInvokeOnMainThread(() => {TextLabel.Text = pages[currentPageIndex]; pageCountLabel.Text = $"Chapter: {book.CurrentChapter}  Page:  {book.CurrentPage} - {book.NumberOfPages}";
				OnPropertyChanged(nameof(pageCountLabel));
			});
		}
		else
		{
			logger.Error("Invalid page index");
		}
	}
	void GotoPage(int index)
	{
		if (index >= 0 && index < pages.Count)
		{
			currentPageIndex = index;
			DisplayCurrentPage();
		}
		else
		{
			ArgumentNullException.ThrowIfNull(book);
			var currentChapter = GetCurrrentChapter(book.CurrentPage);
			if (currentChapter is not null)
			{
				book.CurrentChapter = book.Chapters.IndexOf(currentChapter);
				pages = LoadChapter(currentChapter);
				currentPageIndex = book.CurrentPage - currentChapter.StartPage;
				if (currentPageIndex < 0)
				{
					logger.Error("Invalid page index");
					currentPageIndex = 0;
				}
				GotoPage(currentPageIndex);
			}
			else
			{
				logger.Error("Invalid page index");
			}
		}
	}
	public void LoadBook(Book ebook)
	{
		book = ebook;
		int count = 0;
		book.Chapters.ForEach(x =>
		{
			if(!x.Title.Contains("CHAPTER"))
			{
				chapterOffset++;
				return;
			}
			x.StartPage = count;
			count += PageCount(x.PlainText, (int)Height) ?? 0;
			x.EndPage = count;
		});
		book.NumberOfPages = count;
		var currentChapter = ebook.CurrentChapter;
		var temp= ebook.Chapters.FindAll(x => x.Title == $"CHAPTER {currentChapter}");
		if (temp is null)
		{
			logger.Error("Chapter not found");
			return;
		}
		List<Chapter>? chapter = ebook.Chapters.FindAll(x => x.Title == $"CHAPTER {currentChapter}");
		pages = SplitTextIntoPages(chapter?[0].PlainText);
		currentPageIndex = 0;
		book.CurrentPage = 1;
		CreateToolBar();
		GotoPage(1);
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
						currentPageIndex = 0;
						book.CurrentPage = chapter.StartPage;
						GotoPage(0);
					})
				};
				Shell.Current.ToolbarItems.Add(toolbarItem);
			}
			count++;
		}
	}
	static int? PageCount(string? longText, int pageHeight)
	{
		if (string.IsNullOrEmpty(longText))
		{
			return null;
		}

		var buttonHeight = 50; // Adjust based on your button height
		var lineHeight = 20; // Adjust based on your line height
		return StringPaginator.SplitTextIntoPages(longText, pageHeight, buttonHeight, lineHeight).Count;
	}
	List<string> SplitTextIntoPages(string? longText)
	{
		if (string.IsNullOrEmpty(longText))
		{
			return [];
		}
		var pageHeight = this.Height - 70;
		var buttonHeight = 50; // Adjust based on your button height
		var lineHeight = 20; // Adjust based on your line height

		return StringPaginator.SplitTextIntoPages(longText, pageHeight, buttonHeight, lineHeight);
	}
	void OnTapGestureRecognizerTapped(object sender, TappedEventArgs e)
	{
		ArgumentNullException.ThrowIfNull(book);
		pageCountLabel = new Label
		{
			FontSize = 12,
			FontAttributes = FontAttributes.Bold,
			Text = $"Chapter: {book.CurrentChapter}  Page:  {book.CurrentPage} - {book.NumberOfPages}",
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
		return SplitTextIntoPages(chapter.PlainText);
	}
	Chapter? GetCurrrentChapter(int currentPage)
	{
		if (book is null)
		{
			return null;
		}
		return book.Chapters.Find(x => x.StartPage <= currentPage && x.EndPage >= currentPage);
	}
	void slider_DragCompleted(object? sender, EventArgs e)
	{
		ArgumentNullException.ThrowIfNull(sender);
		ArgumentNullException.ThrowIfNull(book);
		var slider = (Slider)sender;
		book.CurrentPage = (int)slider.Value;
		var currenChapter = GetCurrrentChapter(book.CurrentPage);
		
		if (currenChapter is not null)
		{
			book.CurrentChapter = GetCurrentChapterNumber(currenChapter);
			pages = LoadChapter(currenChapter);
			currentPageIndex = book.CurrentPage - currenChapter.StartPage;
			GotoPage(currentPageIndex);
			pageCountLabel.Text = $"Chapter: {book.CurrentChapter}  Page:  {book.CurrentPage} - {book.NumberOfPages}";
			OnPropertyChanged(nameof(pageCountLabel));
		}
	}

	int GetCurrentChapterNumber(Chapter chapter)
	{
		ArgumentNullException.ThrowIfNull(book);
		return book.Chapters.IndexOf(chapter) - chapterOffset + 1;
	}
	void ContentPage_Loaded(object sender, EventArgs e)
	{
		if(BindingContext is not BookViewModel vm)
		{
			logger.Error("BindingContext is not BookViewModel");
			return;
		}
		LoadBook(vm.Book);
	}

	void SwipeGestureRecognizer_Right(object sender, SwipedEventArgs e)
	{
		ArgumentNullException.ThrowIfNull(book);
		if(book.CurrentPage + 1 < book.NumberOfPages)
		{
			book.CurrentPage++;
			if (currentPageIndex + 1 < pages.Count)
			{
				currentPageIndex++;
				GotoPage(currentPageIndex);
				return;
			}
			var currentChapter = GetCurrrentChapter(book.CurrentPage);
			if (currentChapter is not null)
			{
				book.CurrentChapter = GetCurrentChapterNumber(currentChapter);
				pages = LoadChapter(currentChapter);
				currentPageIndex = 0;
				GotoPage(currentPageIndex);
				return;
			}
			logger.Error("Invalid page index");
		}
	}

	void SwipeGestureRecognizer_Left(object sender, SwipedEventArgs e)
	{
		ArgumentNullException.ThrowIfNull(book);
		
		if (book.CurrentPage - 1 > 0)
		{
			book.CurrentPage--;
			if (currentPageIndex - 1 >= 0)
			{
				currentPageIndex--;
				GotoPage(currentPageIndex);
				return;
			}
			var currentChapter = GetCurrrentChapter(book.CurrentPage);
			if (currentChapter is not null)
			{
				book.CurrentChapter = GetCurrentChapterNumber(currentChapter);
				pages = LoadChapter(currentChapter);
				currentPageIndex = pages.Count - 1;
				GotoPage(currentPageIndex);
				return;
			}
			logger.Error("Invalid page index");
		}
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
	~BookPage()
	{
		ArgumentNullException.ThrowIfNull(keyboardServices);
		keyboardServices.Hook.KeyPressed -= Hook_KeyPressed;
	}
}

