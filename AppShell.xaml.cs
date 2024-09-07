using MauiEpubReader.Views;

namespace MauiEpubReader;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute("//BookPage", typeof(BookPage));
		Routing.RegisterRoute("//LibraryPage", typeof(LibraryPage));
	}

}
