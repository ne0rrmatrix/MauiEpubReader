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
				CurrentChapter = 1,
				CurrentPage = 1,
				NumberOfChapters = epub.TableOfContents.Count,
				NumberOfPages = html.Count,
				Title = epub.Title,
			};
			var numberOfPages = 0;
			var toc = new List<Toc>();
			epub.TableOfContents.ToList().ForEach(x => toc.Add(new Toc { Title = x.Title }));
			book.TOC = toc;
			epub.Authors.ToList().ForEach(x => book.Authors.Add(new Author { Name = x }));
			foreach (var item in epub.TableOfContents.Select(x => x.Title))
			{
				var htmlFile = html.ToList().FindAll(x => x.TextContent.Contains(item));
				if (htmlFile == null)
				{
					continue;
				}
				var chapter = new Chapter
				{
					Title = item
				};
				if (htmlFile.Count > 1)
				{

					chapter.Html = htmlFile[1].TextContent;
					chapter.StartPage = numberOfPages + 1;
					chapter.PlainText = Html.GetContentAsPlainText(htmlFile[1].TextContent) ?? string.Empty;
					numberOfPages += htmlFile.Count;
					chapter.EndPage = numberOfPages;
				}
				else
				{
					chapter.Html = htmlFile[0].TextContent;
					chapter.PlainText = Html.GetContentAsPlainText(htmlFile[0].TextContent) ?? string.Empty;
				}
				book.NumberOfPages = numberOfPages;
				book.TOC.Add(new Toc { Title = item });
				book.Chapters.Add(chapter);
			}
			return book;
		}
		catch (Exception ex)
		{
			System.Diagnostics.Trace.TraceError($"Error parsing book: {ex.Message}");
			return null;
		}
	}
}
