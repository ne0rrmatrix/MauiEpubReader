namespace MauiEpubReader;

using System;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;

public static class StringPaginator
{
	public static List<string> SplitTextIntoPages(string text, double pageHeight, double buttonHeight, double lineHeight)
	{
		var pages = new List<string>();
		var lines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

		double availableHeight = pageHeight - (2 * buttonHeight); // Account for two buttons
		int linesPerPage = (int)(availableHeight / lineHeight);

		var currentPage = new List<string>();
		foreach (var line in lines)
		{
			if (currentPage.Count >= linesPerPage)
			{
				pages.Add(string.Join(Environment.NewLine, currentPage));
				currentPage.Clear();
			}
			currentPage.Add(line);
		}

		if (currentPage.Count > 0)
		{
			pages.Add(string.Join(Environment.NewLine, currentPage));
		}

		return pages;
	}
}