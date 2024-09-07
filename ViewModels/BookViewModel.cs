using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EpubCore;
using MauiEpubReader.Interfaces;
using MauiEpubReader.Models;

namespace MauiEpubReader.ViewModels;

[QueryProperty("Book", "Book")]
public partial class BookViewModel : ObservableObject
{
	[ObservableProperty]
	Book book;
	public BookViewModel()
	{
		book ??= new Book();

	}
	
}
