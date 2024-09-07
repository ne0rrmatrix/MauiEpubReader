using EpubCore;
using MauiEpubReader.Models;

namespace MauiEpubReader.Services;

public static class BookManagement
{
	public static async Task<Book?> Parse(EpubBook epub, string fileName, CancellationToken cancellationToken = default)
	{
		ICollection<EpubTextFile> html = epub.Resources.Html;
		if (html is null || epub.TableOfContents is null)
		{
			System.Diagnostics.Trace.TraceError("HTML or TOC is null");
			return null;
		}
		try
		{
			string tempFile = Path.GetFileNameWithoutExtension(fileName);
			string coverImagePath = tempFile + ".jpg";
			var coverImagefilePath = await FileService.SaveFile(epub.CoverImage, coverImagePath, cancellationToken);
			Book book = new()
			{
				Chapters = [],
				CoverImage = coverImagefilePath,
				FileName = fileName,
				NumberOfChapters = epub.TableOfContents.Count,
				NumberOfPages = html.Count,
				Title = epub.Title,
			};
			epub.Authors.ToList().ForEach(x => book.Authors.Add(new Author { Name = x }));
			foreach (var item in epub.TableOfContents)
			{
				var htmlFile = html.ToList().Find(x => x.AbsolutePath == item.AbsolutePath);
				if(htmlFile is not null)
				{
					var chapter = new Chapter
					{
						Title = item.Title
					};
					book.TOC.Add(new Toc { Title = item.Title });
					chapter.Html = htmlFile.TextContent;
					chapter.PlainText = Html.GetContentAsPlainText(htmlFile.TextContent) ?? string.Empty;
					book.TOC.Add(new Toc { Title = item.Title });
					book.Chapters.Add(chapter);
				}
			}
			return book;
		}
		catch (Exception ex)
		{
			System.Diagnostics.Trace.TraceError($"Error parsing book: {ex.Message}");
			return null;
		}
	}
	public static  Chapter? GetCurrrentChapter(Book book, int currentPage)
	{
		if (book is null)
		{
			return null;
		}
		return book.Chapters.Find(x => x.StartPage <= currentPage && x.EndPage >= currentPage);
	}
}
